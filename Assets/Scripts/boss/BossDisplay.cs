using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer artwork;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text bossNameText;
    
    [SerializeField] private Image resistanceIcon;

    [SerializeField] private Sprite heartSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite clubSprite;
    [SerializeField] private Sprite spadeSprite;
    public SpriteRenderer Artwork => artwork;

    private BossSO boss;

    public void Setup(BossSO data)
    {
        ResetUI();

        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        boss = data;

        artwork.sprite = boss.cardSprite;
        hpText.text = boss.hp.ToString();
        atkText.text = boss.atk.ToString();
        bossNameText.text = GetBossName(boss);
        UpdateResistance(boss.resistanceSuit);
    }
    private string GetBossName(BossSO boss)
    {
        return boss.bossName;
    }

    public void UpdateHP(int hp)
    {
        hpText.text = hp.ToString();
    }
    public void UpdateATK(int atk)
    {
        atkText.text = atk.ToString();
    }
    public void UpdateResistance(CardSO.Suit suit)
    {
        switch (suit)
        {
            case CardSO.Suit.Hearts:
                resistanceIcon.sprite = heartSprite;
                break;

            case CardSO.Suit.Diamonds:
                resistanceIcon.sprite = diamondSprite;
                break;

            case CardSO.Suit.Clubs:
                resistanceIcon.sprite = clubSprite;
                break;

            case CardSO.Suit.Spades:
                resistanceIcon.sprite = spadeSprite;
                break;

            default:
                resistanceIcon.enabled = false;
                return;
        }

        resistanceIcon.enabled = true;
    }

  
    public void ResetUI()
    {
        hpText.gameObject.SetActive(true);
        atkText.gameObject.SetActive(true);
        bossNameText.gameObject.SetActive(true);
    }
}