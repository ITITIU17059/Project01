using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Card", menuName = "Card / Create New Card")]
public class CardSO : ScriptableObject
{
    public enum Suit
    {
        None = 0,
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum CardType
    {
        None = 0,
        Number,
        Jack,
        Queen,
        King,
        Jester

    }

    public Sprite cardSprite;
    public int value;
    public Suit suit;
    public CardType type;

}
