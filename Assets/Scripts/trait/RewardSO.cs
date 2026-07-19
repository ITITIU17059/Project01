using UnityEngine;

[CreateAssetMenu(fileName = "Reward", menuName = "Boss/Reward")]
public class RewardSO : ScriptableObject
{
    [Header("UI")]
    public Sprite icon;

    public string rewardName;

    [TextArea]
    public string description;
}