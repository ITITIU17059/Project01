using System.Collections.Generic;
using UnityEngine;

public static class BossSkill
{
    public delegate int ValueModifier(int value);

    public delegate int DamageModifier(
    CardSO card,
    int originalDamage,
    int finalDamage);

    public delegate void TraitEvent();
    private static Dictionary<TraitID, Dictionary<TraitEventType, TraitEvent>> eventTable;

    private static Dictionary<TraitID, ValueModifier> drawModifiers;

    private static Dictionary<TraitID, ValueModifier> healModifiers;

    private static Dictionary<TraitID, ValueModifier> shieldModifiers;

    private static Dictionary<TraitID, DamageModifier> attackModifiers;

    static BossSkill()
    {
        InitializeEvents();

        InitializeDrawModifiers();
        InitializeHealModifiers();
        InitializeShieldModifiers();
        InitializeAttackModifiers();
    }

    private static void InitializeDrawModifiers()
    {
        drawModifiers = new Dictionary<TraitID, ValueModifier>();
        drawModifiers.Add(
      TraitID.J_GREEDY_TRIBUTE,
      JackGreedyTribute_Draw);
    }

    private static void InitializeHealModifiers()
    {
        healModifiers = new Dictionary<TraitID, ValueModifier>();

        healModifiers[TraitID.J_WITHERED_BLESSING]
            = JackWitheredBlessing_Heal;
    }

    private static void InitializeShieldModifiers()
    {
        shieldModifiers = new Dictionary<TraitID, ValueModifier>();

        shieldModifiers[TraitID.J_HEAVY_GUARD]
            = JackHeavyGuard_Shield;
    }

    private static void InitializeAttackModifiers()
    {
        attackModifiers = new Dictionary<TraitID, DamageModifier>();

        attackModifiers.Add(
            TraitID.J_BROKEN_FORCE,
            JackBrokenForce_Damage);
    }

    private static void InitializeEvents()
    {
        eventTable = new Dictionary<
            TraitID,
            Dictionary<TraitEventType, TraitEvent>>();

        RegisterEvent(
            TraitID.J_GREEDY_TRIBUTE,
            TraitEventType.PlayerTurn,
            JackGreedyTribute_PlayerTurn);

        RegisterEvent(
            TraitID.J_WITHERED_BLESSING,
            TraitEventType.Discard,
            JackWitheredBlessing_Discard);
    }


    public static void Invoke(
TraitEventType type,
BossTraitSO trait)
    {
        if (trait == null)
            return;

        if (!eventTable.ContainsKey(trait.traitID))
            return;

        if (eventTable[trait.traitID]
            .TryGetValue(type, out TraitEvent evt))
        {
            evt.Invoke();
        }
    }
    private static void RegisterEvent(
    TraitID id,
    TraitEventType type,
    TraitEvent evt)
    {
        if (!eventTable.ContainsKey(id))
        {
            eventTable[id] =
                new Dictionary<TraitEventType, TraitEvent>();
        }

        eventTable[id][type] = evt;
    }
    public static int ModifyDrawAmount(BossTraitSO trait, int amount)
    {
        if (trait == null)
            return amount;

        if (drawModifiers.TryGetValue(
            trait.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }

    public static int ModifyHealAmount(BossTraitSO trait, int amount)
    {
        if (trait == null)
            return amount;

        if (healModifiers.TryGetValue(
            trait.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }

    public static int ModifyShieldAmount(BossTraitSO trait, int amount)
    {
        if (trait == null)
            return amount;

        if (shieldModifiers.TryGetValue(
            trait.traitID,
            out ValueModifier modifier))
        {
            return modifier(amount);
        }

        return amount;
    }

    public static int ModifyAttackDamage(
    BossTraitSO trait,
    CardSO card,
    int originalDamage,
    int finalDamage)
    {
        if (trait == null)
            return finalDamage;

        if (attackModifiers.TryGetValue(
            trait.traitID,
            out DamageModifier modifier))
        {
            return modifier(card, originalDamage, finalDamage);
        }

        return finalDamage;
    }
    //==================================================
    // JACK
    //==================================================
    private static void JackGreedyTribute_PlayerTurn()
    {
        Debug.Log("Boss Trait Event : Greedy Tribute");

        BattleManager.Instance.DrawBonusCards(1);
    }
    private static void JackWitheredBlessing_Discard()
    {
        Debug.Log("Boss Trait Event : Withered Blessing");

        BattleManager.Instance.HealDeck(2);
    }
    private static int JackGreedyTribute_Draw(int amount)
    {
        Debug.Log("Jack Greedy Tribute Activated");

        return Mathf.Min(amount, 3);
    }
    private static int JackHeavyGuard_Shield(int amount)
    {
        Debug.Log("Jack Heavy Guard Activated");

        if (amount > 5)
            amount -= 3;

        return amount;
    }
    private static int JackWitheredBlessing_Heal(int amount)
    {
        Debug.Log("Jack Withered Blessing Activated");

        return 1;
    }
    private static int JackBrokenForce_Damage(
    CardSO card,
    int originalDamage,
    int finalDamage)
    {
        if (card == null)
            return finalDamage;

        if (card.suit != CardSO.Suit.Clubs)
            return finalDamage;

        if (card.value >= 6)
        {
            Debug.Log("Jack Broken Force Activated");
            return originalDamage;
        }

        return finalDamage;
    }
}