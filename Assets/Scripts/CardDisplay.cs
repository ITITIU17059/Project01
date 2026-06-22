using UnityEngine;
using DG.Tweening;

public class CardDisplay : MonoBehaviour
{
    public CardSO cardScriptableObject;
    public SpriteRenderer cardSpriteRenderer;

    void Start()
    {
        UpdateCardDisplay();
    }

    void UpdateCardDisplay()
    {
        if (cardScriptableObject != null && cardSpriteRenderer != null)
        {
            cardSpriteRenderer.sprite = cardScriptableObject.cardSprite;
        }
    }

    public void SetSortingOrder(int order)
    {
        if (cardSpriteRenderer != null)
        {
            cardSpriteRenderer.sortingOrder = order;
        }
    }

    public int GetCurrentSortingOrder()
    {
        return cardSpriteRenderer ? cardSpriteRenderer.sortingOrder : 0;
    }
}