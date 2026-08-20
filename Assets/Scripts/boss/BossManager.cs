using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI sign_heal_boss;
    public TextMeshProUGUI sign_attack_boss;
    public BossDisplay BossDisplay => bossDisplay;

    [SerializeField]
    private List<BossSO> disguisePool;
    public List<BossSO> DisguisePool => disguisePool;
    public Transform BossTransform => bossDisplay.transform;
    public DamageManager damageManager;

    private readonly List<BossSO> bossSequence = new();
    private bool initialized;
    public int CurrentBossIndex { get; private set; }
    public int CurrentStageIndex { get; private set; }

    private int defeatedJack;
    private int defeatedQueen;
    private int defeatedKing;

    [SerializeField] private List<BossSO> illusionBosses;
    public BossSO CurrentBoss { get; private set; }

    public int CurrentHP { get; private set; }
    private int maxRuntimeHP;
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
        InitializeTraitPools();

        SaveData save = SaveManager.Instance.LoadProgress();

        if (save != null)
        {
            LoadBossSequence(save.bossSequence);
            CurrentStageIndex = save.stageIndex;
            CurrentBossIndex = save.bossIndex;

            PlayerReward.Instance.LoadRewards(
            save.ownedRewards,
            save.equippedRewards);
            PlayerReward.Instance.LoadTraitHasAdd(
    save.traitHasAdd);
            TraitPoolManager.Instance.LoadPool(
                BossRank.Jack,
                save.jackTraitPool);

            TraitPoolManager.Instance.LoadPool(
                BossRank.Queen,
                save.queenTraitPool);

            TraitPoolManager.Instance.LoadPool(
                BossRank.King,
                save.kingTraitPool);

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
            PlayerReward.Instance.ResetTraitHasAdd();
            CreateQueue();
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
        if (CurrentBossIndex >= bossSequence.Count)
            return false;

        CurrentBoss = bossSequence[CurrentBossIndex];

        if (CurrentBoss == null)
            return false;

        if (PlayerReward.Instance != null)
        {
            PlayerReward.Instance.ResetAceHandBonus();
        }

        // Reset trait của boss mới
        if (!CurrentBoss.isJoker)
        {
            CurrentBoss.currentTrait = null;
        }

        maxRuntimeHP = CurrentBoss.hp;
        CurrentHP = maxRuntimeHP;

        CurrentBoss.currentATK = CurrentBoss.atk;
        CurrentBoss.turnCounter = 0;

        if (CurrentBoss.isJoker)
        {
            CurrentBoss.currentTrait = FindJokerTrait();

            SetupJokerRuntime();
        }

        bossDisplay.Setup(CurrentBoss);
        bossInfoPanel.Setup(CurrentBoss);

        bossDisplay.UpdateHP(CurrentHP);
        bossDisplay.UpdateATK(CurrentBoss.currentATK);
        bossDisplay.UpdateResistance(CurrentBoss.resistanceSuit);

        if (CurrentBoss.isJoker)
        {
            traitSelectionPanel.SetVisible(false);

            RandomizeJokerSuit();
        }
        else
        {
            traitSelectionPanel.SetVisible(true);
            traitSelectionPanel.Show(CurrentBoss);
        }

        BossFXManager.Instance?.PlaySpawnFX(
           bossDisplay.transform);

        return true;
    }
    private void SetupJokerRuntime()
    {
        if (CurrentBoss == null)
            return;

        if (!CurrentBoss.isJoker)
            return;

        if (PlayerReward.Instance == null)
            return;

        int equippedCount = 0;

        foreach (RewardSO reward in PlayerReward.Instance.EquippedRewards)
        {
            if (reward == null)
                continue;

            equippedCount++;
        }

        if (equippedCount > 0)
        {
            maxRuntimeHP += equippedCount * 5;
            CurrentHP = maxRuntimeHP;

            CurrentBoss.currentATK += equippedCount * 3;
        }
    }
    private BossTraitSO FindJokerTrait()
    {
        if (CurrentBoss == null)
            return null;

        if (CurrentBoss.possibleTraits == null)
            return null;

        foreach (BossTraitSO trait in CurrentBoss.possibleTraits)
        {
            if (trait == null)
                continue;

            if (trait.traitID == TraitID.JOKER)
                return trait;
        }

        return null;
    }

    public void OnBossDefeated(BossSO deadBoss)
    {
        if (deadBoss == null)
            return;

        // Joker không nằm trong trait pool thông thường
        if (deadBoss.currentTrait != null &&
            deadBoss.rank != BossRank.Joker &&
            TraitPoolManager.Instance != null)
        {
            TraitPoolManager.Instance.RemoveTrait(
                deadBoss.rank,
                deadBoss.currentTrait);
        }

        // Trait Selection có thể không tồn tại / không active
        if (TraitSelectionPanelUI.Instance != null)
        {
            TraitSelectionPanelUI.Instance.ClearCurrentTraits();
        }

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

            case BossRank.Joker:

                break;
        }

        SaveManager.Instance.SaveProgress(
            CurrentStageIndex,
            CurrentBossIndex);
    }

    public void TakeDamage(int damage)
    {
        int hpBefore = CurrentHP;
        sign_heal_boss.text = "-";

        CurrentHP -= damage;
        sign_heal_boss.text = "-";

        LastKillWasPerfect = false;

        if (CurrentHP <= 0)
        {
            LastKillWasPerfect = (damage == hpBefore);
            CurrentHP = 0;
        }

        bossDisplay.UpdateHP(CurrentHP);
        StartCoroutine(damageManager.ShowBossHeal(damage));
    }
    public void TakeTraitDamage(int damage)
    {
        CurrentHP -= damage;
        sign_heal_boss.text = "-";

        if (CurrentHP < 0)
            CurrentHP = 0;

        bossDisplay.UpdateHP(CurrentHP);
        StartCoroutine(damageManager.ShowBossHeal(damage));
    }
    public void Heal(int amount)
    {
        CurrentHP += amount;
        sign_heal_boss.text = "+";

        if (CurrentHP > maxRuntimeHP)
            CurrentHP = maxRuntimeHP;

        bossDisplay.UpdateHP(CurrentHP);
        StartCoroutine(damageManager.ShowBossHeal(amount));
    }
    public void ReduceAttack(int value)
    {
        CurrentBoss.currentATK -= value;
        sign_attack_boss.text = "-";

        if (CurrentBoss.currentATK < 0)
            CurrentBoss.currentATK = 0;

        bossDisplay.UpdateATK(CurrentBoss.currentATK);
        StartCoroutine(damageManager.ShowBossAttack(value));
    }

    public void HealAttack(int value)
    {
        CurrentBoss.currentATK += value;
        sign_attack_boss.text = "+";

        bossDisplay.UpdateATK(CurrentBoss.currentATK);
        StartCoroutine(damageManager.ShowBossAttack(value));
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

    public List<string> GetBossSequence()
    {
        List<string> data = new();

        foreach (BossSO boss in bossSequence)
            data.Add(boss.name);

        return data;
    }

    public void LoadBossSequence(List<string> data)
    {
        bossSequence.Clear();

        foreach (string bossName in data)
        {
            BossSO boss = Resources.Load<BossSO>("BossSO/" + bossName);

            if (boss != null)
                bossSequence.Add(boss);
        }
    }
    public void RandomizeJokerDisguise()
    {
        if (CurrentBoss == null)
            return;

        if (!CurrentBoss.isJoker)
            return;

        if (disguisePool == null || disguisePool.Count == 0)
            return;

        BossSO disguise =
            disguisePool[Random.Range(0, disguisePool.Count)];

        if (disguise == null)
            return;

        // Sprite quyết định suit gây DAMAGE
        CurrentBoss.currentDisguiseSprite =
            disguise.cardSprite;

        CurrentBoss.jokerDamageSuit =
            disguise.resistanceSuit;

        bossDisplay.SetBossSprite(
            disguise.cardSprite);

        Debug.Log(
            $"[JOKER] Disguise: {disguise.bossName} | " +
            $"Damage Suit: {CurrentBoss.jokerDamageSuit}");
    }

}