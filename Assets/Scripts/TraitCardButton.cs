using UnityEngine;
using UnityEngine.UI;

public class TraitCardButton : MonoBehaviour
{
    private TraitSelectionPanelUI panel;
    private BossTraitSO trait;

    public void Initialize(TraitSelectionPanelUI panelUI, BossTraitSO traitData)
    {
        panel = panelUI;
        trait = traitData;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        panel.SelectTrait(trait);
    }
}