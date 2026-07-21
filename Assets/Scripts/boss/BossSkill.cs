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

    private static Dictionary<TraitID, ValueModifier> drawModifiers;

    private static Dictionary<TraitID, ValueModifier> healModifiers;

    private static Dictionary<TraitID, ValueModifier> shieldModifiers;

    private static Dictionary<TraitID, DamageModifier> attackModifiers;

    private static Dictionary<TraitID, TraitEvent> playerTurnEvents;

    private static Dictionary<TraitID, TraitEvent> bossTurnEvents;

    private static Dictionary<TraitID, TraitEvent> discardEvents;

    private static Dictionary<TraitID, TraitEvent> cardPlayedEvents;
        static BossSkill()
    {
        InitializeDrawModifiers();
        InitializeHealModifiers();
        InitializeShieldModifiers();
        InitializeAttackModifiers();

        InitializePlayerTurnEvents();
        InitializeBossTurnEvents();
        InitializeDiscardEvents();
        InitializeCardPlayedEvents();
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



    private static void InitializePlayerTurnEvents()
    {
        playerTurnEvents = new Dictionary<TraitID, TraitEvent>();
    }

    private static void InitializeBossTurnEvents()
    {
        bossTurnEvents = new Dictionary<TraitID, TraitEvent>();
    }

    private static void InitializeDiscardEvents()
    {
        discardEvents = new Dictionary<TraitID, TraitEvent>();
    }

    private static void InitializeCardPlayedEvents()
    {
        cardPlayedEvents = new Dictionary<TraitID, TraitEvent>();
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