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


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
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

    #region Battle Flow

    private void StartBattle()
    {
        BossManager.Instance.Initialize();

        StartCoroutine(deckManager.AddCardFromFirst());
    }

    private void StartPlayerTurn()
    {
        handManager.handCards.RemoveAll(card => card == null);
        handManager.SetInteractable(true);
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

        // Boss không còn ATK -> bỏ qua bước discard
        if (damage <= 0)
        {
            Debug.Log("Boss ATK = 0, bỏ qua phản công.");

            yield return new WaitForSeconds(0.5f);

            ChangeState(BattleState.PlayerTurn);
            yield break;
        }

        TurnUIController.Instance.ShowDiscardTurn();

        yield return new WaitForSeconds(0.8f);

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

        yield return StartCoroutine(
            BossFXManager.Instance.PlayDeathFX(
                BossManager.Instance.BossTransform));

        bool changeStage =
            BossManager.Instance.NeedChangeStage(deadBoss);
        handManager.SetInteractable(false);
        if (changeStage)
        {
            BossRank nextStage = deadBoss.rank;

            switch (deadBoss.rank)
            {
                case BossRank.Jack:
                    nextStage = BossRank.Queen;
                    break;

                case BossRank.Queen:
                    nextStage = BossRank.King;
                    break;
            }

            yield return StartCoroutine(
                StageManager.Instance.ChangeStage(nextStage));
        }

        bool hasNextBoss = BossManager.Instance.LoadNextBoss();

        if (!hasNextBoss)
        {
            yield return StartCoroutine(StageManager.Instance.VictoryStage());
            ChangeState(BattleState.Victory);
            yield break;
        }

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

        ChangeState(BattleState.PlayerTurn);

    }

    #endregion

    #region Player Action
    private IEnumerator ResolveSelectedCards()
    {
        handManager.SetInteractable(false);

        handWasEmptyAfterPlay = handManager.handCards.Count == 0;


        List<CardSO> cards = new();

        foreach (GameObject obj in handManager.selectedCards)
        {
            if (obj.TryGetComponent(out CardDisplay display))
                cards.Add(display.cardScriptableObject);
        }

        handWasEmptyAfterPlay =
       handManager.handCards.Count ==
       handManager.selectedCards.Count;

        ResolveCombo(cards);

        foreach (GameObject obj in handManager.selectedCards)
        {
            handManager.handCards.Remove(obj);
            if (obj.TryGetComponent(out CardDisplay display))
            {
                GraveyardManager.Instance.AddToGraveyard(display.cardScriptableObject);
            }

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                obj,
                graveyardSpawnPoint);
        }

        yield return new WaitForSeconds(0.55f);

        handManager.ClearSelection();

        ChangeState(BattleState.CheckBattle);
    }
    private void ResolveCombo(List<CardSO> cards)
    {

        CardSO.Suit resist = BossManager.Instance.CurrentBoss.resistanceSuit;

        int total = 0;
        int damage = 0;

        HashSet<CardSO.Suit> suits = new();

        foreach (CardSO card in cards)
        {
            total += card.value;
            damage += card.value;      // Damage luôn tính

            // Chỉ hiệu ứng mới bị kháng
            if (card.suit != resist)
                suits.Add(card.suit);
        }

        if (suits.Contains(CardSO.Suit.Clubs))
            damage *= 2;

        AttackBoss(damage);

        if (suits.Contains(CardSO.Suit.Hearts))
            HealDeck(total);

        if (suits.Contains(CardSO.Suit.Diamonds))
            DrawBonusCards(total);

        if (suits.Contains(CardSO.Suit.Spades))
            BossManager.Instance.ReduceAttack(total);
    }
    private void DrawBonusCards(int amount)
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

        if (!IsValidCombo(cards))
        {
            Debug.Log("Invalid Combo");
            return;
        }

        SoundManager.instance?.PlaySound2D("CardPlay");
        StartCoroutine(ResolveSelectedCards());
    }
    private bool IsValidCombo(List<CardSO> cards)
    {
        if (cards.Count == 0)
            return false;

        // 1 lá luôn hợp lệ (kể cả J, Q, K)
        if (cards.Count == 1)
            return true;

        List<CardSO> aces = new();
        List<CardSO> normals = new();

        foreach (CardSO card in cards)
        {
            if (card.value == 1)
                aces.Add(card);
            else
                normals.Add(card);
        }

        // Chỉ toàn Ace
        if (normals.Count == 0)
        {
            int totalAce = aces.Count;
            return totalAce <= 10;
        }

        // Chỉ được 1 Ace Companion
        if (aces.Count > 1)
            return false;

        int rank = normals[0].value;
        int total = 0;

        foreach (CardSO card in normals)
        {
            if (card.value != rank)
                return false;

            total += card.value;
        }

        // Có Ace Companion thì bỏ qua giới hạn <=10
        if (aces.Count == 1)
            return true;

        // Không có Ace thì tổng phải <=10
        return total <= 10;
    }
    public void FinishDiscard(bool success)
    {
        if (success)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.Defeat);
    }

    public void EndTurn()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        ChangeState(BattleState.BossTurn);
    }

    #endregion

    #region Card Effect


    private void AttackBoss(int damage)
    {
        BossManager.Instance.TakeDamage(damage);

        Debug.Log("Boss HP : " + BossManager.Instance.CurrentHP);
    }



    private void HealDeck(int amount)
    {
        if (GraveyardManager.Instance == null)
            return;

        List<CardSO> healedCards =
            GraveyardManager.Instance.PopRandomCards(amount);

        if (healedCards.Count == 0)
            return;

        foreach (CardSO card in healedCards)
            deckManager.allCards.Add(card);

        CardFXManager.Instance.PlayHealDeckFX(
            healedCards,
            graveyardSpawnPoint,
            tavernSpawnPoint
        );

        deckManager.ShuffleDeck();
        deckManager.RefreshDeckBar();
    }

    #endregion

    #region End Battle

    private void Victory()
    {
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