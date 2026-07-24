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
    private RewardSO[] equippedRewards =
     new RewardSO[MaxEquipSlots];

    public IReadOnlyList<RewardSO> OwnedRewards => ownedRewards;
    public RewardSO[] EquippedRewards => equippedRewards;

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
        foreach (RewardSO r in equippedRewards)
        {
            if (r == reward)
                return true;
        }

        return false;
    }

    public bool EquipReward(RewardSO reward, int slotIndex)
    {
        if (reward == null)
            return false;

        if (!ownedRewards.Contains(reward))
            return false;

        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
            return false;

        // Nếu reward này đã đang equip ở slot khác thì không cho equip
        for (int i = 0; i < equippedRewards.Length; i++)
        {
            if (equippedRewards[i] == reward)
                return false;
        }

        // Ghi đè reward cũ ở slot này
        equippedRewards[slotIndex] = reward;

        return true;
    }

    public bool UnequipReward(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
            return false;

        equippedRewards[slotIndex] = null;
        return true;
    }
}