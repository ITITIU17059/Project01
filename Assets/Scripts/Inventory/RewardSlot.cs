using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class RewardSlot :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private Image icon;
    private Color imageColor;

    private RewardSO reward;

    private CanvasGroup canvasGroup;
    private bool isEquipped;
    private InventoryManager owner;
    public RewardSO Reward => reward;
    public RectTransform IconRect => icon.rectTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        imageColor = icon.color;
    }

    public void Setup(RewardSO reward, InventoryManager owner)
    {
        this.owner = owner;
        this.reward = reward;

        icon.sprite = reward.icon;
        icon.enabled = true;

        isEquipped = PlayerReward.Instance.IsEquipped(reward);

        Color c = icon.color;
        c.a = isEquipped ? 0.3f : 1f;
        icon.color = c;

        canvasGroup.interactable = !isEquipped;
        canvasGroup.blocksRaycasts = !isEquipped;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (reward == null || isEquipped)
            return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.3f;

        DragManager.Instance.BeginDrag(reward, -1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isEquipped)
            return;

        DragManager.Instance.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isEquipped)
            return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = isEquipped ? 0.3f : 1f;

        DragManager.Instance.EndDrag();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (reward != null)
            TooltipUI.Instance.Show(reward);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (reward == null)
            return;

        owner.OnInventoryClicked(this);
    }
}