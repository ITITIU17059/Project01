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

    public Suit suit;

    public int hp;

    public int atk;
}