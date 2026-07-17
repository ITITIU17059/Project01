using UnityEngine;

public abstract class RewardSO : ScriptableObject
{
    public string rewardName;

    [TextArea]
    public string description;
}