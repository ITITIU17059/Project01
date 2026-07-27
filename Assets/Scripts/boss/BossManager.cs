using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [Header("Boss Lists")]
    [SerializeField] private List<BossSO> jackBosses;
    [SerializeField] private List<BossSO> queenBosses;
    [SerializeField] private List<BossSO> kingBosses;
    [SerializeField] private List<BossSO> jokerBosses;

    [Header("Display")]
    [SerializeField] private BossDisplay bossDisplay;
    [SerializeField] private BossInfoPanelUI bossInfoPanel;
    [SerializeField] private TraitSelectionPanelUI traitSelectionPanel;
    public BossDisplay BossDisplay => bossDisplay;

    public Transform BossTransform => bossDisplay.transform;

    private readonly List<BossSO> bossSequence = new();
    private bool initialized;
    public int CurrentBossIndex { get; private set; }
    public int CurrentStageIndex { get; private set; }

    private int defeatedJack;
    private int defeatedQueen;
    private int defeatedKing;


    public BossSO CurrentBoss { get; private set; }

    public int CurrentHP { get; private set; }

    public int CurrentATK => CurrentBoss.currentATK;
    public bool LastKillWasPerfect { get; private set; }

    public bool HasMoreBosses => CurrentBossIndex < bossSequence.Count;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        CreateQueue();
        InitializeTraitPools();

        SaveData save = SaveManager.Instance.LoadProgress();

        if (save != null)
        {
            CurrentStageIndex = save.stageIndex;
            CurrentBossIndex = save.bossIndex;

            defeatedJack = Mathf.Min(CurrentBossIndex, jackBosses.Count);

            if (CurrentBossIndex >= jackBosses.Count)
                defeatedQueen = Mathf.Min(CurrentBossIndex - jackBosses.Count, queenBosses.Count);

            if (CurrentBossIndex >= jackBosses.Count + queenBosses.Count)
                defeatedKing = Mathf.Min(
                    CurrentBossIndex - jackBosses.Count - queenBosses.Count,
                    kingBosses.Count);
        }
        else
        {
            CurrentBossIndex = 0;
            CurrentStageIndex = 0;
        }

        StageManager.Instance.ApplyStage(CurrentStageIndex);

        LoadNextBoss();
    }

    private void CreateQueue()
    {
        bossSequence.Clear();

        List<BossSO> j = new(jackBosses);
        List<BossSO> q = new(queenBosses);
        List<BossSO> k = new(kingBosses);
        List<BossSO> joker = new(jokerBosses);

        Shuffle(j);
        Shuffle(q);
        Shuffle(k);
        Shuffle(joker);

        bossSequence.AddRange(j);
        bossSequence.AddRange(q);
        bossSequence.AddRange(k);
        bossSequence.AddRange(joker);
    }
    private void InitializeTraitPools()
    {
        if (jackBosses.Count > 0)
        {
            TraitPoolManager.Instance.InitializePool(
                BossRank.Jack,
                jackBosses[0].possibleTraits
            );
        }

        if (queenBosses.Count > 0)
        {
            TraitPoolManager.Instance.InitializePool(
                BossRank.Queen,
                queenBosses[0].possibleTraits
            );
        }

        if (kingBosses.Count > 0)
        {
            TraitPoolManager.Instance.InitializePool(
                BossRank.King,
                kingBosses[0].possibleTraits
            );
        }
        Debug.Log("Jack Trait : " + jackBosses[0].possibleTraits.Count);
        Debug.Log("Queen Trait : " + queenBosses[0].possibleTraits.Count);
        Debug.Log("King Trait : " + kingBosses[0].possibleTraits.Count);
    }
    private void Shuffle(List<BossSO> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);
            (list[i], list[random]) = (list[random], list[i]);
        }
    }

    public bool LoadNextBoss()
    {
        Debug.Log("Load Boss Index = " + CurrentBossIndex);
        if (CurrentBossIndex >= bossSequence.Count)
            return false;

        CurrentBoss = bossSequence[CurrentBossIndex];

        CurrentHP = CurrentBoss.hp;
        CurrentBoss.currentATK = CurrentBoss.atk;

        bossDisplay.Setup(CurrentBoss);
        bossInfoPanel.Setup(CurrentBoss);

        bossDisplay.UpdateHP(CurrentHP);
        bossDisplay.UpdateATK(CurrentATK);

        if (CurrentBoss.rank == BossRank.Joker)
        {
            traitSelectionPanel.SetVisible(false);
            BattleManager.Instance.StartBattleAfterTraitSelected();
        }
        else
        {
            traitSelectionPanel.SetVisible(true);
            traitSelectionPanel.Show(CurrentBoss);
        }

        if (!string.IsNullOrEmpty(CurrentBoss.spawnSoundID))
            SoundManager.instance?.PlaySound2D(CurrentBoss.spawnSoundID);

        BossFXManager.Instance?.PlaySpawnFX(bossDisplay.transform);

        return true;
    }

    public void OnBossDefeated(BossSO deadBoss)
    {
        CurrentBossIndex++;

        switch (deadBoss.rank)
        {
            case BossRank.Jack:
                defeatedJack++;
                if (defeatedJack == jackBosses.Count)
                    CurrentStageIndex = 1;
                break;

            case BossRank.Queen:
                defeatedQueen++;
                if (defeatedQueen == queenBosses.Count)
                    CurrentStageIndex = 2;
                break;

            case BossRank.King:
                defeatedKing++;
                if (defeatedKing == kingBosses.Count)
                    CurrentStageIndex = 3;
                break;
        }

        SaveManager.Instance.SaveProgress(CurrentStageIndex, CurrentBossIndex);
    }

    public void TakeDamage(int damage)
    {
        int hpBefore = CurrentHP;

        CurrentHP -= damage;

        LastKillWasPerfect = false;

        if (CurrentHP <= 0)
        {
            LastKillWasPerfect = (damage == hpBefore);
            CurrentHP = 0;
        }

        bossDisplay.UpdateHP(CurrentHP);
    }
    public void TakeTraitDamage(int damage)
    {
        CurrentHP -= damage;

        if (CurrentHP < 0)
            CurrentHP = 0;

        bossDisplay.UpdateHP(CurrentHP);
    }
    public void Heal(int amount)
    {
        CurrentHP += amount;

        if (CurrentHP > CurrentBoss.hp)
            CurrentHP = CurrentBoss.hp;

        bossDisplay.UpdateHP(CurrentHP);
    }
    public void ReduceAttack(int value)
    {
        CurrentBoss.currentATK -= value;

        if (CurrentBoss.currentATK < 0)
            CurrentBoss.currentATK = 0;

        bossDisplay.UpdateATK(CurrentBoss.currentATK);
    }

    public bool NeedChangeStage(BossSO deadBoss)
    {
        switch (deadBoss.rank)
        {
            case BossRank.Jack:
                defeatedJack++;
                return defeatedJack == jackBosses.Count;

            case BossRank.Queen:
                defeatedQueen++;
                return defeatedQueen == queenBosses.Count;

            case BossRank.King:
                defeatedKing++;
                return defeatedKing == kingBosses.Count;
        }

        return false;
    }
    public void RandomizeJokerSuit()
    {
        if (CurrentBoss == null)
            return;

        if (!CurrentBoss.isJoker)
            return;

        CardSO.Suit[] suits =
        {
        CardSO.Suit.Hearts,
        CardSO.Suit.Diamonds,
        CardSO.Suit.Clubs,
        CardSO.Suit.Spades
    };

        CardSO.Suit newSuit;

        do
        {
            newSuit = suits[Random.Range(0, suits.Length)];
        }
        while (newSuit == CurrentBoss.resistanceSuit);

        CurrentBoss.resistanceSuit = newSuit;
        bossDisplay.UpdateResistance(newSuit);

        Debug.Log($"Joker đổi sang {newSuit}");
    }
    public void RefreshBossInfo()
    {
        if (bossInfoPanel != null)
        {
            bossInfoPanel.Setup(CurrentBoss);
        }
    }
    public void MoveNextBoss()
    {
        CurrentBossIndex++;

        SaveManager.Instance.SaveProgress(CurrentStageIndex, CurrentBossIndex);

        LoadNextBoss();
    }
    public bool IsDead()
    {
        return CurrentHP <= 0;
    }
}