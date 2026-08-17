using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReward : MonoBehaviour
{

    public static PlayerReward Instance { get; private set; }

    public const int MaxEquipSlots = 3;

    [SerializeField] private List<RewardSO> ownedRewards = new();

    [SerializeField] private RewardSO[] equippedRewards = new RewardSO[MaxEquipSlots];
    public IReadOnlyList<RewardSO> OwnedRewards => ownedRewards;
    public RewardSO[] EquippedRewards => equippedRewards;
    public static event Action OnEquipmentChanged;
    
    private bool traitHasAdd = false;
    public bool TraitHasAdd => traitHasAdd;

    public int aceHandBonus = 0;
    private void Awake()
    {
        if (equippedRewards == null || equippedRewards.Length != MaxEquipSlots)
        {
            equippedRewards = new RewardSO[MaxEquipSlots];
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkTraitHasAdd()
    {
        traitHasAdd = true;
    }
    public void LoadTraitHasAdd(bool value)
    {
        if (value)
            traitHasAdd = true;
    }
    public bool AddReward(RewardSO reward)
    {
        if (reward == null)
            return false;

        if (ownedRewards.Contains(reward))
        {

            return false;
        }

        ownedRewards.Add(reward);

        return true;
    }
    public void ResetTraitHasAdd()
    {
        traitHasAdd = false;
    }
    public bool IsEquipped(RewardSO reward)
    {
        foreach (RewardSO r in equippedRewards)
        {
            if (r == reward)
                return true;
        }

        return false;
    }

    public bool EquipReward(RewardSO reward, int slotIndex)
    {

        if (reward == null)
        {
            return false;
        }

        if (!ownedRewards.Contains(reward))
        {
            return false;
        }

        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
        {
            return false;
        }

        for (int i = 0; i < equippedRewards.Length; i++)
        {
            if (equippedRewards[i] == reward)
            {
                equippedRewards[i] = null;
                break;
            }
        }

        equippedRewards[slotIndex] = reward;
        RefreshHandSize();
        traitHasAdd = true;
        OnEquipmentChanged?.Invoke();

        return true;

    }

    public bool UnequipReward(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
            return false;

        equippedRewards[slotIndex] = null;
        RefreshHandSize();
        OnEquipmentChanged?.Invoke();
        return true;
    }
    public void SwapReward(int fromSlot, int toSlot)
    {
        if (fromSlot == toSlot)
            return;

        RewardSO temp = equippedRewards[fromSlot];
        equippedRewards[fromSlot] = equippedRewards[toSlot];
        equippedRewards[toSlot] = temp;
        RefreshHandSize();
        OnEquipmentChanged?.Invoke();
    }

    public List<string> GetOwnedRewardNames()
    {
        List<string> list = new();

        foreach (RewardSO reward in ownedRewards)
            list.Add(reward.rewardName);

        return list;
    }

    public List<string> GetEquippedRewardNames()
    {
        List<string> list = new();

        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null)
                list.Add("");
            else
                list.Add(reward.rewardName);
        }

        return list;
    }

    public void LoadRewards(List<string> owned, List<string> equipped)
    {
        ownedRewards.Clear();

        RewardSO[] database = Resources.LoadAll<RewardSO>("");

        foreach (string rewardName in owned)
        {
            RewardSO reward = System.Array.Find(database,
                r => r.rewardName == rewardName);

            if (reward != null)
                ownedRewards.Add(reward);
        }

        equippedRewards = new RewardSO[MaxEquipSlots];

        for (int i = 0; i < equipped.Count && i < MaxEquipSlots; i++)
        {
            if (string.IsNullOrEmpty(equipped[i]))
                continue;

            RewardSO reward = System.Array.Find(database,
                r => r.rewardName == equipped[i]);

            equippedRewards[i] = reward;
        }

        OnEquipmentChanged?.Invoke();
    }
    public void RefreshHandSize()
    {
        if (HandManager.Instance == null)
            return;

        HandManager.Instance.maxHandSize = 8;

        bool hasExpandedArsenal = false;

        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null)
                continue;

            if (reward.traitID == TraitID.Q_SEAL_OF_SILENCE)
            {
                hasExpandedArsenal = true;
                HandManager.Instance.maxHandSize++;
            }
        }

        if (hasExpandedArsenal)
        {
            HandManager.Instance.maxHandSize += aceHandBonus;
        }
        else
        {
            aceHandBonus = 0;
        }
    }
    public void AddAceHandBonus(int amount)
    {
        if (amount <= 0)
            return;

        aceHandBonus += amount;
    }

    public void ResetAceHandBonus()
    {
        aceHandBonus = 0;

        RefreshHandSize();
    }
    public bool HasReward(TraitID traitID)
    {
        foreach (RewardSO reward in equippedRewards)
        {
            if (reward == null)
                continue;

            if (reward.traitID == traitID)
                return true;
        }

        return false;
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < EquippedRewards.Length; i++)
        {
            if (EquippedRewards[i] == null)
                return i;
        }

        return -1;
    }

    public bool IsFull()
    {
        return GetFirstEmptySlot() == -1;
    }
}