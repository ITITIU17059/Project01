using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Reward Storage")]
    [SerializeField] private RewardSlot rewardSlotPrefab;
    [SerializeField] private Transform rewardContent;

    [Header("Equip Slots")]
    [SerializeField] private EquipSlot[] equipSlots;

    private readonly List<RewardSlot> rewardSlots = new();
    [SerializeField] private GameObject notificationObject;
    [SerializeField] private InventoryAnimationManager animationManager;
    private void Start()
    {
        MusicManager.instance.PlayMusic("InventoryTheme");
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

            equipSlots[i].Setup(equipped[i], this);
        }
    }
    public void OnRewardDropped(RewardSO reward, int slotIndex)
    {
        bool success = PlayerReward.Instance.EquipReward(reward, slotIndex);

        if (success)
        {
            RefreshRewardList();
            RefreshEquipSlots();
        }

        DragManager.Instance.EndDrag();
    }

    public void OnInventoryClicked(RewardSlot slot)
    {
        if (PlayerReward.Instance.IsFull())
        {
            StartCoroutine(ShowNotification());
            return;
        }

        int target =
            PlayerReward.Instance.GetFirstEmptySlot();

        StartCoroutine(
            animationManager.PlayEquip(
                slot,
                equipSlots[target],
                () =>
                {
                    PlayerReward.Instance.EquipReward(
                        slot.Reward,
                        target);

                    RefreshRewardList();
                    RefreshEquipSlots();
                }));
    }

    public void OnEquipClicked(int slot)
    {
        RewardSO reward =
            PlayerReward.Instance.EquippedRewards[slot];

        RewardSlot rewardSlot =
            rewardSlots.Find(x => x.Reward == reward);

        StartCoroutine(
            animationManager.PlayUnequip(
                equipSlots[slot],
                rewardSlot,
                () =>
                {
                    PlayerReward.Instance.UnequipReward(slot);

                    RefreshRewardList();
                    RefreshEquipSlots();
                }));
    }

    private IEnumerator ShowNotification()
    {
        notificationObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        notificationObject.SetActive(false);
    }
}