using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TraitCardButton : MonoBehaviour
{
    private TraitSelectionPanelUI panel;
    private BossTraitSO trait;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(TraitSelectionPanelUI panelUI, BossTraitSO traitData)
    {
        panel = panelUI;
        trait = traitData;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        panel.SelectTrait(trait);
    }
}