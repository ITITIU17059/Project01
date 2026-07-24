using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
   
    
    [SerializeField] private int slotIndex;
    private RewardSO currentReward;

    public RewardSO CurrentReward => currentReward;

    private void Awake()
    {

    }

    private InventoryManager owner;

    public void Setup(RewardSO reward, InventoryManager inventory)
    {
        currentReward = reward;
        owner = inventory;

        bool isEmpty = reward == null;

        icon.enabled = !isEmpty;

        if (!isEmpty)
            icon.sprite = reward.icon;

        nameText.text = isEmpty ? "" : reward.rewardName;
    }


    public void Refresh()
    {
        Setup(currentReward, owner);
    }
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("===== DROP =====");

        Debug.Log("Owner = " + owner);
        Debug.Log("DragManager = " + DragManager.Instance);
        Debug.Log("Reward = " + DragManager.Instance.DraggingReward);

        if (DragManager.Instance.DraggingReward == null)
            return;

        owner.OnRewardDropped(
            DragManager.Instance.DraggingReward,
            slotIndex);

        DragManager.Instance.EndDrag();
    }
}