using System.Collections.Generic;
using UnityEngine;

public class GraveyardManager : MonoBehaviour
{
    public static GraveyardManager Instance { get; private set; }

    [Header("Graveyard Data")]
    // Danh sách lưu trữ thực tế dữ liệu ScriptableObject
    public List<CardSO> graveyardCards = new();

    [Header("Visual References (Optional)")]
    [SerializeField] private Transform graveyardSpawnPoint; // Vị trí đống bài hủy trên bàn bàn đấu
    [SerializeField] private DeckBarUI discardDeckBar;
    [SerializeField] private TarvernDeckManager deckManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        Debug.Log("Graveyard Start");
        discardDeckBar.Init(deckManager.allCards.Count);
        discardDeckBar.UpdateBar(graveyardCards.Count);
    }

    // Hàm thêm bài vào nghĩa địa
    public void AddToGraveyard(CardSO card)
    {
        graveyardCards.Add(card);

        ShuffleGraveyard();

        discardDeckBar.UpdateBar(graveyardCards.Count);
    }
    private void ShuffleGraveyard()
    {
        for (int i = graveyardCards.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (graveyardCards[i], graveyardCards[random]) =
                (graveyardCards[random], graveyardCards[i]);
        }
    }

    // Hàm lấy bài ra để hồi phục (Dành cho chất Cơ - Hearts)
    public List<CardSO> PopRandomCards(int amount)
    {
        List<CardSO> poppedCards = new List<CardSO>();

        if (graveyardCards.Count == 0) return poppedCards;

        // Tính số lượng lá bài thực tế có thể lấy ra
        int actualAmount = Mathf.Min(amount, graveyardCards.Count);

        for (int i = 0; i < actualAmount; i++)
        {
            int randomIndex = Random.Range(0, graveyardCards.Count);
            CardSO card = graveyardCards[randomIndex];

            graveyardCards.RemoveAt(randomIndex);
            poppedCards.Add(card);
            discardDeckBar.UpdateBar(graveyardCards.Count);
        }

        return poppedCards;
    }
}
