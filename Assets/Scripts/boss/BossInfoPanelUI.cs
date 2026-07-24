using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossInfoPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text traitTitleText;
    [SerializeField] private TMP_Text traitDescriptionText;
    [SerializeField] private TMP_Text rewardTitleText;
    [SerializeField] private TMP_Text rewardDescriptionText;
    [SerializeField] private Image resistanceIcon;

    public void Setup(BossSO boss)
    {
        if (boss.currentTrait == null)
            return;

        traitTitleText.text = boss.currentTrait.traitName;
        traitDescriptionText.text = boss.currentTrait.description;

        rewardTitleText.text = boss.currentTrait.reward.rewardName;
        rewardDescriptionText.text = boss.currentTrait.reward.description;
    }
}