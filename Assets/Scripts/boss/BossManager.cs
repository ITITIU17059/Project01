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

    private readonly Queue<BossSO> bossQueue = new();

    private int defeatedJack;
    private int defeatedQueen;
    private int defeatedKing;
 

    public BossSO CurrentBoss { get; private set; }

    public int CurrentHP { get; private set; }

    public int CurrentATK => CurrentBoss.currentATK;
    public bool LastKillWasPerfect { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Initialize()
    {
        CreateQueue();
        InitializeTraitPools();
        LoadNextBoss();
    }

    private void CreateQueue()
    {
        bossQueue.Clear();

        List<BossSO> j = new(jackBosses);
        List<BossSO> q = new(queenBosses);
        List<BossSO> k = new(kingBosses);
        List<BossSO> l = new(jokerBosses);

        Shuffle(j);
        Shuffle(q);
        Shuffle(k);
        Shuffle(l);

        foreach (BossSO boss in j)
            bossQueue.Enqueue(boss);

        foreach (BossSO boss in q)
            bossQueue.Enqueue(boss);

        foreach (BossSO boss in k)
            bossQueue.Enqueue(boss);

        foreach (BossSO boss in l)
            bossQueue.Enqueue(boss);
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
        if (bossQueue.Count == 0)
            return false;

        CurrentBoss = bossQueue.Dequeue();

        CurrentHP = CurrentBoss.hp;
        CurrentBoss.currentATK = CurrentBoss.atk;

        bossDisplay.Setup(CurrentBoss);
        bossInfoPanel.Setup(CurrentBoss);
        bossDisplay.UpdateHP(CurrentHP);
        bossDisplay.UpdateATK(CurrentATK);
        traitSelectionPanel.Show(CurrentBoss);


        if (!string.IsNullOrEmpty(CurrentBoss.spawnSoundID))
        {
            SoundManager.instance?.PlaySound2D(CurrentBoss.spawnSoundID);
        }
     
        if (BossFXManager.Instance != null)
        {
            BossFXManager.Instance.PlaySpawnFX(bossDisplay.transform);
        }

        return true;
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
    public bool IsDead()
    {
        return CurrentHP <= 0;
    }
}