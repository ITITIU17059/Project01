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
    public string bossName;
    public Sprite cardSprite;

    public Suit suit;

    public int hp;

    public int atk;

    [Header("Attack")]
    public GameObject attackVFX;      // Đạn bay
    public GameObject hitVFX;         // Nổ khi trúng player
    public string attackSoundID;      // Âm thanh
    public float attackFlyTime = 0.4f;
}