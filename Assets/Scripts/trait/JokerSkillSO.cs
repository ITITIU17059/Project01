using UnityEngine;

public enum JokerSkillTrigger
{
    BattleStart,
    PlayerTurn,
    BossTurn,
    Draw,
    PlayCard,
    AfterAttack,
    BossDamaged,
    Discard
}

[CreateAssetMenu(menuName = "Boss/Joker Skill")]
public class JokerSkillSO : ScriptableObject
{
    public string skillName;
    public TraitID traitID;
    public JokerSkillTrigger trigger;
}