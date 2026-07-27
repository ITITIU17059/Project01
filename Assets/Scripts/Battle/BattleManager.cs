using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; }

    [Header("References")]
    [SerializeField] private TarvernDeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private Button confirmButton;

    [Header("Spawn Points")]
    [SerializeField] private Transform tavernSpawnPoint;
    public Transform graveyardSpawnPoint;
    [SerializeField] private Transform playerHitPoint;

    public Transform PlayerHitPoint => playerHitPoint;
    private bool handWasEmptyAfterPlay;
    private bool waitingForInventory;

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
        if (BossManager.Instance.CurrentBoss == null)
        {
            BossManager.Instance.Initialize();
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
        handManager.handCards.RemoveAll(card => card == null);

        if (BossManager.Instance.CurrentBoss.isJoker)
            BossManager.Instance.RandomizeJokerSuit();

        handManager.SetInteractable(true);
        TraitManager.Instance.InvokeBossEvent(TraitEventType.PlayerTurn);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.PlayerTurn);
        if (handManager.handCards.Count == 0)
        {
            ChangeState(BattleState.Defeat);
            return;
        }

        TurnUIController.Instance.ShowYourTurn();
        confirmButton.interactable = true;
    }

    private void StartBossTurn()
    {
        TraitManager.Instance.InvokeBossEvent(TraitEventType.BossTurn);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.BossTurn);

        TurnUIController.Instance.ShowEnemyTurn();

        DOVirtual.DelayedCall(1.2f, () =>
        {
            ChangeState(BattleState.ResolveAttack);
        });
    }

    private IEnumerator ResolveBossAttack()
    {
        handManager.CancelCurrentSelection();

        // Boss tấn công trước
        yield return StartCoroutine(
            BossFXManager.Instance.PlayBossAttackFX());

        int damage = BossManager.Instance.CurrentATK;

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
        confirmButton.interactable = false;

        if (BossManager.Instance.IsDead())
        {
            StartCoroutine(HandleBossDeath());
            return;
        }

        ChangeState(BattleState.BossTurn);
    }

    private IEnumerator HandleBossDeath()
    {
        handManager.SetInteractable(false);
        BossSO deadBoss = BossManager.Instance.CurrentBoss;

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

        if (BossManager.Instance.CurrentStageIndex == 1 &&
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

        bool hasNextBoss = BossManager.Instance.HasMoreBosses;

        if (!hasNextBoss)
        {
            yield return StartCoroutine(StageManager.Instance.VictoryStage());
            SaveManager.Instance.DeleteSave();
            ChangeState(BattleState.Victory);
            yield break;
        }

        // Vào Inventory Scene (additive, không mất deck/hand hiện tại)
        waitingForInventory = true;
        LevelManager.instance.LoadSceneAdditive("InventoryScene");

        yield return new WaitUntil(() => !waitingForInventory);

        LevelManager.instance.UnloadSceneAdditive("InventoryScene");

        // Rút bài trước

        if (handWasEmptyAfterPlay)
        {
            while (handManager.handCards.Count < handManager.maxHandSize &&
                   deckManager.allCards.Count > 0)
            {
                deckManager.DrawCard(handManager);
            }
        }
        // Còn bài -> chỉ rút 2 lá
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (deckManager.allCards.Count == 0)
                    break;

                if (handManager.handCards.Count >= handManager.maxHandSize)
                    break;

                deckManager.DrawCard(handManager);
            }
        }

        // Thêm Reward Card sau khi rút xong
        if (BossManager.Instance.LastKillWasPerfect)
        {
            deckManager.allCards.Insert(0, deadBoss.bossCard);
            deckManager.RefreshDeckBar();
        }
        else
        {
            GraveyardManager.Instance.AddToGraveyard(deadBoss.bossCard);
        }

        // Kiểm tra còn bài trên tay
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

        yield return StartCoroutine(
            CardResolver.DiscardCards(
                handManager.selectedCards,
                handManager,
                graveyardSpawnPoint));

        handManager.ClearSelection();

        ChangeState(BattleState.CheckBattle);
    }
    private IEnumerator ResolveCombo(List<CardSO> cards)
    {
        TraitManager.Instance.InvokeBossEvent(TraitEventType.PlayCard);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.PlayCard);

        int damage = CardResolver.ResolveDamage(cards);
        TraitManager.Instance.InvokeBossEvent(TraitEventType.BossDamaged);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.BossDamaged);
        BossManager.Instance.TakeDamage(damage);

        

        TraitManager.Instance.InvokeBossEvent(TraitEventType.AfterAttack);
        TraitManager.Instance.InvokeRewardEvent(TraitEventType.AfterAttack);

        yield return StartCoroutine(
            CardResolver.ResolveEffects(cards));

        yield return StartCoroutine(
    CardResolver.PlaySuitFX(handManager.selectedCards));
    }

    private IEnumerator FinishDiscardRoutine(bool success)
    {
        handManager.SetInteractable(false);

        if (success)
        {
            TraitManager.Instance.InvokeBossEvent(TraitEventType.Discard);
            TraitManager.Instance.InvokeRewardEvent(TraitEventType.Discard);

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

        for (int i = 0; i < amount; i++)
        {
            deckManager.DrawCard(handManager);

        }
    }

    public void ConfirmPlayCards()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        if (handManager.selectedCards.Count == 0)
            return;

        List<CardSO> cards = new();

        foreach (GameObject obj in handManager.selectedCards)
        {
            if (obj.TryGetComponent(out CardDisplay display))
                cards.Add(display.cardScriptableObject);
        }

        if (!CardResolver.IsValidCombo(cards))
        {
            Debug.Log("Invalid Combo");
            return;
        }
        SoundManager.instance?.PlaySound2D("CardPlay");
        handManager.SetInteractable(false);
        confirmButton.interactable = false;

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
        if (GraveyardManager.Instance == null)
            return;

        List<CardSO> healedCards =
            GraveyardManager.Instance.PopRandomCards(amount);

        if (healedCards.Count == 0)
            return;

        deckManager.allCards.AddRange(healedCards);

        CardFXManager.Instance.PlayHealDeckFX(
            healedCards,
            graveyardSpawnPoint,
            tavernSpawnPoint
        );

        deckManager.RefreshDeckBar();

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
        confirmButton.interactable = false;
        handManager.enabled = false;

        yield return TurnUIController.Instance.ShowVictory();

        yield return new WaitForSeconds(2f);

        LevelManager.instance.LoadScene("MenuScene", "CrossFade");
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
        confirmButton.interactable = false;
        handManager.enabled = false;

        yield return TurnUIController.Instance.ShowDefeat();

        yield return new WaitForSeconds(2f);

        LevelManager.instance.LoadScene("MenuScene", "CrossFade");
    }
    #endregion
}