using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer artwork;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text bossNameText;

    private BossSO boss;

    public void Setup(BossSO data)
    {
        boss = data;

        artwork.sprite = boss.cardSprite;
        hpText.text = boss.hp.ToString();
        atkText.text = boss.atk.ToString();
        bossNameText.text = GetBossName(boss);
       
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
<<<<<<< HEAD

    public void ResetUI()
    {
        hpText.gameObject.SetActive(true);
        atkText.gameObject.SetActive(true);
        bossNameText.gameObject.SetActive(true);
    }
=======
>>>>>>> parent of aa29897 (Thêm hiệu ứng boss bay vào bộ bài)
}