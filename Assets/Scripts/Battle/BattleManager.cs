using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public void ContinueFromInventory()
    {
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

            GraveyardManager.Instance.LoadData(save.graveyardCards);

            handManager.LoadHand(save.handCards);
        }
        else
        {
            StartCoroutine(deckManager.AddCardFromFirst());
        }
    }

    private void StartPlayerTurn()
    {
        waitingExtraAttack = false;
        extraAttackUsed = false;

        handManager.handCards.RemoveAll(card => card == null);

        handManager.SetInteractable(true);

        TraitManager.Instance.InvokeBossEvent(
            TraitEventType.PlayerTurn,
            0);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.PlayerTurn,
            0);

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
            StartCoroutine(HandleBossDeath());
            return;
        }

        ChangeState(BattleState.BossTurn);
    }

    private IEnumerator HandleBossDeath()
    {
        
        handManager.UnlockCard();
        handManager.SetInteractable(false);
        handManager.RevealAllHiddenCards();
        BossSO deadBoss = BossManager.Instance.CurrentBoss;
        BossManager.Instance.BossDisplay.ResetBossSprite();

        yield return BossFXManager.Instance.PlayDeathFX(
        BossManager.Instance.BossTransform);

        Transform target =
        BossManager.Instance.LastKillWasPerfect
            ? tavernSpawnPoint
            : graveyardSpawnPoint;

        yield return BossFXManager.Instance
            .PlayCollectRewardFX(
                BossManager.Instance.BossDisplay,
                deadBoss.bossCard,
                target);

        BossManager.Instance.OnBossDefeated(deadBoss);

        if (deadBoss.currentTrait != null)
        {
            PlayerReward.Instance.AddReward(deadBoss.currentTrait.reward);
        }

        bool hasNextBoss = BossManager.Instance.HasMoreBosses;

        if (!hasNextBoss)
        {
            yield return StartCoroutine(StageManager.Instance.VictoryStage());
            SaveManager.Instance.DeleteSave();
            ChangeState(BattleState.Victory);
            yield break;
        }

        waitingForInventory = true;
        LevelManager.instance.LoadSceneAdditive("InventoryScene");

        yield return new WaitUntil(() => !waitingForInventory);

        LevelManager.instance.UnloadSceneAdditive("InventoryScene");

        if (BossManager.Instance.CurrentStageIndex == 0)
        {
            yield return StageManager.Instance.ChangeStage(BossRank.Jack);
        }
        else if (BossManager.Instance.CurrentStageIndex == 1 &&
    deadBoss.rank == BossRank.Jack)
        {
            yield return StageManager.Instance.ChangeStage(BossRank.Queen);
        }
        else if (BossManager.Instance.CurrentStageIndex == 2 &&
                 deadBoss.rank == BossRank.Queen)
        {
            yield return StageManager.Instance.ChangeStage(BossRank.King);
        }
        else if (BossManager.Instance.CurrentStageIndex == 3 &&
                 deadBoss.rank == BossRank.King)
        {
            yield return StageManager.Instance.ChangeStage(BossRank.Joker);
        }



        if (handWasEmptyAfterPlay)
        {
            while (handManager.handCards.Count < handManager.maxHandSize &&
                   deckManager.allCards.Count > 0)
            {
                deckManager.DrawCard(handManager);
                handManager.HideNextCardIfNeeded();
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (deckManager.allCards.Count == 0)
                    break;

                if (handManager.handCards.Count >= handManager.maxHandSize)
                    break;

                deckManager.DrawCard(handManager);
                handManager.HideNextCardIfNeeded();
            }
        }

        if (BossManager.Instance.LastKillWasPerfect)
        {
            deckManager.allCards.Insert(0, deadBoss.bossCard);
            deckManager.RefreshDeckBar();
        }
        else
        {
            GraveyardManager.Instance.AddToGraveyard(deadBoss.bossCard);
        }

        if (handManager.handCards.Count == 0)
        {
            ChangeState(BattleState.Defeat);
            yield break;
        }

        yield break;

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

        yield return StartCoroutine(
            ResolveCombo(cards));

        if (PlayerReward.Instance.HasReward(TraitID.K_ABSOLUTE_AUTHORITY))
        {
            handManager.ReturnRandomSelectedCard();
        }
        yield return StartCoroutine(
            CardResolver.DiscardCards(
                handManager.selectedCards,
                handManager,
                graveyardSpawnPoint));

        handManager.ClearSelection();
        bool playerHasExtraAttackReward =
    PlayerReward.Instance.HasReward(TraitID.Q_LONE_DUEL);

        bool bossIsLoneDuel =
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.currentTrait != null &&
            BossManager.Instance.CurrentBoss.currentTrait.traitID == TraitID.Q_LONE_DUEL;
        if (!extraAttackUsed &&
    playerHasExtraAttackReward &&
    !bossIsLoneDuel)
        {
            extraAttackUsed = true;
            waitingExtraAttack = true;

            handManager.SetInteractable(true);
            InteractConfirmButton(true);
            yield break;
        }

        yield return new WaitForSeconds(1f);

        ChangeState(BattleState.CheckBattle);
    }

    private IEnumerator ResolveCombo(List<CardSO> cards)
    {
        TraitManager.Instance.InvokeBossEvent(TraitEventType.PlayCard, 0);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.PlayCard, 0);
        ApplyRewardK3(cards);
        ApplyRewardK4(cards);
        int damage = CardResolver.ResolveDamage(cards);
       
        // Hiệu ứng lá bài
        yield return StartCoroutine(
            CardResolver.ResolveEffects(cards));

        // Animation chất bài
        yield return StartCoroutine(
            CardResolver.PlaySuitFX(handManager.selectedCards));
        if (rewardK4Card != null)
        {
            yield return StartCoroutine(
                CardResolver.PlayRewardK4FX(rewardK4Card));

            GraveyardManager.Instance.AddToGraveyard(rewardK4Card);

            rewardK4Card = null;
        }
        // Sau cùng mới gây sát thương
        BossManager.Instance.TakeDamage(damage);

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
        BattleManager.Instance.OverrideSuit = CardSO.Suit.None;
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

            // Chỉ kích hoạt trait/reward ở phase discard đầu tiên
            if (!waitingExtraDiscard)
            {
                TraitManager.Instance.InvokeBossEvent(
                    TraitEventType.Discard,
                    0);
            }

            // Sau phase 1 thì bắt discard thêm
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

        bool bossLimit =
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.currentTrait != null &&
            BossManager.Instance.CurrentBoss.currentTrait.traitID == TraitID.Q_LONE_DUEL;

        if (bossLimit &&
            !waitingExtraAttack &&
            handManager.selectedCards.Count > 1)
        {
            Debug.Log("Boss chỉ cho phép đánh 1 lá.");
            return;
        }

        if (waitingExtraAttack)
        {
            if (handManager.selectedCards.Count == 0)
            {
                waitingExtraAttack = false;
                ChangeState(BattleState.CheckBattle);
                return;
            }

            if (handManager.selectedCards.Count != 1)
            {
                Debug.Log("Lượt đánh thêm chỉ được đánh 1 lá.");
                return;
            }
        }

        if (handManager.selectedCards.Count == 0)
            return;

        List<CardSO> cards = new();

        foreach (GameObject obj in handManager.selectedCards)
        {
            if (obj.TryGetComponent(out CardDisplay display))
                cards.Add(display.cardScriptableObject);
        }

        if (!CardResolver.IsValidCombo(cards))
            return;

        SoundManager.instance?.PlaySound2D("CardPlay");

        handManager.SetInteractable(false);
        InteractConfirmButton(false);

        StartCoroutine(ResolveSelectedCards());
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
        SaveManager.Instance.DeleteSave();
        StartCoroutine(VictoryRoutine());
        MusicManager.instance.PlayMusic(
    "VictoryTheme",
    1f);
    }

    private IEnumerator VictoryRoutine()
    {
        InteractConfirmButton(false);
        handManager.enabled = false;
        GameObject cardContainer = GameObject.FindGameObjectWithTag("CardContainer");
        cardContainer.SetActive(false);


        yield return TurnUIController.Instance.ShowVictory();

        yield return new WaitForSeconds(2f);

        LevelManager.instance.LoadScene("MenuScene");
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
        if (!PlayerReward.Instance.HasReward(TraitID.K_ROYAL_DECREE))
            return;

        if (cards.Count != 1)
            return;

        CardSO.Suit originalSuit = cards[0].suit;

        List<CardSO.Suit> possibleSuits = new List<CardSO.Suit>();

        foreach (CardSO.Suit suit in System.Enum.GetValues(typeof(CardSO.Suit)))
        {
            if (suit == CardSO.Suit.None)
                continue;

            if (suit == originalSuit)
                continue;

            possibleSuits.Add(suit);
        }

        BattleManager.Instance.OverrideSuit =
            possibleSuits[Random.Range(0, possibleSuits.Count)];

        SuitChangedUI.Instance.Show(
            BattleManager.Instance.OverrideSuit);

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

        if (!PlayerReward.Instance.HasReward(TraitID.K_BLIND_FATE))
            return;

        if (cards.Count != 1)
            return;

        if (deckManager.allCards.Count == 0)
            return;

        rewardK4Card = deckManager.allCards[0];

        deckManager.allCards.RemoveAt(0);
        deckManager.RefreshDeckBar();

        cards.Add(rewardK4Card);
    }

}