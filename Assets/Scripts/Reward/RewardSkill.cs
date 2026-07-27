using System.Collections.Generic;
using UnityEngine;

public static class RewardSkill
{
    public delegate int ValueModifier(int value);
    public delegate int DamageModifier(CardSO card, int originalValue, int finalValue);
    public delegate void RewardEvent();

    private static Dictionary<TraitID, Dictionary<TraitEventType, RewardEvent>> eventTable;

    private static Dictionary<TraitID, ValueModifier> drawModifiers;
    private static Dictionary<TraitID, ValueModifier> healModifiers;
    private static Dictionary<TraitID, ValueModifier> shieldModifiers;
    private static Dictionary<TraitID, DamageModifier> attackModifiers;


    private static readonly Dictionary<TraitID, CardSO.Suit> traitSuitMap = new()
    {
        { TraitID.J_GREEDY_TRIBUTE, CardSO.Suit.Diamonds },
        { TraitID.J_WITHERED_BLESSING, CardSO.Suit.Hearts },
        { TraitID.J_HEAVY_GUARD, CardSO.Suit.Spades },
    };

    private static bool IsResistedByCurrentBoss(TraitID traitID)
    {
        if (!traitSuitMap.TryGetValue(traitID, out CardSO.Suit requiredSuit))
            return false;

        BossSO currentBoss = BossManager.Instance != null
            ? BossManager.Instance.CurrentBoss
            : null;

        if (currentBoss == null)
            return false;

        return currentBoss.resistanceSuit == requiredSuit;
    }

    static RewardSkill()
    {
        InitializeEvents();

        InitializeDrawModifiers();
        InitializeHealModifiers();
        InitializeShieldModifiers();
        InitializeAttackModifiers();
    }

    //========================================================
    // EVENT
    //========================================================

    private static void InitializeEvents()
    {
        eventTable = new();

        RegisterEvent(
            TraitID.J_GREEDY_TRIBUTE,
            TraitEventType.PlayerTurn,
            JackGreedyTribute_PlayerTurnReward);

        RegisterEvent(
            TraitID.J_WITHERED_BLESSING,
            TraitEventType.Discard,
            JackWitheredBlessing_DiscardReward);
        RegisterEvent(
        TraitID.Q_LIFE_LEECH,
        TraitEventType.Draw,
            () => QueenLifeLeech_Draw(1));
    }

    private static void RegisterEvent(
        TraitID id,
        TraitEventType type,
        RewardEvent evt)
    {
        if (!eventTable.ContainsKey(id))
            eventTable[id] = new();

        eventTable[id][type] = evt;
    }

    public static void Invoke(
        TraitEventType type,
        IReadOnlyList<RewardSO> rewards,
        int value)
    {
        if (rewards == null)
            return;

        foreach (RewardSO reward in rewards)
        {
            if (reward == null)
                continue;

            if (!eventTable.ContainsKey(reward.traitID))
                continue;

            if (IsResistedByCurrentBoss(reward.traitID))
                continue;

            if (eventTable[reward.traitID]
                .TryGetValue(type, out RewardEvent evt))
            {
                evt.Invoke();
            }
        }
    }

    //========================================================
    // DRAW
    //========================================================

    private static void InitializeDrawModifiers()
    {
        drawModifiers = new();

        drawModifiers.Add(
            TraitID.J_GREEDY_TRIBUTE,
            JackGreedyTribute_DrawReward);
    }

    //========================================================
    // HEAL
    //========================================================

    private static void InitializeHealModifiers()
    {
        healModifiers = new();

        healModifiers.Add(
            TraitID.J_WITHERED_BLESSING,
            JackWitheredBlessing_HealReward);
    }

    //========================================================
    // SHIELD
    //========================================================

    private static void InitializeShieldModifiers()
    {
        shieldModifiers = new();

        shieldModifiers.Add(
            TraitID.J_HEAVY_GUARD,
            JackHeavyGuard_ShieldReward);
    }

    //========================================================
    // ATTACK
    //========================================================

    private static void InitializeAttackModifiers()
    {
        attackModifiers = new();

        attackModifiers.Add(
            TraitID.J_BROKEN_FORCE,
            JackBrokenForce_AttackReward);
    }


    //========================================================
    // MODIFY
    //========================================================

    public static int ModifyDrawAmount(
        IReadOnlyList<RewardSO> rewards,
        int amount)
    {
        if (rewards == null)
            return amount;

        foreach (RewardSO reward in rewards)
        {
            if (reward == null)
                continue;

            if (IsResistedByCurrentBoss(reward.traitID))
                continue;

            if (drawModifiers.TryGetValue(
                reward.traitID,
                out ValueModifier modifier))
            {
                amount = modifier(amount);
            }
        }

        return amount;
    }

    public static int ModifyHealAmount(
        IReadOnlyList<RewardSO> rewards,
        int amount)
    {
        if (rewards == null)
            return amount;

        foreach (RewardSO reward in rewards)
        {
            if (reward == null)
                continue;

            if (IsResistedByCurrentBoss(reward.traitID))
                continue;

            if (healModifiers.TryGetValue(
                reward.traitID,
                out ValueModifier modifier))
            {
                amount = modifier(amount);
            }
        }

        return amount;
    }

    public static int ModifyShieldAmount(
        IReadOnlyList<RewardSO> rewards,
        int amount)
    {
        if (rewards == null)
            return amount;

        foreach (RewardSO reward in rewards)
        {
            if (reward == null)
                continue;

            if (IsResistedByCurrentBoss(reward.traitID))
                continue;

            if (shieldModifiers.TryGetValue(
                reward.traitID,
                out ValueModifier modifier))
            {
                amount = modifier(amount);
            }
        }

        return amount;
    }

    public static int ModifyAttackDamage(
        IReadOnlyList<RewardSO> rewards,
        CardSO card,
        int originalValue,
        int finalValue)
    {
        if (rewards == null)
            return finalValue;

        foreach (RewardSO reward in rewards)
        {
            if (reward == null)
                continue;

            if (attackModifiers.TryGetValue(
                reward.traitID,
                out DamageModifier modifier))
            {
                finalValue =
                    modifier(
                        card,
                        originalValue,
                        finalValue);
            }
        }

        return finalValue;
    }

    //========================================================
    // JACK EVENT
    //========================================================

    private static void JackGreedyTribute_PlayerTurnReward()
    {

        BattleManager.Instance.DrawBonusCards(1);
    }

    private static void JackWitheredBlessing_DiscardReward()
    {

        BattleManager.Instance.HealDeck(2);
    }

    //========================================================
    // JACK DRAW
    //========================================================

    private static int JackGreedyTribute_DrawReward(int amount)
    {

        return amount + 1;
    }

    //========================================================
    // JACK HEAL
    //========================================================

    private static int JackWitheredBlessing_HealReward(int amount)
    {

        return amount + 2;
    }

    //========================================================
    // JACK SHIELD
    //========================================================

    private static int JackHeavyGuard_ShieldReward(int amount)
    {
        return amount + 3;
    }

    //========================================================
    // JACK ATTACK
    //========================================================

    private static int JackBrokenForce_AttackReward(
        CardSO card,
        int originalValue,
        int finalValue)
    {
        if (card == null)
            return finalValue;

        if (card.suit != CardSO.Suit.Clubs)
            return finalValue;

        if (card.value >= 6)
            return finalValue;

        return finalValue + originalValue;
    }
    private static void QueenLifeLeech_Draw(int value)
    {
        BossManager.Instance.TakeTraitDamage(1);
    }
}