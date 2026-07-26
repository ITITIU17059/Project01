using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardResolver
{
    //====================================================
    // DAMAGE
    //====================================================

    public static int ResolveDamage(List<CardSO> cards)
    {
        if (cards == null || cards.Count == 0)
            return 0;

        int damage = 0;
        bool hasClub = false;

        foreach (CardSO card in cards)
        {
            damage += card.value;

            if (card.suit == CardSO.Suit.Clubs)
                hasClub = true;
        }

        if (!hasClub)
            return damage;

        int originalDamage = damage;

        damage *= 2;

        foreach (CardSO card in cards)
        {
            if (card.suit != CardSO.Suit.Clubs)
                continue;

            // Reward trước
            damage = TraitManager.Instance.ModifyRewardAttackDamage(
                card,
                originalDamage,
                damage);

            // Boss luôn xử lý cuối
            damage = TraitManager.Instance.ModifyAttackDamage(
                cards,
                originalDamage,
                damage);

            break;
        }

        return damage;

    }

    //====================================================
    // CARD EFFECT
    //====================================================

    public static IEnumerator ResolveEffects(List<CardSO> cards)
    {
        bool hasHeart = false;
        bool hasDiamond = false;
        bool hasSpade = false;

        int total = 0;

        foreach (CardSO card in cards)
        {
            total += card.value;

            switch (card.suit)
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

        // Kháng chất: chất mà boss đang kháng thì HIỆU ỨNG của chất đó
        // (heal/draw/giảm đòn) không có tác dụng — damage không bị ảnh hưởng.
        BossSO currentBoss = BossManager.Instance != null
            ? BossManager.Instance.CurrentBoss
            : null;

        CardSO.Suit resistedSuit = currentBoss != null
            ? currentBoss.resistanceSuit
            : CardSO.Suit.None;

        if (resistedSuit == CardSO.Suit.Hearts && hasHeart)
        {
            Debug.Log("Boss kháng chất Hearts -> hiệu ứng hồi máu không có tác dụng");
            hasHeart = false;
        }

        if (resistedSuit == CardSO.Suit.Diamonds && hasDiamond)
        {
            Debug.Log("Boss kháng chất Diamonds -> hiệu ứng rút bài không có tác dụng");
            hasDiamond = false;
        }

        if (resistedSuit == CardSO.Suit.Spades && hasSpade)
        {
            Debug.Log("Boss kháng chất Spades -> hiệu ứng giảm đòn không có tác dụng");
            hasSpade = false;
        }

        //---------------- HEART ----------------

        if (hasHeart)
        {
            total = TraitManager.Instance.ModifyHealAmount(total);
            total = TraitManager.Instance.ModifyRewardHealAmount(total);

            BattleManager.Instance.HealDeck(total);

            TraitManager.Instance.InvokeBossEvent(TraitEventType.HealDeck);
            TraitManager.Instance.InvokeRewardEvent(TraitEventType.HealDeck);
        }

        //---------------- DIAMOND ----------------

        if (hasDiamond)
        {
            total = TraitManager.Instance.ModifyDrawAmount(total);
            total = TraitManager.Instance.ModifyRewardDrawAmount(total);

            BattleManager.Instance.DrawBonusCards(total);

            TraitManager.Instance.InvokeBossEvent(TraitEventType.Draw);
            TraitManager.Instance.InvokeRewardEvent(TraitEventType.Draw);
        }

        //---------------- SPADE ----------------

        if (hasSpade)
        {
            int reduceAmount = total;

            reduceAmount =
     TraitManager.Instance.ModifyShieldAmount(
         cards,
         reduceAmount);

            foreach (CardSO card in cards)
            {
                if (card.suit == CardSO.Suit.Spades &&
                    card.value < 6)
                {
                    reduceAmount =
                        TraitManager.Instance.ModifyRewardShieldAmount(reduceAmount);

                    break;
                }
            }

            BossManager.Instance.ReduceAttack(reduceAmount);

            TraitManager.Instance.InvokeBossEvent(TraitEventType.ReduceAttack);
            TraitManager.Instance.InvokeRewardEvent(TraitEventType.ReduceAttack);
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

            yield return BossFXManager.Instance.PlayCardSuitFX(
                display.cardScriptableObject,
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
}