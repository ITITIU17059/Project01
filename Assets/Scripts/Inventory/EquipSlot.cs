using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
public class EquipSlot :
MonoBehaviour,
IDropHandler,
IBeginDragHandler,
IDragHandler,
IEndDragHandler,
IPointerEnterHandler,
IPointerExitHandler,
IPointerClickHandler

{
    [SerializeField] private Image icon;

    [SerializeField] private int slotIndex;
    private RewardSO currentReward;

    public RewardSO CurrentReward => currentReward;
    [SerializeField] private CanvasGroup canvasGroup;

    private InventoryManager owner;
    public RectTransform IconRect => icon.rectTransform;

    public void Setup(RewardSO reward, InventoryManager inventory)
    {
        owner = inventory;
        currentReward = reward;

        bool isEmpty = reward == null;

        icon.enabled = !isEmpty;

        if (!isEmpty)
        {
            icon.sprite = reward.icon;
            // icon.SetNativeSize(); // thêm dòng này
        }
    }


    public void Refresh()
    {
        Setup(currentReward, owner);
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (DragManager.Instance.DraggingReward == null)
            return;

        // Drag từ Equip Slot khác
        if (DragManager.Instance.DraggingEquipSlot != -1)
        {
            PlayerReward.Instance.SwapReward(
                DragManager.Instance.DraggingEquipSlot,
                slotIndex);

            owner.RefreshEquipSlots();

            DragManager.Instance.EndDrag();

            return;
        }

        // Drag từ Inventory
        owner.OnRewardDropped(
            DragManager.Instance.DraggingReward,
            slotIndex);

        DragManager.Instance.EndDrag();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentReward == null)
            return;


        canvasGroup.alpha = 0f;

        DragManager.Instance.BeginDrag(currentReward, slotIndex);
    }
    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instance.Drag(eventData.position);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;

        DragManager.Instance.EndDrag();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentReward != null)
            TooltipUI.Instance.Show(currentReward);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentReward == null)
            return;

        owner.OnEquipClicked(slotIndex);
    }
    public void Clear()
    {
        currentReward = null;
        icon.enabled = false;
    }
}