using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using DG.Tweening;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardSO;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; }

    [Header("References")]
    [SerializeField] private TarvernDeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private Button confirmButton;
    [SerializeField] private DamageManager damageManager;

    [Header("Spawn Points")]
    [SerializeField] private Transform tavernSpawnPoint;
    public Transform TavernSpawnPoint => tavernSpawnPoint;
    public Transform graveyardSpawnPoint;
    [SerializeField] private Transform playerHitPoint;

    public Transform PlayerHitPoint => playerHitPoint;
    public HandManager Hand => handManager;
    private bool handWasEmptyAfterPlay;
    private bool waitingForInventory;
    private bool extraAttackUsed = false;
    private bool waitingExtraAttack = false;
    private bool waitingExtraDiscard = false;
    private CardSO rewardK4Card;
    public int LastDiscardOverflow { get; set; }
    public bool IsExtraAttack => waitingExtraAttack;
    public int LastDrawAmount { get; private set; }

    public CardSO.Suit OverrideSuit = CardSO.Suit.None;

    private bool hasPendingJokerEnding = false;
    private bool pendingBadEnding = false;
    private bool jesterInstantKill = false;
    private bool handlingBossDeath = false;

    private BossTraitSO jesterResetDisabledTrait;
    private CardSO.Suit jesterResetDisabledResistance;
    private BossSO jesterResetBoss;

    public void ContinueFromInventory()
    {
        if (PlayerReward.Instance != null &&
            PlayerReward.Instance.HasAnyRewardEquipped())
        {
            PlayerReward.Instance.MarkTraitHasAdd();

            if (JesterManager.Instance != null)
            {
                JesterManager.Instance.RevokeJesters();
            }
        }

        waitingForInventory = false;
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ChangeState(BattleState.StartBattle);
    }

    #region State Machine

    public void ChangeState(BattleState newState)
    {
        CurrentState = newState;

        switch (CurrentState)
        {
            case BattleState.StartBattle:
                StartBattle();
                break;

            case BattleState.PlayerTurn:
                StartPlayerTurn();
                break;

            case BattleState.BossTurn:
                StartBossTurn();
                break;

            case BattleState.ResolveAttack:
                StartCoroutine(ResolveBossAttack());
                break;

            case BattleState.CheckBattle:
                CheckBattle();
                break;

            case BattleState.Victory:
                Victory();
                break;

            case BattleState.Defeat:
                Defeat();
                break;
        }
    }

    #endregion
    public void StartBattleAfterTraitSelected()
    {
        ChangeState(BattleState.PlayerTurn);
    }
    #region Battle Flow

    private void StartBattle()
    {
        handlingBossDeath = false;
        jesterResetDisabledTrait = null;
        jesterResetBoss = null;
        extraAttackUsed = false;
        waitingExtraAttack = false;

        if (BossManager.Instance.CurrentBoss == null)
        {
            BossManager.Instance.Initialize();
            handManager.RevealAllHiddenCards();
        }
        else
        {
            BossManager.Instance.RefreshBossInfo();
        }

        SaveData save = SaveManager.Instance.LoadProgress();

        if (save != null)
        {
            deckManager.LoadDeck(save.deckCards);

            GraveyardManager.Instance.LoadData(
                save.graveyardCards);

            handManager.LoadHand(
                save.handCards);

            if (BossManager.Instance.CurrentBoss != null &&
                BossManager.Instance.CurrentBoss.isJoker)
            {
                ChangeState(BattleState.PlayerTurn);
            }
        }
        else
        {
            StartCoroutine(StartFirstBattleRoutine());
        }
    }
    private IEnumerator StartFirstBattleRoutine()
    {
        yield return StartCoroutine(
            deckManager.AddCardFromFirst()
        );

        if (BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.isJoker)
        {
            ChangeState(BattleState.PlayerTurn);
        }
    }
    private void StartPlayerTurn()
    {
        handlingBossDeath = false;
        waitingExtraAttack = false;
        extraAttackUsed = false;

        handManager.handCards.RemoveAll(
            card => card == null);

        BossSO boss = BossManager.Instance.CurrentBoss;

        if (boss != null && boss.isJoker)
        {
            BossManager.Instance.RandomizeJokerDisguise();
            SoundManager.instance.PlaySound2D(boss.spawnSoundID);
        }

        handManager.SetInteractable(false);

        TraitManager.Instance.InvokeBossEvent(
            TraitEventType.PlayerTurn,
            0);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.PlayerTurn,
            0);

        handManager.SetInteractable(true);

        if (handManager.handCards.Count == 0)
        {
            ChangeState(BattleState.Defeat);
            return;
        }

        TurnUIController.Instance.ShowYourTurn();

        InteractConfirmButton(true);
    }

    private void StartBossTurn()
    {
        TraitManager.Instance.InvokeBossEvent(TraitEventType.BossTurn, 0);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.BossTurn, 0);

        TurnUIController.Instance.ShowEnemyTurn();

        DOVirtual.DelayedCall(1.2f, () =>
        {
            ChangeState(BattleState.ResolveAttack);
        });
    }

    private IEnumerator ResolveBossAttack()
    {
        handManager.CancelCurrentSelection();

        yield return StartCoroutine(
            BossFXManager.Instance.PlayBossAttackFX());

        int damage = BossManager.Instance.CurrentATK;
        damage = Mathf.Max(0, damage);

        yield return StartCoroutine(damageManager.ShowTakenDamage(damage));

        if (damage <= 0)
        {
            yield return StartCoroutine(
                BossFXManager.Instance.PlayBlockSuccessFX());

            ChangeState(BattleState.PlayerTurn);
            yield break;
        }

        TurnUIController.Instance.ShowDiscardTurn();

        yield return new WaitForSeconds(0.6f);

        handManager.StartDiscardPhase(damage);
    }
    private void CheckBattle()
    {
        InteractConfirmButton(false);

        if (BossManager.Instance.IsDead())
        {
            if (handlingBossDeath)
                return;

            StartCoroutine(HandleBossDeath());
            return;
        }

        ChangeState(BattleState.BossTurn);
    }

    private IEnumerator HandleBossDeath()
    {
        handlingBossDeath = true;

        handManager.UnlockCard();
        handManager.SetInteractable(false);
        handManager.RevealAllHiddenCards();

        BossSO deadBoss =
            BossManager.Instance.CurrentBoss;

        if (deadBoss != null &&
            deadBoss == jesterResetBoss)
        {
            if (jesterResetDisabledTrait != null)
            {
                deadBoss.currentTrait = jesterResetDisabledTrait;

                Debug.Log(
                    $"[JESTER RESET] Restored defeated boss trait: " +
                    $"{deadBoss.currentTrait.name}");
            }

            deadBoss.resistanceSuit = jesterResetDisabledResistance;
        }

        int oldStageIndex =
            BossManager.Instance.CurrentStageIndex;

        BossManager.Instance.BossDisplay.ResetBossSprite();

        yield return BossFXManager.Instance.PlayDeathFX(
            BossManager.Instance.BossTransform);

        if (BossEliminatedUI.Instance != null)
        {
            Debug.Log("BossEleminatePlay");
            yield return BossEliminatedUI.Instance.Play();
        }

        Transform target =
            BossManager.Instance.LastKillWasPerfect
                ? tavernSpawnPoint
                : graveyardSpawnPoint;

        yield return BossFXManager.Instance.PlayCollectRewardFX(
            BossManager.Instance.BossDisplay,
            deadBoss.bossCard,
            target);

        BossManager.Instance.OnBossDefeated(
            deadBoss);

        if (deadBoss.currentTrait != null)
        {
            PlayerReward.Instance.AddReward(
                deadBoss.currentTrait.reward);
        }

        jesterResetDisabledTrait = null;
        jesterResetBoss = null;

        bool hasNextBoss =
            BossManager.Instance.HasMoreBosses;

        if (!hasNextBoss)
        {
            if (deadBoss.rank == BossRank.Joker)
            {
                hasPendingJokerEnding = true;

                pendingBadEnding =
                    PlayerReward.Instance != null &&
                    PlayerReward.Instance.TraitHasAdd;


                ChangeState(BattleState.Victory);
                yield break;
            }
        }
        waitingForInventory = true;

        LevelManager.instance.LoadSceneAdditive(
            "InventoryScene");

        yield return new WaitUntil(
            () => !waitingForInventory);

        LevelManager.instance.UnloadSceneAdditive(
            "InventoryScene");

        int newStageIndex =
            BossManager.Instance.CurrentStageIndex;

        if (newStageIndex != oldStageIndex)
        {
            BossRank nextRank;

            switch (newStageIndex)
            {
                case 1:
                    nextRank = BossRank.Queen;
                    break;

                case 2:
                    nextRank = BossRank.King;
                    break;

                case 3:
                    nextRank = BossRank.Joker;
                    break;

                default:
                    nextRank = BossRank.Jack;
                    break;
            }

            bool jesterUnlockFlow =
                nextRank == BossRank.Queen &&
                JesterManager.Instance != null &&
                PlayerReward.Instance != null &&
                !PlayerReward.Instance.TraitHasAdd;

            if (jesterUnlockFlow)
            {
                JesterManager.Instance.UnlockJesters();

                if (JesterUnlockUI.Instance != null)
                {
                    JesterUnlockUI.Instance.SetKeepHandsHiddenAfterClose(true);
                    JesterUnlockUI.Instance.Show();

                    yield return new WaitUntil(
                        () =>
                            JesterUnlockUI.Instance == null ||
                            !JesterUnlockUI.Instance.IsShowing
                    );
                }
            }

            if (!jesterUnlockFlow &&
                JesterManager.Instance != null)
            {
                JesterManager.Instance.RecoverAfterRank(nextRank);
            }

            yield return StartCoroutine(
                StageManager.Instance.ChangeStage(nextRank));
        }
        else
        {
            yield return StartCoroutine(
                StageManager.Instance.ChangeStage(deadBoss.rank));
        }

        if (!BossManager.Instance.LoadNextBoss(
            showTraitSelection: false))
        {
            Debug.LogError(
                "[BATTLE] Failed to load next boss.");

            ChangeState(BattleState.Victory);
            yield break;
        }

        if (newStageIndex != oldStageIndex)
        {
            LevelManager.instance.FadeOut();
        }

        BossSO nextBoss =
            BossManager.Instance.CurrentBoss;

        bool isNextBossJoker =
            nextBoss != null &&
            nextBoss.isJoker;

        bool nextBossWaitingForTraitSelection = false;

        if (!isNextBossJoker)
        {
            if (TraitSelectionPanelUI.Instance != null &&
                BossManager.Instance.CurrentBoss != null)
            {
                TraitSelectionPanelUI.Instance.Show(
                    BossManager.Instance.CurrentBoss);

                nextBossWaitingForTraitSelection = true;
            }
            else
            {
                Debug.LogError(
                    "[BATTLE] TraitSelectionPanelUI or next boss is missing. " +
                    "Starting PlayerTurn as fallback.");
            }
        }



        bool drawBonusUnlocked =
           PlayerReward.Instance != null &&
           !PlayerReward.Instance.TraitHasAdd;

        if (drawBonusUnlocked)
        {
            if (handWasEmptyAfterPlay)
            {
                DrawBonusCards(8);
            }
            else
            {
                DrawBonusCards(2);
            }
        }
        else if (handManager.handCards.Count == 0 &&
                 deckManager.allCards.Count > 0)
        {

            DrawBonusCards(1);
        }

        if (BossManager.Instance.LastKillWasPerfect)
        {
            deckManager.allCards.Insert(
                0,
                deadBoss.bossCard);

            deckManager.RefreshDeckBar();
        }
        else
        {
            GraveyardManager.Instance.AddToGraveyard(
                deadBoss.bossCard);
        }

        if (handManager.handCards.Count == 0)
        {
            ChangeState(
                BattleState.Defeat);

            yield break;
        }

        if (!nextBossWaitingForTraitSelection)
        {
            ChangeState(BattleState.PlayerTurn);
        }

    }


    #endregion

    #region Player Action
    private IEnumerator ResolveSelectedCards()
    {
        if (waitingExtraAttack)
            waitingExtraAttack = false;

        handManager.SetInteractable(false);

        handWasEmptyAfterPlay =
            handManager.handCards.Count == 0;

        List<CardSO> cards = new();

        foreach (GameObject obj in handManager.selectedCards)
        {
            if (obj.TryGetComponent(out CardDisplay display))
            {
                cards.Add(display.cardScriptableObject);
            }
        }

        if (PlayerReward.Instance != null &&
            PlayerReward.Instance.HasReward(
                TraitID.Q_SEAL_OF_SILENCE))
        {
            int aceCount = 0;

            foreach (CardSO card in cards)
            {
                if (card != null && card.value == 1)
                {
                    aceCount++;
                }
            }

            if (aceCount > 0)
            {
                PlayerReward.Instance.AddAceHandBonus(
                    aceCount);

                PlayerReward.Instance.RefreshHandSize();
            }
        }

        yield return StartCoroutine(
            ResolveCombo(cards));

        if (
       cards.Count > 1 &&
       PlayerReward.Instance != null &&
       PlayerReward.Instance.HasReward(
           TraitID.K_ABSOLUTE_AUTHORITY) &&
       BossManager.Instance.CurrentBoss != null &&
       !BossManager.Instance.IsDead())
        {
            yield return StartCoroutine(
                ResolveCombo(cards));
        }

        yield return StartCoroutine(
            CardResolver.DiscardCards(
                handManager.selectedCards,
                handManager,
                graveyardSpawnPoint));

        handManager.ClearSelection();

        bool playerHasExtraAttackReward =
            PlayerReward.Instance.HasReward(
                TraitID.Q_LONE_DUEL);

        bool bossIsLoneDuel =
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.currentTrait != null &&
            BossManager.Instance.CurrentBoss.currentTrait.traitID
                == TraitID.Q_LONE_DUEL;

        if (!extraAttackUsed &&
            playerHasExtraAttackReward &&
            !bossIsLoneDuel)
        {
            extraAttackUsed = true;
            waitingExtraAttack = true;

            handManager.SetInteractable(true);
            InteractConfirmButton(true);
            TurnUIController.Instance.ShowYourTurn();

            yield break;
        }

        yield return new WaitForSeconds(1f);

        ChangeState(BattleState.CheckBattle);
    }

    private IEnumerator ResolveCombo(List<CardSO> cards)
    {
        TraitManager.Instance.InvokeBossEvent(
            TraitEventType.PlayCard,
            0);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.PlayCard,
            0);

        ApplyRewardK3(cards);

        ApplyRewardK4(cards);

        int foresightValue = 0;

        if (rewardK4Card != null)
        {
            foresightValue = rewardK4Card.value;
        }

        int damage =
            CardResolver.ResolveDamage(
                cards,
                foresightValue);

        int effectTotal =
            CardResolver.ResolveEffectTotal(
                cards,
                rewardK4Card);

        yield return StartCoroutine(
            CardResolver.ResolveEffects(
                cards,
                effectTotal));

        yield return StartCoroutine(
            CardResolver.PlaySuitFX(
                handManager.selectedCards));

        if (rewardK4Card != null)
        {
            CardSO foresightCard = rewardK4Card;

            yield return StartCoroutine(
                CardResolver.PlayRewardK4FX(
                    foresightCard));

            if (deckManager.allCards.Count > 0 &&
                deckManager.allCards[0] == foresightCard)
            {
                deckManager.allCards.RemoveAt(0);
                deckManager.RefreshDeckBar();
            }

            GraveyardManager.Instance.AddToGraveyard(
                foresightCard);

            rewardK4Card = null;
        }

        BossManager.Instance.TakeDamage(
            damage);

        TraitManager.Instance.InvokeBossEvent(
            TraitEventType.BossDamaged,
            damage);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.BossDamaged,
            damage);

        TraitManager.Instance.InvokeBossEvent(
            TraitEventType.AfterAttack,
            damage);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.AfterAttack,
            damage);

        BattleManager.Instance.OverrideSuit =
            CardSO.Suit.None;
    }

    private IEnumerator FinishDiscardRoutine(bool success)
    {
        handManager.SetInteractable(false);

        if (success)
        {
            BossSO boss = BossManager.Instance.CurrentBoss;

            bool bossNeedExtraDiscard =
                boss != null &&
                boss.currentTrait != null &&
                boss.currentTrait.traitID == TraitID.Q_ROYAL_TAX &&
                !BossManager.Instance.IsDead();

            if (!waitingExtraDiscard)
            {
                TraitManager.Instance.InvokeBossEvent(
                    TraitEventType.Discard,
                    0);
            }

            if (bossNeedExtraDiscard && !waitingExtraDiscard)
            {
                waitingExtraDiscard = true;

                yield return StartCoroutine(
                    BossFXManager.Instance.PlayBlockSuccessFX());

                TurnUIController.Instance.ShowDiscardTurn();

                handManager.StartDiscardPhase(1);

                yield break;
            }

            // Phase 2 hoàn tất
            waitingExtraDiscard = false;

            yield return StartCoroutine(
                BossFXManager.Instance.PlayBlockSuccessFX());

            ChangeState(BattleState.PlayerTurn);
        }
        else
        {
            yield return StartCoroutine(
                BossFXManager.Instance.PlayBlockFailFX());

            yield return new WaitForSeconds(0.2f);

            ChangeState(BattleState.Defeat);
        }
    }

    public void DrawBonusCards(int amount)
    {
        StartCoroutine(DrawCardRoutine(amount));
    }

    private IEnumerator DrawCardRoutine(int amount)
    {
        int actualDraw = 0;

        for (int i = 0; i < amount; i++)
        {
            if (deckManager.allCards.Count == 0)
                break;

            if (handManager.handCards.Count >= handManager.maxHandSize)
                break;

            deckManager.DrawCard(handManager);
            handManager.HideNextCardIfNeeded();
            actualDraw++;
            yield return new WaitForSeconds(0.3f);
        }

        LastDrawAmount = actualDraw;
        if (actualDraw > 0)
        {
            TraitManager.Instance.InvokeBossEvent(
                TraitEventType.Draw,
                actualDraw);

            TraitManager.Instance.InvokeRewardEvent(
                TraitEventType.Draw,
                actualDraw);
            StartCoroutine(damageManager.ShowAdditionCard(actualDraw));
        }
    }

    public void ConfirmPlayCards()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        if (JesterHandManager.Instance != null &&
            JesterHandManager.Instance.HasSelectedJester)
        {
            SoundManager.instance?.PlaySound2D("CardPlay");

            InteractConfirmButton(false);

            JesterHandManager.Instance.ConfirmSelectedJester();

            return;
        }

        bool bossLimit =
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.currentTrait != null &&
            BossManager.Instance.CurrentBoss.currentTrait.traitID
                == TraitID.Q_LONE_DUEL;

        if (bossLimit &&
            !waitingExtraAttack &&
            handManager.selectedCards.Count > 1)
        {
            StartCoroutine(
                NotificationInfo.Instance.SetUp(
                    "Can only play one card this turn"));

            return;
        }

        if (waitingExtraAttack)
        {
            if (handManager.selectedCards.Count == 0)
            {
                waitingExtraAttack = false;

                ChangeState(
                    BattleState.CheckBattle);

                return;
            }

            if (handManager.selectedCards.Count != 1)
            {
                StartCoroutine(
                    NotificationInfo.Instance.SetUp(
                        "Can only play one card this turn"));

                return;
            }
        }

        if (handManager.selectedCards.Count == 0)
            return;

        List<CardSO> cards = new();

        foreach (GameObject obj in handManager.selectedCards)
        {
            if (obj.TryGetComponent(
                out CardDisplay display))
            {
                cards.Add(
                    display.cardScriptableObject);
            }
        }

        if (!CardResolver.IsValidCombo(cards))
            return;

        SoundManager.instance?.PlaySound2D(
            "CardPlay");

        handManager.SetInteractable(false);

        InteractConfirmButton(false);

        StartCoroutine(
            ResolveSelectedCards());
    }

    public void FinishDiscard(bool success)
    {
        StartCoroutine(FinishDiscardRoutine(success));
    }

    public void EndTurn()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        ChangeState(BattleState.BossTurn);
    }

    #endregion

    #region Card Effect
    public void HealDeck(int amount)
    {
        int actualHeal = 0;
        if (GraveyardManager.Instance == null)
            return;

        List<CardSO> healedCards =
            GraveyardManager.Instance.PopRandomCards(amount);

        actualHeal = healedCards.Count;

        if (healedCards.Count == 0)
            return;

        deckManager.allCards.AddRange(healedCards);

        CardFXManager.Instance.PlayHealDeckFX(
            healedCards,
            graveyardSpawnPoint,
            tavernSpawnPoint
        );

        deckManager.RefreshDeckBar();
        StartCoroutine(damageManager.ShowHealCard(actualHeal));
    }

    #endregion
    #region End Battle

    private void Victory()
    {
        StartCoroutine(VictoryRoutine());

        if (MusicManager.instance != null)
        {
            MusicManager.instance.PlayMusic(
                "VictoryTheme",
                1f);
        }
    }

    private IEnumerator VictoryRoutine()
    {
        GameObject cardContainer = GameObject.FindGameObjectWithTag("CardContainer");
        cardContainer.SetActive(false);
        GameObject bossDisplay = GameObject.FindGameObjectWithTag("BossDisplay");
        bossDisplay.SetActive(false);
        GameObject[] decks = GameObject.FindGameObjectsWithTag("Deck");
        foreach (GameObject deck in decks) deck.SetActive(false);
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        foreach (GameObject bar in bars) bar.SetActive(false);
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
        foreach (GameObject button in buttons) button.SetActive(false);
        InteractConfirmButton(false);

        if (handManager != null)
        {
            handManager.enabled = false;
        }

        if (cardContainer != null)
        {
            cardContainer.SetActive(false);
        }

        if (TurnUIController.Instance != null)
        {
            yield return TurnUIController.Instance.ShowVictory();
        }

        yield return new WaitForSeconds(2f);

        if (hasPendingJokerEnding)
        {
            hasPendingJokerEnding = false;

            SaveManager.Instance.DeleteSave();

            if (pendingBadEnding)
            {
                pendingBadEnding = false;

                LevelManager.instance.LoadCutScene("badEndingFrames", "BadEndingTheme");
            }
            else
            {
                LevelManager.instance.LoadCutScene("goodEndingFrames", "GoodEndingTheme");
            }

            yield break;
        }

        SaveManager.Instance.DeleteSave();

        LevelManager.instance.LoadScene(
            "MenuScene");
    }

    private void Defeat()
    {
        SaveManager.Instance.DeleteSave();
        StartCoroutine(DefeatRoutine());
        MusicManager.instance.PlayMusic(
    "DefeatTheme",
    1f);
    }

    private IEnumerator DefeatRoutine()
    {
        InteractConfirmButton(false);
        handManager.enabled = false;

        yield return TurnUIController.Instance.ShowDefeat();

        yield return new WaitForSeconds(2f);

        LevelManager.instance.LoadScene("MenuScene");
    }
    #endregion

    public void InteractConfirmButton(bool isInteract)
    {
        EventTrigger eventTrigger = confirmButton.GetComponent<EventTrigger>();
        Image image = confirmButton.GetComponent<Image>();
        if (isInteract)
        {
            eventTrigger.enabled = true;
            confirmButton.interactable = true;
        }
        else
        {
            eventTrigger.enabled = false;
            confirmButton.interactable = false;
            image.color = Color.white;
        }
    }
    private void ApplyRewardK3(List<CardSO> cards)
    {
        if (PlayerReward.Instance == null)
            return;

        if (!PlayerReward.Instance.HasReward(TraitID.K_ROYAL_DECREE))
            return;

        if (cards == null || cards.Count != 1)
            return;

        CardSO card = cards[0];

        if (card == null)
            return;

        BossSO boss = BossManager.Instance.CurrentBoss;

        if (boss == null)
            return;

        CardSO.Suit originalSuit = card.suit;
        CardSO.Suit bossResistance = boss.resistanceSuit;

        List<CardSO.Suit> possibleSuits =
            new List<CardSO.Suit>();

        foreach (CardSO.Suit suit in
            System.Enum.GetValues(typeof(CardSO.Suit)))
        {
            if (suit == CardSO.Suit.None)
                continue;

            if (suit == originalSuit)
                continue;

            if (suit == bossResistance)
                continue;

            possibleSuits.Add(suit);
        }

        if (possibleSuits.Count == 0)
            return;

        CardSO.Suit newSuit =
            possibleSuits[
                Random.Range(0, possibleSuits.Count)
            ];

        BattleManager.Instance.OverrideSuit = newSuit;

        SuitChangedUI.Instance.Show(newSuit);
    }


    public CardSO.Suit GetSuit(CardSO card)
    {
        if (OverrideSuit != CardSO.Suit.None)
            return OverrideSuit;

        return card.suit;
    }
    private void ApplyRewardK4(List<CardSO> cards)
    {
        rewardK4Card = null;

        if (PlayerReward.Instance == null ||
            !PlayerReward.Instance.HasReward(TraitID.K_BLIND_FATE))
            return;

        if (cards == null || cards.Count <= 1)
            return;

        if (deckManager == null ||
            deckManager.allCards == null ||
            deckManager.allCards.Count == 0)
            return;

        rewardK4Card = deckManager.allCards[0];

        if (rewardK4Card == null)
        {
            rewardK4Card = null;
            return;
        }
    }
    public bool UseJesterReset()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return false;

        if (JesterManager.Instance == null)
            return false;

        if (!JesterManager.Instance.ConsumeReset())
            return false;

        StartCoroutine(JesterResetRoutine());

        return true;
    }

    private IEnumerator JesterResetRoutine()
    {
        handManager.SetInteractable(false);

        InteractConfirmButton(false);


        BossSO boss =
            BossManager.Instance.CurrentBoss;

        if (boss != null)
        {
            // Only capture/disable on the FIRST Reset used against this
            // boss in the current fight. If Reset is used again on the
            // same still-alive boss (e.g. 2 stacked Jester charges spent
            // in one King fight), boss.currentTrait/resistanceSuit are
            // already null from the first Reset - re-capturing here would
            // overwrite the saved original trait with null and the boss
            // would never award its trait/reward on death.
            if (jesterResetBoss != boss)
            {
                // IMPORTANT: do not destroy the trait. We temporarily remove
                // it from CurrentBoss so TraitManager/CardResolver see no
                // boss skill during the reset fight. The original trait is
                // restored when this exact boss dies, so its reward is
                // still granted.
                jesterResetBoss = boss;
                jesterResetDisabledTrait = boss.currentTrait;
                jesterResetDisabledResistance = boss.resistanceSuit;

                Debug.Log(
                    $"[JESTER RESET] Boss skill disabled. " +
                    $"Saved trait = " +
                    (jesterResetDisabledTrait != null
                        ? jesterResetDisabledTrait.name
                        : "NONE"));

                boss.currentTrait = null;
                boss.resistanceSuit = CardSO.Suit.None;

                BossManager.Instance.RefreshBossInfo();

                if (BossManager.Instance.BossDisplay != null)
                {
                    BossManager.Instance.BossDisplay
                        .UpdateResistance(CardSO.Suit.None);
                }
            }
            else
            {
                Debug.Log(
                    "[JESTER RESET] Reset used again on the same boss; " +
                    "trait/resistance already disabled, original values " +
                    "already saved.");
            }
        }


        foreach (GameObject obj in
            new List<GameObject>(
                handManager.handCards))
        {
            if (obj == null)
                continue;

            CardDisplay display =
                obj.GetComponent<CardDisplay>();

            if (display != null &&
                display.cardScriptableObject != null)
            {
                if (GraveyardManager.Instance != null)
                {
                    GraveyardManager.Instance.AddToGraveyard(
                        display.cardScriptableObject);
                }
            }

            Destroy(obj);
        }

        handManager.handCards.Clear();
        handManager.selectedCards.Clear();

        yield return DrawCardRoutine(8);
        handManager.RepositionAllCards(null);

        handManager.SetInteractable(true);

        InteractConfirmButton(true);

    }

    public bool UseJesterInstantKill()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return false;

        if (JesterManager.Instance == null)
            return false;

        if (!JesterManager.Instance.ConsumeInstantKill())
            return false;

        int currentHP =
            BossManager.Instance.CurrentHP;

        if (currentHP > 0)
        {
            BossManager.Instance.TakeDamage(
                currentHP);
        }

        return true;
    }

}