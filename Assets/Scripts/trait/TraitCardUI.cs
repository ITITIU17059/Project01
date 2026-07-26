using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image traitIcon;
    [SerializeField] private TMP_Text traitName;
    [SerializeField] private TMP_Text traitDescription;
    [SerializeField] private TMP_Text rewardTitle;
    [SerializeField] private TMP_Text rewardDescription;

    private BossTraitSO traitData;

    public BossTraitSO TraitData => traitData;

    public void Setup(BossTraitSO trait)
    {
        traitData = trait;

        traitIcon.sprite = trait.icon;

        traitName.text = trait.traitName;
        traitDescription.text = trait.description;

        rewardTitle.text = trait.reward.rewardName;
        rewardDescription.text = trait.reward.description;
    }

    public void Setup(RewardSO trait)
    {
        traitIcon.sprite = trait.icon;
        rewardDescription.text = trait.description;
    }
}