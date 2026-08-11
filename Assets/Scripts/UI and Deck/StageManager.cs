using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Background")]
    [SerializeField] private RawImage background;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Stage Background")]
    // [SerializeField] private Sprite jackBackground;
    // [SerializeField] private Sprite queenBackground;
    // [SerializeField] private Sprite kingBackground;
    // [SerializeField] private Sprite jokerBackground;
    // [SerializeField] private Sprite victoryBackground;
    [SerializeField] private VideoClip jackBackground;
    [SerializeField] private VideoClip queenBackground;
    [SerializeField] private VideoClip kingBackground;
    [SerializeField] private VideoClip jokerBackground;
    [SerializeField] private VideoClip victoryBackground;
    [SerializeField] private RenderTexture jackTextture;
    [SerializeField] private RenderTexture queenTextture;
    [SerializeField] private RenderTexture kingTextture;
    [SerializeField] private RenderTexture jokerTextture;
    [SerializeField] private RenderTexture victoryTextture;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        background.transform.SetAsFirstSibling();
    }

    public IEnumerator ChangeStage(BossRank nextStage)
    {
        yield return fadeGroup
            .DOFade(1, 0.5f)
            .WaitForCompletion();

        switch (nextStage)
        {
            case BossRank.Jack:

                background.texture = jackTextture;
                videoPlayer.clip = jackBackground;
                videoPlayer.targetTexture = jackTextture;
                MusicManager.instance.PlayMusic("JackTheme");
                break;

            case BossRank.Queen:

                background.texture = queenTextture;
                videoPlayer.clip = queenBackground;
                videoPlayer.targetTexture = queenTextture;
                MusicManager.instance.PlayMusic("QueenTheme");
                break;

            case BossRank.King:

                background.texture = kingTextture;
                videoPlayer.clip = kingBackground;
                videoPlayer.targetTexture = kingTextture;
                MusicManager.instance.PlayMusic("KingTheme");
                break;

            case BossRank.Joker:

                background.texture = jokerTextture;
                videoPlayer.clip = jokerBackground;
                videoPlayer.targetTexture = jokerTextture;
                MusicManager.instance.PlayMusic("JokerTheme");
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

        background.texture = victoryTextture;

        MusicManager.instance.PlayMusic("VictoryTheme");

        background.transform.SetAsFirstSibling();

        yield return fadeGroup
            .DOFade(0, 0.5f)
            .WaitForCompletion();
    }

    public void ApplyStage(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0:
                background.texture = jackTextture;
                videoPlayer.clip = jackBackground;
                videoPlayer.targetTexture = jackTextture;
                MusicManager.instance.PlayMusic("JackTheme");
                break;

            case 1:
                background.texture = queenTextture;
                videoPlayer.clip = queenBackground;
                videoPlayer.targetTexture = queenTextture;
                MusicManager.instance.PlayMusic("QueenTheme");
                break;

            case 2:
                background.texture = kingTextture;
                videoPlayer.clip = kingBackground;
                videoPlayer.targetTexture = kingTextture;
                MusicManager.instance.PlayMusic("KingTheme");
                break;

            case 3:
                background.texture = jokerTextture;
                videoPlayer.clip = jokerBackground;
                videoPlayer.targetTexture = jokerTextture;
                MusicManager.instance.PlayMusic("JokerTheme");
                break;
        }

        background.transform.SetAsFirstSibling();
    }
}