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
    // Trait tạm thời (chỉ trong trận này)
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

    public int ModifyAttackDamage(CardSO card, int originalDamage, int finalDamage)
    {
        return BossSkill.ModifyAttackDamage(CurrentTrait, card, originalDamage, finalDamage);
    }

    //====================================================
    // Reward vĩnh viễn (đã trang bị từ Inventory, tối đa 3)
    //====================================================

    public int ModifyRewardDrawAmount(int amount)
    {
        return RewardSkill.ModifyDrawAmount(PlayerReward.Instance.EquippedRewards, amount);
    }

    public int ModifyRewardHealAmount(int amount)
    {
        return RewardSkill.ModifyHealAmount(PlayerReward.Instance.EquippedRewards, amount);
    }

    public int ModifyRewardShieldAmount(int amount)
    {
        return RewardSkill.ModifyShieldAmount(PlayerReward.Instance.EquippedRewards, amount);
    }

    public int ModifyRewardAttackDamage(CardSO card, int original, int finalDamage)
    {
        return RewardSkill.ModifyAttackDamage(PlayerReward.Instance.EquippedRewards, card, original, finalDamage);
    }

    public void InvokeBossEvent(TraitEventType type)
    {
        BossSkill.Invoke(type, CurrentTrait);
    }

    public void InvokeRewardEvent(TraitEventType type)
    {
        RewardSkill.Invoke(type, PlayerReward.Instance.EquippedRewards);
    }
}