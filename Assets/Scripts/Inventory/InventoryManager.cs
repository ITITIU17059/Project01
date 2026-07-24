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

    private void Start()
    {
        //RefreshRewardList();
    }

    //public void RefreshRewardList()
    //{
    //    ClearRewardList();

    //    List<RewardSO> rewards = PlayerReward.Instance.OwnedRewards;

    //    foreach (RewardSO reward in rewards)
    //    {
    //        RewardSlot slot =
    //            Instantiate(rewardSlotPrefab, rewardContent);

    //        slot.Setup(
    //        reward,
    //        this);

    //        rewardSlots.Add(slot);
    //    }
    //}

    private void ClearRewardList()
    {
        foreach (RewardSlot slot in rewardSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        rewardSlots.Clear();
    }

    public EquipSlot GetFirstEmptySlot()
    {
        foreach (EquipSlot slot in equipSlots)
        {
            if (slot.CurrentReward == null)
                return slot;
        }

        return null;
    }

    public void RefreshEquipSlots()
    {
        IReadOnlyList<RewardSO> equipped =
            PlayerReward.Instance.EquippedRewards;

        for (int i = 0; i < equipSlots.Length; i++)
        {
            RewardSO reward = null;

            if (i < equipped.Count)
                reward = equipped[i];

            equipSlots[i].Setup(reward);
        }
    }
}