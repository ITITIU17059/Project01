using UnityEngine;

public class BossTraitSO : ScriptableObject
{
    [Header("TraitId")]
    public TraitID traitID;

    [Header("UI")]
    public Sprite icon;

    public string traitName;

    [TextArea]
    public string description;

    [Header("Reward")]
    public RewardSO reward;
}