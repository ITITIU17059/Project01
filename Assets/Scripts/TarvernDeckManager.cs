using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarvernDeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new();
    [SerializeField] private HandManager handManager;

    private void Start()
    {
        CardSO[] cards = Resources.LoadAll<CardSO>("CardSO");
        allCards.AddRange(cards);
        ShuffleDeck();
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

    public IEnumerator AddCardFromFirst(float duration = 0.3f)
    {
        for (int i = 0; i < handManager.maxHandSize; i++)
        {
            yield return new WaitForSeconds(duration);
            DrawCard(handManager);
        }

        BattleManager.Instance.ChangeState(BattleState.PlayerTurn);
    }
}