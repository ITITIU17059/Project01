using System.Collections.Generic;
using UnityEngine;

public static class RewardSkill
{
    public delegate int ValueModifier(int value);

    public delegate int DamageModifier(CardSO card, int originalValue, int finalValue);

    private static Dictionary<TraitID, ValueModifier> drawModifiers;
    private static Dictionary<TraitID, ValueModifier> healModifiers;
    private static Dictionary<TraitID, ValueModifier> shieldModifiers;
    private static Dictionary<TraitID, DamageModifier> attackModifiers;

    static RewardSkill()
    {
        InitializeDrawModifiers();
        InitializeHealModifiers();
        InitializeShieldModifiers();
        InitializeAttackModifiers();
    }
    private static void InitializeDrawModifiers()
    {
        drawModifiers = new();
    }

    private static void InitializeHealModifiers()
    {
        healModifiers = new();
    }

    private static void InitializeShieldModifiers()
    {
        shieldModifiers = new();
    }

    private static void InitializeAttackModifiers()
    {
        attackModifiers = new();
    }
    public static int ModifyDrawAmount(
        RewardSO reward,
        int amount)
    {
        if (reward == null)
            return amount;

        if (drawModifiers.TryGetValue(
            reward.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }
    public static int ModifyHealAmount(
        RewardSO reward,
        int amount)
    {
        if (reward == null)
            return amount;

        if (healModifiers.TryGetValue(
            reward.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }
    public static int ModifyShieldAmount(
        RewardSO reward,
        int amount)
    {
        if (reward == null)
            return amount;

        if (shieldModifiers.TryGetValue(
            reward.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }
    public static int ModifyAttackDamage(
           RewardSO reward,
           CardSO card,
           int originalValue,
           int finalValue)
    {
        if (reward == null)
            return finalValue;

        if (attackModifiers.TryGetValue(
            reward.traitID,
            out DamageModifier modifier))
        {
            return modifier(
                card,
                originalValue,
                finalValue);
        }

        return finalValue;
    }
}