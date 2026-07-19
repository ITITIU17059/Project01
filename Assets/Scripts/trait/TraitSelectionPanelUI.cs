using System.Collections.Generic;
using UnityEngine;

public class TraitSelectionPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject traitCardPrefab;
    [SerializeField] private Transform traitContainer;

    private BossSO currentBoss;
    private readonly List<GameObject> spawnedCards = new();
    
    public void Show(BossSO boss)
    {


        currentBoss = boss;

        gameObject.SetActive(true);

        transform.SetAsLastSibling();

        ClearCards();


        List<BossTraitSO> traits =
    TraitPoolManager.Instance.GetRandomTraits(
        boss.rank,
        3
    );

        foreach (BossTraitSO trait in traits)
        {
            GameObject card =
                Instantiate(traitCardPrefab, traitContainer);

            TraitCardUI ui =
                card.GetComponent<TraitCardUI>();

            ui.Setup(trait);

            TraitCardButton button =
                card.GetComponent<TraitCardButton>();

            button.Initialize(this, trait);

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
        currentBoss.currentReward = selectedTrait.reward;
        TraitPoolManager.Instance.RemoveTrait(
        currentBoss.rank,
        selectedTrait
        );
        BossManager.Instance.RefreshBossInfo();

        gameObject.SetActive(false);

        BattleManager.Instance.StartBattleAfterTraitSelected();
    }
}