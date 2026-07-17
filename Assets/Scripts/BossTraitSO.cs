using UnityEngine;

public abstract class BossTraitSO : ScriptableObject
{
    public string traitName;

    [TextArea]
    public string description;
}