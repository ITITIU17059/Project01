using System.Collections.Generic;
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


    public int ModifyDrawAmount(int amount)
    {
        return BossSkill.ModifyDrawAmount(CurrentTrait, amount);
    }

    public int ModifyHealAmount(int amount)
    {
        return BossSkill.ModifyHealAmount(CurrentTrait, amount);
    }

    public int ModifyShieldAmount(
     List<CardSO> cards,
     int amount)
    {
        return BossSkill.ModifyShieldAmount(
            CurrentTrait,
            cards,
            amount);
    }

    public int ModifyAttackDamage(
       List<CardSO> cards,
       int originalDamage,
       int finalDamage)
    {
        return BossSkill.ModifyAttackDamage(
            CurrentTrait,
            cards,
            originalDamage,
            finalDamage);
    }


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

    public int ModifyRewardAttackDamage(
      CardSO card,
      int originalDamage,
      int finalDamage)
    {
        return RewardSkill.ModifyAttackDamage(
            PlayerReward.Instance.EquippedRewards,
            card,
            originalDamage,
            finalDamage);
    }

    public void InvokeBossEvent(
     TraitEventType type,
     int value = 0)
    {
        BossSkill.Invoke(
            type,
            CurrentTrait,
            value);
    }

    public void InvokeRewardEvent(TraitEventType type, int value = 0)
    {
        RewardSkill.Invoke(type, PlayerReward.Instance.EquippedRewards, value);
    }
}