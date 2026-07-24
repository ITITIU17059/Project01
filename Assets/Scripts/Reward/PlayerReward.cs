using System.Collections.Generic;
using UnityEngine;

public class PlayerReward : MonoBehaviour
{
   
    public static PlayerReward Instance { get; private set; }


    private void Awake()
    {
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

    public const int MaxEquipSlots = 3;

    [SerializeField]
    private List<RewardSO> ownedRewards = new();

    [SerializeField]
    private List<RewardSO> equippedRewards = new();

    public IReadOnlyList<RewardSO> OwnedRewards => ownedRewards;
    public IReadOnlyList<RewardSO> EquippedRewards => equippedRewards;

    public bool AddReward(RewardSO reward)
    {
        if (reward == null)
            return false;

        if (ownedRewards.Contains(reward))
        {
            Debug.LogWarning("Reward already owned : " + reward.rewardName);
            return false;
        }

        ownedRewards.Add(reward);
        Debug.Log("Receive Reward : " + reward.rewardName);
        return true;
    }

    public bool IsEquipped(RewardSO reward)
    {
        return reward != null && equippedRewards.Contains(reward);
    }

    public bool EquipReward(RewardSO reward)
    {
        if (reward == null)
            return false;

        if (!ownedRewards.Contains(reward))
        {
            Debug.LogWarning("Cannot equip, reward not owned : " + reward.rewardName);
            return false;
        }

        if (equippedRewards.Contains(reward))
            return false;

        if (equippedRewards.Count >= MaxEquipSlots)
        {
            Debug.LogWarning("Equip slots full (" + MaxEquipSlots + ")");
            return false;
        }

        equippedRewards.Add(reward);
        return true;
    }

    public bool UnequipReward(RewardSO reward)
    {
        if (reward == null)
            return false;

        return equippedRewards.Remove(reward);
    }
}