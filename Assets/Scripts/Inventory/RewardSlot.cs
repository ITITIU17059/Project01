using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class RewardSlot :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image icon;

    private RewardSO reward;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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

        DragManager.Instance.BeginDrag(reward);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instance.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        DragManager.Instance.EndDrag();
    }
}