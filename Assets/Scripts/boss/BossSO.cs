using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boss Card", menuName = "Boss/BossSO")]
public class BossSO : ScriptableObject
{
    public enum Suit
    {
        None = 0,
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    [Header("Boss Info")]
    public string bossName;
    public Sprite cardSprite;
    public Suit suit;

    public int hp;
    public int atk;

    [Header("Trait Pool")]
    public List<BossTraitSO> possibleTraits;

    [HideInInspector]
    public BossTraitSO currentTrait;

    [Header("Boss Card")]
    public CardSO bossCard;

    [Header("Special")]
    public bool isJoker;

    [Header("Resistance")]
    public CardSO.Suit resistanceSuit;

    [HideInInspector]
    public int currentATK;

    [Header("Attack")]
    public GameObject attackVFX;
    public GameObject hitVFX;
    public string attackSoundID;
    public float attackFlyTime = 0.6f;

    [HideInInspector] public int turnCounter;

    [Header("Stage")]
    public BossRank rank;

    [Header("Sound")]
    public string spawnSoundID;

    [HideInInspector]
    public CardSO.Suit requiredSuit = CardSO.Suit.None;

    [HideInInspector]
    public Sprite currentDisguiseSprite;

    public CardSO.Suit jokerDamageSuit;
}