using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarvernDeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new();
    [SerializeField] private HandManager handManager;
    [SerializeField] private DeckBarUI drawDeckBar;
    public static TarvernDeckManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        CardSO[] cards = Resources.LoadAll<CardSO>("CardSO");
        allCards.AddRange(cards);
        ShuffleDeck();
        drawDeckBar.Init(allCards.Count);
        drawDeckBar.UpdateBar(allCards.Count);
    }

    public void DrawCard(HandManager handManager)
    {

        if (allCards.Count == 0)
        {

            return;
        }

        if (handManager.handCards.Count == handManager.maxHandSize) return;

        // Luôn bốc lá bài nằm ở trên cùng (vị trí số 0)
        CardSO nextCard = allCards[0];

        // Xóa ngay lập tức lá đó khỏi bộ bài rút
        allCards.RemoveAt(0);
        // Cập nhật thanh deck
        drawDeckBar.UpdateBar(allCards.Count);
        // Thêm vào tay người chơi
        handManager.AddCardToHand(nextCard);
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            int randomIndex = Random.Range(0, allCards.Count);
            CardSO temp = allCards[i];
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }
    }
    public void RefreshDeckBar()
    {
        drawDeckBar.UpdateBar(allCards.Count);
    }
    public IEnumerator AddCardFromFirst(float duration = 0.3f)
    {
        for (int i = 0; i < handManager.maxHandSize; i++)
        {
            yield return new WaitForSeconds(duration);
            DrawCard(handManager);
        }

        // BattleManager.Instance.ChangeState(BattleState.PlayerTurn);
    }

    public List<string> GetDeckSaveData()
    {
        List<string> data = new();

        foreach (CardSO card in allCards)
            data.Add(card.name);

        return data;
    }

    public void LoadDeck(List<string> data)
    {
        allCards.Clear();

        foreach (string cardName in data)
        {
            CardSO card = Resources.Load<CardSO>("CardSO/" + cardName);

            if (card != null)
                allCards.Add(card);
        }

        RefreshDeckBar();
    }
}