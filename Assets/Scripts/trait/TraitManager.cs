using UnityEngine;

public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //====================================================
    // Current Trait
    //====================================================

    public BossTraitSO CurrentTrait
    {
        get
        {
            if (BossManager.Instance == null)
                return null;

            if (BossManager.Instance.CurrentBoss == null)
                return null;

            return BossManager.Instance.CurrentBoss.currentTrait;
        }
    }

    public bool HasTrait => CurrentTrait != null;

    //====================================================
    // Event
    //====================================================

    public int ModifyDrawAmount(int amount)
    {
        return BossSkill.ModifyDrawAmount(CurrentTrait, amount);
    }

    public int ModifyHealAmount(int amount)
    {
        return BossSkill.ModifyHealAmount(CurrentTrait, amount);
    }

    public int ModifyShieldAmount(int amount)
    {
        return BossSkill.ModifyShieldAmount(CurrentTrait, amount);
    }

    public int ModifyAttackDamage(
     CardSO card,
     int originalDamage,
     int finalDamage)
    {
        return BossSkill.ModifyAttackDamage(
            CurrentTrait,
            card,
            originalDamage,
            finalDamage);
    }
    public int ModifyRewardDrawAmount(int amount)
    {
        return RewardSkill.ModifyDrawAmount(CurrentReward, amount);
    }

    public int ModifyRewardHealAmount(int amount)
    {
        return RewardSkill.ModifyHealAmount(CurrentReward, amount);
    }

    public int ModifyRewardShieldAmount(int amount)
    {
        return RewardSkill.ModifyShieldAmount(CurrentReward, amount);
    }

    public int ModifyRewardAttackDamage(CardSO card, int original, int finalDamage)
    {
        return RewardSkill.ModifyAttackDamage(CurrentReward, card, original, finalDamage);
    }


    public RewardSO CurrentReward
    {
        get
        {
            if (BossManager.Instance == null)
                return null;

            if (BossManager.Instance.CurrentBoss == null)
                return null;

            return BossManager.Instance.CurrentBoss.currentReward;
        }
    }
}