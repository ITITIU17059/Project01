using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossInfoPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text traitDescriptionText;

    public void Setup(BossSO boss)
    {
        if (boss.currentTrait == null)
            return;

        traitDescriptionText.text = boss.currentTrait.description;
    }
}