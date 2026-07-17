using UnityEngine;
using DG.Tweening;

public class CardDisplay : MonoBehaviour
{
    public CardSO cardScriptableObject;
    public SpriteRenderer cardSpriteRenderer;

    void Start()
    {
        UpdateCardDisplay();
        if (cardScriptableObject != null && cardSpriteRenderer != null)
        {
            cardSpriteRenderer.sortingLayerName = "UI";
        }
    }

    void UpdateCardDisplay()
    {
        if (cardScriptableObject != null && cardSpriteRenderer != null)
        {
            cardSpriteRenderer.sprite = cardScriptableObject.cardSprite;
        }
    }
    private void OnDestroy()
    {
        transform.DOKill();
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