using TMPro;
using UnityEngine;

public class BossInfoPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text traitTitle;
    [SerializeField] private TMP_Text traitText;

    [SerializeField] private TMP_Text rewardTitle;
    [SerializeField] private TMP_Text rewardText;

    public void Setup(BossSO boss)
    {
        traitTitle.text = boss.traitTitle;
        traitText.text = boss.traitDescription;

        rewardTitle.text = boss.rewardTitle;
        rewardText.text = boss.rewardDescription;
    }
}