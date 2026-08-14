using UnityEngine;
using DG.Tweening;

public class CardDisplay : MonoBehaviour
{
    public CardSO cardScriptableObject;
    public SpriteRenderer cardSpriteRenderer;
    private bool isHidden = false;
    private bool isFade = false;

    public bool IsHidden => isHidden;
    public bool IsFade => isFade;

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

    public void SetFade(bool value)
    {
        isFade = value;

        if (value)
            cardSpriteRenderer.color = Color.gray;
        else
            cardSpriteRenderer.color = Color.white;
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