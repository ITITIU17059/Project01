using UnityEngine;
using DG.Tweening;

public class CardDisplay : MonoBehaviour
{
    public CardSO cardScriptableObject;
    public SpriteRenderer cardSpriteRenderer;
    private bool isHidden = false;

    public bool IsHidden => isHidden;

    [SerializeField] private Sprite cardBackSprite;
    void Start()
    {
        UpdateCardDisplay();
        if (cardScriptableObject != null && cardSpriteRenderer != null)
        {
            cardSpriteRenderer.sortingLayerName = "UI";
        }
    }
    public void SetHidden(bool value)
    {
        isHidden = value;

        if (value)
            cardSpriteRenderer.sprite = cardBackSprite;
        else
            cardSpriteRenderer.sprite = cardScriptableObject.cardSprite;
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