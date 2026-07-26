using System.Collections.Generic;
using UnityEngine;

public class GameplayTraitPanelUI : MonoBehaviour
{
    [SerializeField] private TraitCardUI traitPrefab;
    [SerializeField] private Transform content;

    private readonly List<TraitCardUI> spawnedTraits = new();

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        PlayerReward.OnEquipmentChanged += Refresh;
    }

    private void OnDisable()
    {
        PlayerReward.OnEquipmentChanged -= Refresh;
    }

    public void Refresh()
    {
        foreach (var item in spawnedTraits)
            Destroy(item.gameObject);

        spawnedTraits.Clear();

        foreach (RewardSO reward in PlayerReward.Instance.EquippedRewards)
        {
            if (reward == null)
                continue;

            TraitCardUI ui = Instantiate(traitPrefab, content);
            ui.Setup(reward);

            spawnedTraits.Add(ui);
        }
    }
}