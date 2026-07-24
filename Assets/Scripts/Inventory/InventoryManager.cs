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
        Debug.Log("PlayerReward = " + PlayerReward.Instance);
        Debug.Log("Prefab = " + rewardSlotPrefab);
        Debug.Log("Content = " + rewardContent);

        ClearRewardList();

        foreach (RewardSO reward in PlayerReward.Instance.OwnedRewards)
        {
            Debug.Log("Create : " + reward.rewardName);

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

    public void OnRewardClicked(RewardSO reward)
    {
        selectedReward = reward;

        Debug.Log("Selected Reward : " + reward.rewardName);
    }

    public void OnEquipSlotClicked(int slotIndex)
    {
        if (selectedReward == null)
        {
            PlayerReward.Instance.UnequipReward(slotIndex);
        }
        else
        {
            PlayerReward.Instance.EquipReward(selectedReward, slotIndex);
            selectedReward = null;
        }

        RefreshRewardList();
        RefreshEquipSlots();
    }

    public void RefreshEquipSlots()
    {
        RewardSO[] equipped = PlayerReward.Instance.EquippedRewards;


        for (int i = 0; i < Mathf.Min(equipSlots.Length, equipped.Length); i++)
        {
            Debug.Log($"Setup Slot {i}");

            if (equipSlots[i] == null)
            {
                Debug.LogError($"EquipSlot {i} is NULL");
                continue;
            }

            equipSlots[i].Setup(equipped[i], this);
        }
    }
    public void OnRewardDropped(RewardSO reward, int slotIndex)
    {
        if (PlayerReward.Instance.EquipReward(reward, slotIndex))
        {
            DragManager.Instance.EndDrag();

            RefreshRewardList();
            RefreshEquipSlots();
        }
    }
}