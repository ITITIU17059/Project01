using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardResolver
{
    public static int ResolveDamage(List<CardSO> cards)
    {
        if (cards == null || cards.Count == 0)
            return 0;

        BossSO boss = BossManager.Instance.CurrentBoss;

        // ============================================
        // ROYAL DECREE
        // ============================================

        if (boss != null &&
            boss.currentTrait != null &&
            boss.currentTrait.traitID == TraitID.K_ROYAL_DECREE)
        {
            bool matched = false;

            foreach (CardSO card in cards)
            {
                if (BattleManager.Instance.GetSuit(card)
                    == boss.requiredSuit)
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                Debug.Log(
                    "[ROYAL DECREE] Wrong suit -> Damage = 0");

                return 0;
            }
        }

        // ============================================
        // JOKER
        // ============================================

        if (boss != null &&
            boss.currentTrait != null &&
            boss.currentTrait.traitID == TraitID.JOKER)
        {
            bool matched = false;

            foreach (CardSO card in cards)
            {
                if (BattleManager.Instance.GetSuit(card)
                    == boss.jokerDamageSuit)
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                Debug.Log(
                    "[JOKER] Wrong suit -> Damage = 0");

                return 0;
            }
        }

        // ============================================
        // BASE DAMAGE
        // ============================================

        int baseDamage = 0;
        bool hasClub = false;

        foreach (CardSO card in cards)
        {
            if (card == null)
                continue;

            baseDamage += card.value;

            if (BattleManager.Instance.GetSuit(card)
                == CardSO.Suit.Clubs)
            {
                hasClub = true;
            }
        }

        int damage = baseDamage;

        // ============================================
        // CLUB = x2 DAMAGE ONLY
        // ============================================

        if (!hasClub)
            return damage;

        bool clubResisted =
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.resistanceSuit
                == CardSO.Suit.Clubs;

        if (!clubResisted)
        {
            damage = baseDamage * 2;

            Debug.Log(
                $"[CLUB] Damage: {baseDamage} -> {damage}");
        }

        // ============================================
        // CLUB MODIFIERS
        // ============================================

        foreach (CardSO card in cards)
        {
            if (card == null)
                continue;

            if (BattleManager.Instance.GetSuit(card)
                != CardSO.Suit.Clubs)
            {
                continue;
            }

            if (!clubResisted)
            {
                damage =
                    TraitManager.Instance.ModifyRewardAttackDamage(
                        card,
                        baseDamage,
                        damage);

                damage =
                    TraitManager.Instance.ModifyAttackDamage(
                        cards,
                        baseDamage,
                        damage);
            }

            break;
        }

        return damage;
    }

    //====================================================
    // CARD EFFECT
    //====================================================
    public static int ResolveEffectTotal(
    List<CardSO> cards,
    CardSO foresightCard = null)
    {
        if (cards == null || cards.Count == 0)
            return 0;

        int total = 0;

        foreach (CardSO card in cards)
        {
            if (card == null)
                continue;

            total += card.value;
        }

        // Foresight chỉ cộng VALUE.
        // Không thêm card vào cards.
        if (foresightCard != null)
        {
            total += foresightCard.value;

            Debug.Log(
                $"[FORESIGHT] " +
                $"+{foresightCard.value} -> Effect Total = {total}");
        }

        return total;
    }
    public static IEnumerator ResolveEffects(
     List<CardSO> cards,
     int effectTotal)
    {
        bool hasHeart = false;
        bool hasDiamond = false;
        bool hasSpade = false;

        if (cards == null || cards.Count == 0)
            yield break;

        int healValue = effectTotal;
        int drawValue = effectTotal;
        int shieldValue = effectTotal;

        bool firstSuitOnly =
            BossManager.Instance != null &&
            BossManager.Instance.CurrentBoss != null &&
            BossManager.Instance.CurrentBoss.currentTrait != null &&
            BossManager.Instance.CurrentBoss.currentTrait.traitID ==
                TraitID.K_ABSOLUTE_AUTHORITY;

        if (firstSuitOnly)
        {
            CardSO firstCard = cards[0];

            if (firstCard != null)
            {
                CardSO.Suit suit =
                    BattleManager.Instance.GetSuit(firstCard);

                switch (suit)
                {
                    case CardSO.Suit.Hearts:
                        hasHeart = true;
                        break;

                    case CardSO.Suit.Diamonds:
                        hasDiamond = true;
                        break;

                    case CardSO.Suit.Spades:
                        hasSpade = true;
                        break;
                }
            }
        }
        else
        {
            foreach (CardSO card in cards)
            {
                if (card == null)
                    continue;

                switch (BattleManager.Instance.GetSuit(card))
                {
                    case CardSO.Suit.Hearts:
                        hasHeart = true;
                        break;

                    case CardSO.Suit.Diamonds:
                        hasDiamond = true;
                        break;

                    case CardSO.Suit.Spades:
                        hasSpade = true;
                        break;
                }
            }
        }

        CardSO.Suit resistedSuit =
            BossManager.Instance != null &&
            BossManager.Instance.CurrentBoss != null
                ? BossManager.Instance.CurrentBoss.resistanceSuit
                : CardSO.Suit.None;

        bool IsResisted(CardSO.Suit suit)
        {
            return suit != CardSO.Suit.None &&
                   suit == resistedSuit;
        }

        if (IsResisted(CardSO.Suit.Hearts) && hasHeart)
            hasHeart = false;

        if (IsResisted(CardSO.Suit.Diamonds) && hasDiamond)
            hasDiamond = false;

        if (IsResisted(CardSO.Suit.Spades) && hasSpade)
            hasSpade = false;

        if (hasHeart)
        {
            healValue =
                TraitManager.Instance.ModifyHealAmount(
                    healValue);

            healValue =
                TraitManager.Instance.ModifyRewardHealAmount(
                    healValue);

            BattleManager.Instance.HealDeck(healValue);

            TraitManager.Instance.InvokeBossEvent(
                TraitEventType.HealDeck,
                healValue);

            TraitManager.Instance.InvokeRewardEvent(
                TraitEventType.HealDeck,
                healValue);
        }

        if (hasDiamond)
        {
            drawValue =
                TraitManager.Instance.ModifyDrawAmount(
                    drawValue);

            drawValue =
                TraitManager.Instance.ModifyRewardDrawAmount(
                    drawValue);

            BattleManager.Instance.DrawBonusCards(
                drawValue);
        }

        if (hasSpade)
        {
            int reduceAmount = shieldValue;

            reduceAmount =
                TraitManager.Instance.ModifyShieldAmount(
                    cards,
                    reduceAmount);

            foreach (CardSO card in cards)
            {
                if (card == null)
                    continue;

                if (BattleManager.Instance.GetSuit(card)
                        == CardSO.Suit.Spades &&
                    card.value < 6)
                {
                    reduceAmount =
                        TraitManager.Instance
                            .ModifyRewardShieldAmount(
                                reduceAmount);

                    break;
                }
            }

            BossManager.Instance.ReduceAttack(
                reduceAmount);

            TraitManager.Instance.InvokeBossEvent(
                TraitEventType.ReduceAttack,
                reduceAmount);

            TraitManager.Instance.InvokeRewardEvent(
                TraitEventType.ReduceAttack,
                reduceAmount);
        }

        yield break;
    }

    //====================================================
    // COMBO CHECK
    //====================================================

    public static bool IsValidCombo(List<CardSO> cards)
    {
        if (cards == null || cards.Count == 0)
            return false;

        if (cards.Count == 1)
            return true;

        List<CardSO> aces = new();
        List<CardSO> normals = new();

        foreach (CardSO card in cards)
        {
            if (card.value == 1)
                aces.Add(card);
            else
                normals.Add(card);
        }

        if (normals.Count == 0)
            return aces.Count <= 10;

        if (aces.Count > 1)
            return false;

        int rank = normals[0].value;
        int total = 0;

        foreach (CardSO card in normals)
        {
            if (card.value != rank)
                return false;

            total += card.value;
        }

        if (aces.Count == 1)
            return true;

        return total <= 10;
    }
    //====================================================
    // CARD FX
    //====================================================

    public static IEnumerator PlaySuitFX(List<GameObject> cards)
    {
        foreach (GameObject obj in cards)
        {
            if (obj == null)
                continue;

            CardDisplay display = obj.GetComponent<CardDisplay>();

            if (display == null)
                continue;

            CardSO.Suit suit =
                BattleManager.Instance.GetSuit(display.cardScriptableObject);

            yield return BossFXManager.Instance.PlayCardSuitFX(
                suit,
                display.transform);
        }
    }
    //====================================================
    // DISCARD
    //====================================================

    public static IEnumerator DiscardCards(
        List<GameObject> selectedCards,
        HandManager handManager,
        Transform graveyardSpawnPoint)
    {
        foreach (GameObject obj in selectedCards)
        {
            if (obj == null)
                continue;

            handManager.handCards.Remove(obj);

            CardDisplay display = obj.GetComponent<CardDisplay>();

            if (display != null)
            {
                GraveyardManager.Instance.AddToGraveyard(
                    display.cardScriptableObject);
            }

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                obj,
                graveyardSpawnPoint);
        }

        yield return new WaitForSeconds(0.55f);
    }
    public static IEnumerator PlayRewardK4FX(CardSO card)
    {
        GameObject temp = new GameObject("RewardK4");

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();

        sr.sprite = card.cardSprite;
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 9999;

        temp.transform.position =
            BattleManager.Instance.TavernSpawnPoint.position;

        yield return BossFXManager.Instance.PlayCardSuitFX(
            BattleManager.Instance.GetSuit(card),
            temp.transform);

        Object.Destroy(temp);
    }
}