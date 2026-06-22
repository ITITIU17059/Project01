using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TarvernDeckManager deckManager;
    [SerializeField] private HandManager handManager;

    [Header("Spawn Points Setup")]
    [SerializeField] private Transform tavernSpawnPoint;   // Kéo ô TavernSpawnPoint từ Hierarchy vào đây
    [SerializeField] private Transform graveyardSpawnPoint; // Kéo ô GraveyardSpawnPoint từ Hierarchy vào đây

    [Header("Stats")]
    public int currentShield = 0;
    public int bossHealth = 40;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        Debug.Log($"Tấn công Boss! Máu Boss còn lại: {bossHealth}");
        if (bossHealth <= 0) Debug.Log("Boss đã bị đánh bại!");
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
    }

    public void ConfirmPlayCards()
    {
        if (handManager == null || handManager.selectedCards.Count == 0) return;

        // Bước 1: Tính toán logic hiệu ứng trước
        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent<CardDisplay>(out var display) && display.cardScriptableObject != null)
            {
                OnCardPlayed(display.cardScriptableObject);
            }
        }

        // Bước 2: Nạp dữ liệu vào mộ và kích hoạt hoạt ảnh biến mất tiêu biến bài
        int index = 0;
        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent<CardDisplay>(out var display) && display.cardScriptableObject != null)
            {
                if (GraveyardManager.Instance) GraveyardManager.Instance.AddToGraveyard(display.cardScriptableObject);
            }

            if (CardFXManager.Instance)
            {
                CardFXManager.Instance.PlayAnimateToGraveyardFX(cardObject, graveyardSpawnPoint);
            }
            index++;
        }

        handManager.selectedCards.Clear();
    }
}