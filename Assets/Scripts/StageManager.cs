using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Background")]
    [SerializeField] private Image background;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Stage Background")]
    [SerializeField] private Sprite jackBackground;
    [SerializeField] private Sprite queenBackground;
    [SerializeField] private Sprite kingBackground;
    [SerializeField] private Sprite victoryBackground;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        background.sprite = jackBackground;
        background.transform.SetAsFirstSibling();

        MusicManager.instance.PlayMusic("JackTheme");
    }

    public IEnumerator ChangeStage(BossRank nextStage)
    {
        yield return fadeGroup
            .DOFade(1, 0.5f)
            .WaitForCompletion();

        switch (nextStage)
        {
            case BossRank.Jack:

                background.sprite = jackBackground;
                MusicManager.instance.PlayMusic("JackTheme");
                break;

            case BossRank.Queen:

                background.sprite = queenBackground;
                MusicManager.instance.PlayMusic("QueenTheme");
                break;

            case BossRank.King:

                background.sprite = kingBackground;
                MusicManager.instance.PlayMusic("KingTheme");
                break;
        }

        background.transform.SetAsFirstSibling();

        yield return fadeGroup
            .DOFade(0, 0.5f)
            .WaitForCompletion();
    }

    public IEnumerator VictoryStage()
    {
        yield return fadeGroup
            .DOFade(1, 0.5f)
            .WaitForCompletion();

        background.sprite = victoryBackground;

        MusicManager.instance.PlayMusic("VictoryTheme");

        background.transform.SetAsFirstSibling();

        yield return fadeGroup
            .DOFade(0, 0.5f)
            .WaitForCompletion();
    }
}