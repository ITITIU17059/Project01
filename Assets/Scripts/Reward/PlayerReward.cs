using System.Collections.Generic;
using UnityEngine;

public class PlayerReward : MonoBehaviour
{
    public static PlayerReward Instance;

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
}