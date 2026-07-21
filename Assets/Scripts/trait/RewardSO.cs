using UnityEngine;

[CreateAssetMenu(fileName = "Reward", menuName = "Boss/Reward")]
public class RewardSO : ScriptableObject
{
    [Header("TraitId")]
    public TraitID traitID;

    [Header("UI")]
    public Sprite icon;

    public string rewardName;

    [TextArea]
    public string description;
}