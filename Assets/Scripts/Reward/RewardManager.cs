using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [SerializeField]
    private RewardSO currentReward;

    public RewardSO CurrentReward => currentReward;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EquipReward(RewardSO reward)
    {
        currentReward = reward;
    }

    public void UnequipReward()
    {
        currentReward = null;
    }
}