using System.Collections.Generic;
using UnityEngine;

public static class RewardSkill
{
    public delegate int ValueModifier(int value);
    public delegate int DamageModifier(CardSO card, int originalValue, int finalValue);
    public delegate void RewardEvent();

    private static Dictionary<TraitID, RewardEvent> playerTurnEvents;
    private static Dictionary<TraitID, RewardEvent> discardEvents;
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
        InitializePlayerTurnEvents();
        InitializeDiscardEvents();
    }

    private static void InitializePlayerTurnEvents()
    {
        playerTurnEvents = new();
        playerTurnEvents.Add(TraitID.J_GREEDY_TRIBUTE, JackGreedyTribute_PlayerTurnReward);
    }

    private static void InitializeDiscardEvents()
    {
        discardEvents = new();
        discardEvents.Add(TraitID.J_WITHERED_BLESSING, JackWitheredBlessing_DiscardReward);
    }

    private static void InitializeDrawModifiers() { drawModifiers = new(); }
    private static void InitializeHealModifiers() { healModifiers = new(); }

    private static void InitializeShieldModifiers()
    {
        shieldModifiers = new();
        shieldModifiers.Add(TraitID.J_HEAVY_GUARD, JackHeavyGuard_ShieldReward);
    }

    private static void InitializeAttackModifiers()
    {
        attackModifiers = new();
        attackModifiers.Add(TraitID.J_BROKEN_FORCE, JackBrokenForce_AttackReward);
    }

    public static int ModifyDrawAmount(IReadOnlyList<RewardSO> equippedRewards, int amount)
    {
        if (equippedRewards == null) return amount;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (drawModifiers.TryGetValue(reward.traitID, out ValueModifier modifier))
                amount = modifier(amount);
        }
        return amount;
    }

    public static int ModifyHealAmount(IReadOnlyList<RewardSO> equippedRewards, int amount)
    {
        if (equippedRewards == null) return amount;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (healModifiers.TryGetValue(reward.traitID, out ValueModifier modifier))
                amount = modifier(amount);
        }
        return amount;
    }

    public static int ModifyShieldAmount(IReadOnlyList<RewardSO> equippedRewards, int amount)
    {
        if (equippedRewards == null) return amount;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (shieldModifiers.TryGetValue(reward.traitID, out ValueModifier modifier))
                amount = modifier(amount);
        }
        return amount;
    }

    public static int ModifyAttackDamage(IReadOnlyList<RewardSO> equippedRewards, CardSO card, int originalValue, int finalValue)
    {
        if (equippedRewards == null) return finalValue;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (attackModifiers.TryGetValue(reward.traitID, out DamageModifier modifier))
                finalValue = modifier(card, originalValue, finalValue);
        }
        return finalValue;
    }

    private static void JackGreedyTribute_PlayerTurnReward()
    {
        
        BattleManager.Instance.DrawBonusCards(1);
    }

    private static void JackWitheredBlessing_DiscardReward()
    {
        Debug.Log("Reward J2 Activated");
        BattleManager.Instance.HealDeck(2);
    }

    private static int JackHeavyGuard_ShieldReward(int amount)
    {
       
        return amount + 3;
    }

    private static int JackBrokenForce_AttackReward(CardSO card, int originalValue, int finalValue)
    {
        
        if (card == null) return finalValue;
        if (card.suit != CardSO.Suit.Clubs) return finalValue;
        if (card.value >= 6) return finalValue;
        return originalValue * 3;
    }

    public static void InvokePlayerTurn(IReadOnlyList<RewardSO> equippedRewards)
    {
        if (equippedRewards == null) return;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (playerTurnEvents.TryGetValue(reward.traitID, out RewardEvent evt))
                evt.Invoke();
        }
    }

    public static void InvokeDiscard(IReadOnlyList<RewardSO> equippedRewards)
    {
        if (equippedRewards == null) return;
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null) continue;
            if (discardEvents.TryGetValue(reward.traitID, out RewardEvent evt))
                evt.Invoke();
        }
    }
}