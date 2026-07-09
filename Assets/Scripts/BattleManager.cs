using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; }

    [Header("References")]
    [SerializeField] private TarvernDeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button confirmButton;

    [Header("Spawn Points Setup")]
    [SerializeField] private Transform tavernSpawnPoint;   // Kéo ô TavernSpawnPoint từ Hierarchy vào đây
    public Transform graveyardSpawnPoint; // Kéo ô GraveyardSpawnPoint từ Hierarchy vào đây

    [Header("Stats")]
    public int currentShield = 0;
    [SerializeField] private List<BossSO> jackBosses;
    [SerializeField] private List<BossSO> queenBosses;
    [SerializeField] private List<BossSO> kingBosses;

    private Queue<BossSO> bossQueue = new();

    [SerializeField] private BossDisplay bossDisplay;

    private int currentBossIndex = 0;

    private BossSO currentBoss;
    public int bossHealth;

    public int bossAttack;

    private void Start()
    {
        ChangeState(BattleState.StartBattle);
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
                ResolveBossAttack();
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

    private void CreateBossQueue()
    {
        bossQueue.Clear();

        List<BossSO> j = new List<BossSO>(jackBosses);
        List<BossSO> q = new List<BossSO>(queenBosses);
        List<BossSO> k = new List<BossSO>(kingBosses);

        Shuffle(j);
        Shuffle(q);
        Shuffle(k);

        foreach (var boss in j)
            bossQueue.Enqueue(boss);

        foreach (var boss in q)
            bossQueue.Enqueue(boss);

        foreach (var boss in k)
            bossQueue.Enqueue(boss);
    }

    private void Shuffle(List<BossSO> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (list[i], list[random]) = (list[random], list[i]);
        }
    }

    private void StartBattle()
    {
        currentShield = 0;

        CreateBossQueue();

        LoadNextBoss();

        StartCoroutine(deckManager.AddCardFromFirst());
    }

    private void LoadNextBoss()
    {
        if (bossQueue.Count == 0)
        {
            ChangeState(BattleState.Victory);
            return;
        }

        currentBoss = bossQueue.Dequeue();

        bossHealth = currentBoss.hp;
        bossAttack = currentBoss.atk;

        bossDisplay.Setup(currentBoss);
    }

    private void StartPlayerTurn()
    {
        Debug.Log("===== PLAYER TURN =====");

        deckManager.DrawCard(handManager);
        confirmButton.interactable = true;
    }

    public void ConfirmPlayCards()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        if (handManager == null)
            return;

        if (handManager.selectedCards.Count == 0)
            return;

        ResolveSelectedCards();
    }

    private void ResolveSelectedCards()
    {
        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent<CardDisplay>(out var display))
            {
                OnCardPlayed(display.cardScriptableObject);
            }
        }

        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent<CardDisplay>(out var display))
            {
                GraveyardManager.Instance.AddToGraveyard(display.cardScriptableObject);
            }

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                cardObject,
                graveyardSpawnPoint
            );
        }

        handManager.ClearSelection();

        ChangeState(BattleState.CheckBattle);
    }

    private void CheckBattle()
    {
        confirmButton.interactable = false;
        if (bossHealth <= 0)
        {
            LoadNextBoss();

            ChangeState(BattleState.PlayerTurn);

            return;
        }

        ChangeState(BattleState.BossTurn);
    }

    private void StartBossTurn()
    {
        Debug.Log("Boss Attack");

        ChangeState(BattleState.ResolveAttack);
    }

    private void ResolveBossAttack()
    {
        int damage = Mathf.Max(0, bossAttack - currentShield);

        currentShield = 0;

        if (damage == 0)
        {
            FinishDiscard(true);
            return;
        }

        handManager.StartDiscardPhase(damage);
    }

    public void FinishDiscard(bool success)
    {
        if (success)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.Defeat);
    }

    private void Victory()
    {
        Debug.Log("Victory");
    }

    private void Defeat()
    {
        Debug.Log("Defeat");
    }

    public void EndTurn()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        ChangeState(BattleState.BossTurn);
    }

    public void OnCardPlayed(CardSO cardData)
    {
        int baseValue = cardData.value;
        string suit = cardData.suit.ToString();

        switch (suit)
        {
            case "Hearts":
                HealDeck(baseValue);
                AttackBoss(baseValue);
                break;
            case "Diamonds":
                AttackBoss(baseValue);
                DrawBonusCards(baseValue);
                break;
            case "Spades":
                AddShield(baseValue);
                AttackBoss(baseValue);
                break;
            case "Clubs":
                AttackBoss(baseValue * 2);
                break;
        }
    }

    private void AttackBoss(int damage)
    {
        bossHealth -= damage;

        if (bossHealth < 0)
            bossHealth = 0;

        bossDisplay.UpdateHP(bossHealth);

        Debug.Log("Boss HP: " + bossHealth);
    }
    private void AddShield(int value)
    {
        currentShield += value;
        Debug.Log($"[Chất Bích] Tăng {value} Giáp. Giáp hiện tại: {currentShield}");
    }

    private void DrawBonusCards(int amount)
    {
        if (deckManager && handManager)
        {
            for (int i = 0; i < amount; i++) deckManager.DrawCard(handManager);
        }
    }

    private void HealDeck(int amount)
    {
        if (GraveyardManager.Instance == null || deckManager == null || CardFXManager.Instance == null) return;

        // Lấy danh sách dữ liệu bài ngẫu nhiên từ mộ ra
        List<CardSO> healedCards = GraveyardManager.Instance.PopRandomCards(amount);
        if (healedCards.Count == 0) return;

        // 1. XỬ LÝ LOGIC CORE: Thêm ngược dữ liệu vào bộ bài rút
        foreach (var card in healedCards)
        {
            deckManager.allCards.Add(card);
        }

        // 2. XỬ LÝ HIỆU ỨNG VISUAL: Kích hoạt hiệu ứng bay bài ảo qua hệ thống FX tập trung
        CardFXManager.Instance.PlayHealDeckFX(
            healedCards,
            graveyardSpawnPoint,
            tavernSpawnPoint
        );

        deckManager.ShuffleDeck();
    }
}