using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class TraitSelectionPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject traitCardPrefab;
    [SerializeField] private Transform traitContainer;
    [SerializeField] private HandManager hand;
    private List<BossTraitSO> currentTraits = new();
    public static TraitSelectionPanelUI Instance;

    private BossSO currentBoss;
    private readonly List<GameObject> spawnedCards = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<string> GetCurrentTraitNames()
    {
        List<string> data = new();

        foreach (BossTraitSO trait in currentTraits)
            data.Add(trait.name);

        return data;
    }

    public void Show(BossSO boss)
    {
        currentBoss = boss;

        hand.SetInteractable(false);
        hand.ResetAllCardHover();

        if (JesterHandManager.Instance != null)
        {
            JesterHandManager.Instance
                .SetJesterInteractionLocked(true);
        }

        gameObject.SetActive(true);

        transform.SetAsLastSibling();

        ClearCards();

        SaveData save = SaveManager.Instance.LoadProgress();

        List<BossTraitSO> traits = new();

        if (save != null &&
            save.currentTraitSelection.Count > 0)
        {
            foreach (string traitName in
                     save.currentTraitSelection)
            {
                BossTraitSO trait =
                    TraitPoolManager.Instance
                        .GetTraitByName(traitName);

                if (trait != null)
                    traits.Add(trait);
            }
        }
        else
        {
            traits =
                TraitPoolManager.Instance
                    .GetRandomTraits(
                        boss.rank,
                        3);
        }

        currentTraits = traits;

        foreach (BossTraitSO trait in traits)
        {
            GameObject card =
                Instantiate(
                    traitCardPrefab,
                    traitContainer
                );

            TraitCardUI ui =
                card.GetComponent<TraitCardUI>();

            ui.Setup(trait);

            TraitCardButton button =
                card.GetComponent<TraitCardButton>();

            button.Initialize(
                this,
                trait
            );

            spawnedCards.Add(card);
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void ClearCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            Destroy(card);
        }

        spawnedCards.Clear();
    }
    public void SelectTrait(BossTraitSO selectedTrait)
    {
        currentBoss.currentTrait = selectedTrait;

        BossManager.Instance.RefreshBossInfo();


        if (JesterHandManager.Instance != null)
        {
            JesterHandManager.Instance
                .SetJesterInteractionLocked(false);
        }

        gameObject.SetActive(false);

        hand.SetInteractable(true);

        BattleManager.Instance.StartBattleAfterTraitSelected();

        SoundManager.instance.PlaySound2D(
            currentBoss.spawnSoundID
        );
    }


    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);

        if (JesterHandManager.Instance != null)
        {
            JesterHandManager.Instance
                .SetJesterInteractionLocked(visible);
        }

        if (!visible)
            return;

        hand.SetInteractable(false);
    }

    public void ClearCurrentTraits()
    {
        currentTraits.Clear();
    }
}