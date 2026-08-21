using System;
using UnityEngine;

public class JesterManager : MonoBehaviour
{
    public static JesterManager Instance { get; private set; }

    //==================================================
    // EVENT
    //==================================================

    public event Action OnChanged;

    //==================================================
    // STATE
    //==================================================

    [Header("Jester State")]
    [SerializeField] private bool unlocked = false;

    [Header("Jester Charges")]
    [SerializeField] private int resetCharges = 0;
    [SerializeField] private int instantKillCharges = 0;

    //==================================================
    // PROPERTIES
    //==================================================

    public bool IsUnlocked => unlocked;

    public int ResetCharges => resetCharges;

    public int InstantKillCharges => instantKillCharges;

    public bool CanUseReset =>
        unlocked && resetCharges > 0;

    public bool CanUseInstantKill =>
        unlocked && instantKillCharges > 0;

    //==================================================
    // UNITY
    //==================================================

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

    //==================================================
    // UNLOCK
    //==================================================

    public void UnlockJesters()
    {
        if (unlocked)
            return;

        unlocked = true;

        // Khi mở khóa lần đầu:
        // mỗi Jester có 1 lượt sử dụng.
        resetCharges = 1;
        instantKillCharges = 1;

        NotifyChanged();

        Debug.Log(
            $"[JESTER] Unlocked! " +
            $"Reset = {resetCharges}, " +
            $"Instant Kill = {instantKillCharges}"
        );
    }

    //==================================================
    // RECOVER AFTER RANK
    //==================================================

    public void RecoverAfterRank(BossRank rank)
    {
        if (!unlocked)
            return;

        // Chỉ hồi sau khi hoàn thành Queen hoặc King.
        if (rank != BossRank.Queen &&
            rank != BossRank.King)
            return;

        resetCharges++;
        instantKillCharges++;

        NotifyChanged();

        Debug.Log(
            $"[JESTER] {rank} cleared. " +
            $"Reset = {resetCharges}, " +
            $"Instant Kill = {instantKillCharges}"
        );
    }

    //==================================================
    // CONSUME RESET
    //==================================================

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

    //==================================================
    // CONSUME INSTANT KILL
    //==================================================

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

    //==================================================
    // SAVE DATA
    //==================================================

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

    //==================================================
    // LOAD DATA
    //==================================================

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

    //==================================================
    // RESET DATA
    //==================================================

    public void ResetData()
    {
        unlocked = false;

        resetCharges = 0;
        instantKillCharges = 0;

        NotifyChanged();

        Debug.Log("[JESTER] Data reset.");
    }

    //==================================================
    // NOTIFY UI
    //==================================================

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}