using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Reward Storage")]
    [SerializeField] private RewardSlot rewardSlotPrefab;
    [SerializeField] private Transform rewardContent;

    [Header("Equip Slots")]
    [SerializeField] private EquipSlot[] equipSlots;

    private readonly List<RewardSlot> rewardSlots = new();
    private RewardSO selectedReward;
    private void Start()
    {
        RefreshRewardList();
        RefreshEquipSlots();
    }

    public void RefreshRewardList()
    {


        ClearRewardList();

        foreach (RewardSO reward in PlayerReward.Instance.OwnedRewards)
        {


            RewardSlot slot = Instantiate(rewardSlotPrefab, rewardContent);

            slot.Setup(reward, this);

            rewardSlots.Add(slot);
        }
    }

    private void ClearRewardList()
    {
        foreach (RewardSlot slot in rewardSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        rewardSlots.Clear();
    }

    public void RefreshEquipSlots()
    {
        RewardSO[] equipped = PlayerReward.Instance.EquippedRewards;

        for (int i = 0; i < equipSlots.Length; i++)
        {
            Debug.Log($"Refresh Slot {i} = {(equipped[i] == null ? "NULL" : equipped[i].rewardName)}");

            equipSlots[i].Setup(equipped[i], this);
        }
    }
    public void OnRewardDropped(RewardSO reward, int slotIndex)
    {
        Debug.Log($"Drop: {reward.rewardName} -> Slot {slotIndex}");

        bool success = PlayerReward.Instance.EquipReward(reward, slotIndex);

        Debug.Log("Equip Success = " + success);

        if (success)
        {
            RefreshRewardList();
            RefreshEquipSlots();
        }

        DragManager.Instance.EndDrag();
    }
}