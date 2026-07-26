using System.Collections.Generic;
using UnityEngine;

public static class BossSkill
{
    public delegate int ValueModifier(int value);

    public delegate int ShieldModifier(
     List<CardSO> cards,
     int amount);

    public delegate int DamageModifier(
       List<CardSO> cards,
       int original,
       int final);

    public delegate void TraitEvent();
    private static Dictionary<TraitID, Dictionary<TraitEventType, TraitEvent>> eventTable;

    private static Dictionary<TraitID, ValueModifier> drawModifiers;

    private static Dictionary<TraitID, ValueModifier> healModifiers;

    private static Dictionary<TraitID, ShieldModifier> shieldModifiers;

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
        shieldModifiers = new Dictionary<TraitID, ShieldModifier>();

        shieldModifiers.Add(
            TraitID.J_HEAVY_GUARD,
            JackHeavyGuard_Shield);
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

        // Lưu ý: Greedy Tribute và Withered Blessing KHÔNG đăng ký event
        // PlayerTurn/Discard ở đây nữa, vì hiệu ứng "+1 draw mỗi lượt" /
        // "+2 heal khi discard" là phần thưởng (RewardSkill) chỉ dành cho
        // người chơi ĐÃ hạ boss và equip reward tương ứng.
        // Lời nguyền (curse) của 2 trait này chỉ nên tác dụng qua
        // JackGreedyTribute_Draw / JackWitheredBlessing_Heal (giới hạn
        // draw/heal khi chơi combo bài), không phải cho không lợi ích
        // giống hệt reward ngay trong lúc đang đánh boss.
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

    public static int ModifyShieldAmount(
        BossTraitSO trait,
        List<CardSO> cards,
        int amount)
    {
        if (trait == null)
            return amount;

        if (shieldModifiers.TryGetValue(
            trait.traitID,
            out ShieldModifier modifier))
        {
            return modifier(cards, amount);
        }

        return amount;
    }

    public static int ModifyAttackDamage(
    BossTraitSO trait,
    List<CardSO> cards,
    int originalDamage,
    int finalDamage)
    {
        if (trait == null)
            return finalDamage;

        if (attackModifiers.TryGetValue(
            trait.traitID,
            out DamageModifier modifier))
        {
            return modifier(
                cards,
                originalDamage,
                finalDamage);
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
    private static int JackHeavyGuard_Shield(
        List<CardSO> cards,
        int amount)
    {
        if (cards == null)
            return amount;

        foreach (CardSO card in cards)
        {
            if (card.suit == CardSO.Suit.Spades &&
                card.value >= 6)
            {
                Debug.Log("Jack Heavy Guard Activated");

                return amount - 3;
            }
        }

        return amount;
    }
    private static int JackWitheredBlessing_Heal(int amount)
    {
        Debug.Log("Jack Withered Blessing Activated");

        return 1;
    }

    private static int JackBrokenForce_Damage(
     List<CardSO> cards,
     int originalDamage,
     int finalDamage)
    {
        if (cards == null)
            return finalDamage;

        foreach (CardSO card in cards)
        {
            if (card.suit == CardSO.Suit.Clubs &&
                card.value >= 6)
            {
                Debug.Log("Jack Broken Force Activated");
                return finalDamage - originalDamage;
            }
        }

        return finalDamage;
    }
}