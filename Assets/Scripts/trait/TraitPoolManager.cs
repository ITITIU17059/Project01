using System.Collections.Generic;
using UnityEngine;

public class TraitPoolManager : MonoBehaviour
{
    public static TraitPoolManager Instance;

    private readonly Dictionary<BossRank, List<BossTraitSO>> traitPools = new();

    private void Awake()
    {
        Instance = this;
    }

    public void InitializePool(BossRank rank, List<BossTraitSO> traits)
    {
        if (traitPools.ContainsKey(rank))
            return;

        traitPools.Add(rank, new List<BossTraitSO>(traits));
    }

    public List<BossTraitSO> GetRandomTraits(BossRank rank, int amount)
    {
        List<BossTraitSO> result = new();

        if (!traitPools.ContainsKey(rank))
            return result;

        List<BossTraitSO> pool = new(traitPools[rank]);

        while (result.Count < amount && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);

            result.Add(pool[index]);

            pool.RemoveAt(index);
        }

        return result;
    }

    public void RemoveTrait(BossRank rank, BossTraitSO trait)
    {
        if (!traitPools.ContainsKey(rank))
            return;

        traitPools[rank].Remove(trait);
    }

    public int RemainingTrait(BossRank rank)
    {
        if (!traitPools.ContainsKey(rank))
            return 0;

        return traitPools[rank].Count;
    }
}