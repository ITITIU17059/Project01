using System;
using UnityEngine;

public class JesterManager : MonoBehaviour
{
    public static JesterManager Instance { get; private set; }

    public event Action OnChanged;

    [Header("Jester State")]
    [SerializeField] private bool unlocked = false;

    [Header("Jester Charges")]
    [SerializeField] private int resetCharges = 0;
    [SerializeField] private int instantKillCharges = 0;

    // Max stack for each Jester charge type (per bug fix: recovering after
    // a rank should stack, capped at 2 per Jester).
    private const int MaxChargesPerJester = 2;


    public bool IsUnlocked => unlocked;

    public int ResetCharges => resetCharges;

    public int InstantKillCharges => instantKillCharges;

    public bool CanUseReset =>
        unlocked && resetCharges > 0;

    public bool CanUseInstantKill =>
        unlocked && instantKillCharges > 0;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    public void UnlockJesters()
    {
        if (unlocked)
            return;

        unlocked = true;

        resetCharges = 1;
        instantKillCharges = 1;

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Unlocked! " +
            $"Reset = {resetCharges}, " +
            $"Instant Kill = {instantKillCharges}"
        );
    }

    public void RecoverAfterRank(BossRank rank)
    {
        if (!unlocked)
            return;

        // This method is called every time a full rank of 4 bosses has
        // been cleared (Jack, Queen or King), right as the next rank
        // (Queen, King or Joker respectively) is entered. Jack itself is
        // excluded because Jesters unlock only once Queen is reached, so
        // there is nothing to recover yet at that point.
        if (rank == BossRank.Jack)
            return;

        resetCharges =
            Mathf.Min(MaxChargesPerJester, resetCharges + 1);

        instantKillCharges =
            Mathf.Min(MaxChargesPerJester, instantKillCharges + 1);

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Rank cleared, entering {rank}. " +
            $"Charges: Reset = {resetCharges}, " +
            $"Instant Kill = {instantKillCharges}"
        );
    }


    /// <summary>
    /// Removes the Jester cards when the player equips a reward trait.
    /// Jesters are the alternative reward path, so equipping a reward
    /// immediately revokes the Jester unlock and all remaining charges.
    /// </summary>
    public void RevokeJesters()
    {
        if (!unlocked && resetCharges <= 0 && instantKillCharges <= 0)
            return;

        unlocked = false;
        resetCharges = 0;
        instantKillCharges = 0;

        NotifyChanged();

        Debug.Log("[JESTER] Revoked because a reward trait was equipped.");
    }


    public bool ConsumeReset()
    {
        if (!CanUseReset)
            return false;

        resetCharges--;

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Reset used. " +
            $"Remaining = {resetCharges}"
        );

        return true;
    }


    public bool ConsumeInstantKill()
    {
        if (!CanUseInstantKill)
            return false;

        instantKillCharges--;

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Instant Kill used. " +
            $"Remaining = {instantKillCharges}"
        );

        return true;
    }


    public bool GetUnlocked()
    {
        return unlocked;
    }

    public int GetResetCharges()
    {
        return resetCharges;
    }

    public int GetInstantKillCharges()
    {
        return instantKillCharges;
    }


    public void LoadData(
        bool savedUnlocked,
        int savedResetCharges,
        int savedInstantKillCharges)
    {
        unlocked = savedUnlocked;

        resetCharges =
            Mathf.Max(0, savedResetCharges);

        instantKillCharges =
            Mathf.Max(0, savedInstantKillCharges);

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Loaded. " +
            $"Unlocked = {unlocked}, " +
            $"Reset = {resetCharges}, " +
            $"Instant Kill = {instantKillCharges}"
        );
    }


    public void ResetData()
    {
        unlocked = false;

        resetCharges = 0;
        instantKillCharges = 0;

        NotifyChanged();

        Debug.Log("[JESTER] Data reset.");
    }


    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}