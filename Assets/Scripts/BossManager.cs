using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [Header("Boss Lists")]
    [SerializeField] private List<BossSO> jackBosses;
    [SerializeField] private List<BossSO> queenBosses;
    [SerializeField] private List<BossSO> kingBosses;

    [Header("Display")]
    [SerializeField] private BossDisplay bossDisplay;
    public Transform BossTransform => bossDisplay.transform;

    private readonly Queue<BossSO> bossQueue = new();

    public BossSO CurrentBoss { get; private set; }

    public int CurrentHP { get; private set; }

    public int CurrentATK { get; private set; }

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
        LoadNextBoss();
    }

    private void CreateQueue()
    {
        bossQueue.Clear();

        List<BossSO> j = new(jackBosses);
        List<BossSO> q = new(queenBosses);
        List<BossSO> k = new(kingBosses);

        Shuffle(j);
        Shuffle(q);
        Shuffle(k);

        foreach (BossSO boss in j)
            bossQueue.Enqueue(boss);

        foreach (BossSO boss in q)
            bossQueue.Enqueue(boss);

        foreach (BossSO boss in k)
            bossQueue.Enqueue(boss);
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
        CurrentATK = CurrentBoss.atk;

        bossDisplay.Setup(CurrentBoss);

        if (!string.IsNullOrEmpty(CurrentBoss.bossName))
        {
            SoundManager.instance?.PlaySound2D(CurrentBoss.bossName);
        }

        if (BossFXManager.Instance)
            BossFXManager.Instance.PlaySpawnFX(bossDisplay.transform);

        return true;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;

        if (CurrentHP < 0)
            CurrentHP = 0;

        bossDisplay.UpdateHP(CurrentHP);
    }

    public bool IsDead()
    {
        return CurrentHP <= 0;
    }
}