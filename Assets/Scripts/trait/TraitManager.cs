using UnityEngine;

public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance { get; private set; }

    private BossTraitSO currentTrait;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCurrentTrait(BossTraitSO trait)
    {
        currentTrait = trait;
    }

    public BossTraitSO GetCurrentTrait()
    {
        return currentTrait;
    }

    public bool HasTrait()
    {
        return currentTrait != null;
    }
    public void DebugCurrentTrait()
    {
        if (currentTrait == null)
            return;

        Debug.Log("Current Trait: " + currentTrait.traitID);
    }
}