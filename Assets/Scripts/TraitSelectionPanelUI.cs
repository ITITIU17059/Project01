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

        ClearCards();

        int count = Mathf.Min(3, boss.possibleTraits.Count);

        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(traitCardPrefab, traitContainer);

            TraitCardUI ui = card.GetComponent<TraitCardUI>();

            ui.Setup(boss.possibleTraits[i]);

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

        BossManager.Instance.RefreshBossInfo();

        gameObject.SetActive(false);

        BattleManager.Instance.StartBattleAfterTraitSelected();
    }
}