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
    IPointerExitHandler
{
    [SerializeField] private Image icon;
    private Color imageColor;

    private RewardSO reward;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        imageColor = icon.color;
    }

    public void Setup(RewardSO reward, InventoryManager owner)
    {
        this.reward = reward;

        icon.sprite = reward.icon;
        icon.enabled = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (reward == null)
            return;

        canvasGroup.blocksRaycasts = false;

        DragManager.Instance.BeginDrag(reward, -1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instance.Drag(eventData.position);
        imageColor.a = 0.2f;
        icon.color = imageColor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        imageColor.a = 1f;
        icon.color = imageColor;

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
}