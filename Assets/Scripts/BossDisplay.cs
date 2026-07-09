using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer artwork;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;

    private BossSO boss;

    public void Setup(BossSO data)
    {
        boss = data;

        artwork.sprite = boss.cardSprite;
        hpText.text = boss.hp.ToString();
        atkText.text = boss.atk.ToString();
    }

    public void UpdateHP(int hp)
    {
        hpText.text = hp.ToString();
    }
}