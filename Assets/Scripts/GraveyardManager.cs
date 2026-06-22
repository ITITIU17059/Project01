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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm thêm bài vào nghĩa địa
    public void AddToGraveyard(CardSO card)
    {
        graveyardCards.Add(card);
        Debug.Log($"[Graveyard] Đã đưa lá {card.name} vào nghĩa địa. Tổng số bài trong mộ: {graveyardCards.Count}");

        // TODO: Cập nhật hình ảnh lá bài trên cùng của đống bài hủy nếu cần
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
        }

        return poppedCards;
    }
}
