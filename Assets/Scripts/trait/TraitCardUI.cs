using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraitCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image traitIcon;
    [SerializeField] private TMP_Text traitName;
    [SerializeField] private TMP_Text traitDescription;
    [SerializeField] private TMP_Text rewardTitle;
    [SerializeField] private TMP_Text rewardDescription;
    [NonSerialized] public string rewardText;

    private BossTraitSO traitData;

    public BossTraitSO TraitData => traitData;

    private void Awake()
    {

    }

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
        rewardText = trait.description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (traitIcon.sprite == null || rewardText == null) return;

        RewardInfoUI.Instance.Show(rewardText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RewardInfoUI.Instance.Hide();
    }

}