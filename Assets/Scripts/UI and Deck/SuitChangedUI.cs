using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SuitChangedUI : MonoBehaviour
{
    public static SuitChangedUI Instance;

    [SerializeField] private Image icon;

    public Sprite heart;
    public Sprite diamond;
    public Sprite club;
    public Sprite spade;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(CardSO.Suit suit)
    {
        switch (suit)
        {
            case CardSO.Suit.Hearts:
                icon.sprite = heart;
                break;

            case CardSO.Suit.Diamonds:
                icon.sprite = diamond;
                break;

            case CardSO.Suit.Clubs:
                icon.sprite = club;
                break;

            case CardSO.Suit.Spades:
                icon.sprite = spade;
                break;
        }

        gameObject.SetActive(true);

        Color c = icon.color;
        c.a = 0;
        icon.color = c;

        Sequence seq = DOTween.Sequence();

        seq.Append(icon.DOFade(1, 0.15f));
        seq.AppendInterval(0.8f);
        seq.Append(icon.DOFade(0, 0.3f));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}