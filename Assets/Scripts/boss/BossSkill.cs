using System.Collections.Generic;
using System.Security.Cryptography;
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

    public delegate void TraitEvent(int value);
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

        RegisterEvent(
    TraitID.Q_LIFE_LEECH,
    TraitEventType.GainCard,
    QueenLifeLeech_Draw);

        RegisterEvent(
    TraitID.Q_SEAL_OF_SILENCE,
    TraitEventType.PlayerTurn,
    QueenSeal_PlayerTurn);

        RegisterEvent(
    TraitID.K_ENDLESS_WRATH,
    TraitEventType.BossTurn,
    KingEndlessWrath_BossTurn);

        RegisterEvent(
    TraitID.K_ROYAL_DECREE,
    TraitEventType.PlayerTurn,
    KingDisguise_PlayerTurn);
        RegisterEvent(
    TraitID.K_BLIND_FATE,
    TraitEventType.PlayerTurn,
    KingBlindFate_PlayerTurn);
        RegisterEvent(
    TraitID.JOKER,
    TraitEventType.BossTurn,
    Joker_BossTurn);

        RegisterEvent(
            TraitID.JOKER,
            TraitEventType.PlayerTurn,
            Joker_PlayerTurn);

        RegisterEvent(
            TraitID.JOKER,
            TraitEventType.DrawOneCard,
            Joker_DrawOneCard);

        RegisterEvent(
            TraitID.JOKER,
            TraitEventType.HealDeck,
            Joker_HealDeck);
    }

    public static void Invoke(
     TraitEventType type,
     BossTraitSO trait,
     int value)
    {
        if (trait == null)
            return;

        if (!eventTable.ContainsKey(trait.traitID))
            return;

        if (eventTable[trait.traitID]
            .TryGetValue(type, out TraitEvent evt))
        {
            evt.Invoke(value);
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


    public static int ModifyDrawAmount(
    BossTraitSO trait,
    int amount)
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

    public static int ModifyHealAmount(
     BossTraitSO trait,
     int amount)
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
            if (BattleManager.Instance.GetSuit(card) == CardSO.Suit.Spades &&
      card.value >= 6)
            {
                return amount - 3;
            }
        }

        return amount;
    }
    private static int JackWitheredBlessing_Heal(int amount)
    {
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
            if (BattleManager.Instance.GetSuit(card) == CardSO.Suit.Clubs &&
     card.value >= 6)
            {
                return finalDamage - originalDamage;
            }
        }

        return finalDamage;
    }
    private static void QueenLifeLeech_Draw(int value)
    {
        BossManager.Instance.Heal(value);
    }
    private static void QueenSeal_PlayerTurn(int value)
    {
        HandManager hand =
            Object.FindAnyObjectByType<HandManager>();

        if (hand == null)
            return;

        hand.LockHighestCard();
    }
    private static void KingEndlessWrath_BossTurn(int value)
    {
        BossSO boss = BossManager.Instance.CurrentBoss;

        if (boss == null)
            return;

        boss.turnCounter++;

        // Mỗi 2 lượt tăng 5 ATK, tối đa 20 ATK
        if (boss.turnCounter % 2 == 0 &&
            boss.currentATK < 20)
        {
            int increase = Mathf.Min(5, 20 - boss.currentATK);

            BossManager.Instance.HealAttack(increase);

            Debug.Log(
                $"[KING] +{increase} ATK -> Current ATK: {boss.currentATK}");
        }
    }
    private static void KingDisguise_PlayerTurn(int value)
    {
        BossManager manager = BossManager.Instance;

        if (manager == null)
            return;

        BossSO boss = manager.CurrentBoss;

        if (boss == null || boss.rank != BossRank.King)
            return;

        List<BossSO> pool = manager.DisguisePool;

        if (pool == null || pool.Count == 0)
            return;

        BossSO disguise =
            pool[Random.Range(0, pool.Count)];

        if (disguise == null)
            return;

        boss.requiredSuit = disguise.resistanceSuit;

        manager.BossDisplay.SetBossSprite(
            disguise.cardSprite);

    }
    private static void KingBlindFate_PlayerTurn(int value)
    {
        if (HandManager.Instance == null)
            return;

        HandManager.Instance.RefreshHiddenCards();
    }
    private static void Joker_BossTurn(int value)
    {
        BossManager manager = BossManager.Instance;

        if (manager == null)
            return;

        BossSO boss = manager.CurrentBoss;

        if (boss == null || !boss.isJoker)
            return;

        boss.turnCounter++;

        Debug.Log(
            $"[JOKER] Boss Turn: {boss.turnCounter}");

        manager.HealAttack(boss.turnCounter);

        // Đổi disguise + suit mỗi lượt
        manager.RandomizeJokerDisguise();
        manager.RandomizeJokerSuit();

        Debug.Log(
            $"[JOKER] Turn {boss.turnCounter} -> +{boss.turnCounter} ATK + Disguise + Suit");
    }
    private static void Joker_DrawOneCard(int value)
    {
        BossManager manager = BossManager.Instance;

        if (manager == null)
            return;

        BossSO boss = manager.CurrentBoss;

        if (boss == null || !boss.isJoker)
            return;

        manager.Heal(1);

        Debug.Log(
            "[JOKER] Draw 1 card -> +1 HP");
    }
    private static void Joker_HealDeck(int value)
    {
        BossManager manager = BossManager.Instance;

        if (manager == null)
            return;

        BossSO boss = manager.CurrentBoss;

        if (boss == null || !boss.isJoker)
            return;

        manager.Heal(1);

        Debug.Log(
            "[JOKER] Heart heal -> +1 HP");
    }
    private static void Joker_PlayerTurn(int value)
    {
        HandManager hand = HandManager.Instance;

        if (hand == null)
            return;

        // Khóa 1 lá
        hand.LockHighestCard();

        // Úp 1 lá
        hand.HideRandomCard();

        Debug.Log(
            "[JOKER] Player Turn -> Lock 1 card + Hide 1 card");
    }
}   
