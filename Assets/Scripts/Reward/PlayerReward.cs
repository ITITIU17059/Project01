using System.Collections.Generic;
using UnityEngine;

public class PlayerReward : MonoBehaviour
{
   
    public static PlayerReward Instance { get; private set; }

     public const int MaxEquipSlots = 3;

     [SerializeField] private List<RewardSO> ownedRewards = new();

    [SerializeField] private RewardSO[] equippedRewards = new RewardSO[MaxEquipSlots];
    public IReadOnlyList<RewardSO> OwnedRewards => ownedRewards;
    public RewardSO[] EquippedRewards => equippedRewards;
    private void Awake()
    {
        if (equippedRewards == null || equippedRewards.Length != MaxEquipSlots)
        {
            equippedRewards = new RewardSO[MaxEquipSlots];
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public bool AddReward(RewardSO reward)
    {
        if (reward == null)
            return false;

        if (ownedRewards.Contains(reward))
        {
           
            return false;
        }

        ownedRewards.Add(reward);
        
        return true;
    }

    public bool IsEquipped(RewardSO reward)
    {
        foreach (RewardSO r in equippedRewards)
        {
            if (r == reward)
                return true;
        }

        return false;
    }

    public bool EquipReward(RewardSO reward, int slotIndex)
    {
        Debug.Log($"Equip {reward.rewardName} -> Slot {slotIndex}");

        if (reward == null)
        {
            Debug.Log("Reward NULL");
            return false;
        }

        if (!ownedRewards.Contains(reward))
        {
            Debug.Log("Reward chưa sở hữu");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
        {
            Debug.Log("Slot không hợp lệ");
            return false;
        }

        for (int i = 0; i < equippedRewards.Length; i++)
        {
            if (equippedRewards[i] == reward)
            {
                equippedRewards[i] = null;
                break;
            }
        }

        equippedRewards[slotIndex] = reward;

        Debug.Log("Equip thành công!");

        return true;
    
}

    public bool UnequipReward(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
            return false;

        equippedRewards[slotIndex] = null;
        return true;
    }
    public void SwapReward(int fromSlot, int toSlot)
    {
        if (fromSlot == toSlot)
            return;

        RewardSO temp = equippedRewards[fromSlot];
        equippedRewards[fromSlot] = equippedRewards[toSlot];
        equippedRewards[toSlot] = temp;
    }
}