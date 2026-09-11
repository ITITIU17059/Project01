using UnityEngine;

public class BossCrackManager : MonoBehaviour
{
    public static BossCrackManager Instance { get; set; }

    [SerializeField] private GameObject crackLine1;
    [SerializeField] private GameObject crackLine2;
    [SerializeField] private GameObject crackLine3;
    [SerializeField] private GameObject crackLine4;

    private int boss20Health;
    private int boss40Health;
    private int boss60Health;
    private int boss80Health;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetUp()
    {
        boss20Health = BossManager.Instance.CurrentBoss.hp - BossManager.Instance.CurrentBoss.hp / 5;
        boss40Health = BossManager.Instance.CurrentBoss.hp - 2 * (BossManager.Instance.CurrentBoss.hp / 5);
        boss60Health = BossManager.Instance.CurrentBoss.hp - 3 * (BossManager.Instance.CurrentBoss.hp / 5);
        boss80Health = BossManager.Instance.CurrentBoss.hp - 4 * (BossManager.Instance.CurrentBoss.hp / 5);

        if (BossManager.Instance.CurrentHP <= boss80Health)
        {
            crackLine1.SetActive(true);
            crackLine2.SetActive(true);
            crackLine3.SetActive(true);
            crackLine4.SetActive(true);
        }
        else if (BossManager.Instance.CurrentHP <= boss60Health)
        {
            crackLine1.SetActive(true);
            crackLine2.SetActive(true);
            crackLine3.SetActive(true);
        }
        else if (BossManager.Instance.CurrentHP <= boss40Health)
        {
            crackLine1.SetActive(true);
            crackLine2.SetActive(true);
        }
        else if (BossManager.Instance.CurrentHP <= boss20Health)
        {
            crackLine1.SetActive(true);
        }
    }

    public void ResetCrackLine()
    {
        crackLine1.SetActive(false);
        crackLine2.SetActive(false);
        crackLine3.SetActive(false);
        crackLine4.SetActive(false);
    }
}
