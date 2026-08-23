using UnityEngine;

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

    [Header("Card")]
    public Sprite cardSprite;

    public int value;

    public Suit suit;

    public CardType type;

    [Header("Description")]
    public string cardName;

    [TextArea(2, 5)]
    public string description;
}