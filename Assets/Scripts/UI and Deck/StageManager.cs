using System.Collections;
using System.Collections.Generic;
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
    private List<string> JackThemeList;
    private List<string> QueenThemeList;
    private List<string> KingThemeList;

    private void Awake()
    {
        Instance = this;
        JackThemeList = new List<string>() { "JackTheme1", "JackTheme2", "JackTheme3", "JackTheme4" };
        QueenThemeList = new List<string>() { "QueenTheme1", "QueenTheme2", "QueenTheme3", "QueenTheme4" };
        KingThemeList = new List<string>() { "KingTheme1", "KingTheme2", "KingTheme3", "KingTheme4" };
    }

    private void Start()
    {
        background.transform.SetAsFirstSibling();
    }

    public IEnumerator ChangeStage(BossRank nextStage)
    {
        int randomTheme;
        yield return fadeGroup
            .DOFade(1, 0.5f)
            .WaitForCompletion();

        switch (nextStage)
        {
            case BossRank.Jack:

                background.texture = jackTextture;
                videoPlayer.clip = jackBackground;
                videoPlayer.targetTexture = jackTextture;
                randomTheme = Random.Range(0, JackThemeList.Count);
                MusicManager.instance.PlayMusic(JackThemeList[randomTheme]);
                JackThemeList.Remove(JackThemeList[randomTheme]);
                break;

            case BossRank.Queen:

                background.texture = queenTextture;
                videoPlayer.clip = queenBackground;
                videoPlayer.targetTexture = queenTextture;
                randomTheme = Random.Range(0, QueenThemeList.Count);
                MusicManager.instance.PlayMusic(QueenThemeList[randomTheme]);
                QueenThemeList.Remove(QueenThemeList[randomTheme]);
                break;

            case BossRank.King:

                background.texture = kingTextture;
                videoPlayer.clip = kingBackground;
                videoPlayer.targetTexture = kingTextture;
                randomTheme = Random.Range(0, KingThemeList.Count);
                MusicManager.instance.PlayMusic(KingThemeList[randomTheme]);
                KingThemeList.Remove(KingThemeList[randomTheme]);
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
        videoPlayer.clip = victoryBackground;
        videoPlayer.targetTexture = victoryTextture;

        MusicManager.instance.PlayMusic("VictoryTheme");

        background.transform.SetAsFirstSibling();

        yield return fadeGroup
            .DOFade(0, 0.5f)
            .WaitForCompletion();
    }

    public void ApplyStage(int stageIndex)
    {
        int randomTheme;
        switch (stageIndex)
        {
            case 0:
                background.texture = jackTextture;
                videoPlayer.clip = jackBackground;
                videoPlayer.targetTexture = jackTextture;
                randomTheme = Random.Range(0, JackThemeList.Count);
                MusicManager.instance.PlayMusic(JackThemeList[randomTheme]);
                JackThemeList.Remove(JackThemeList[randomTheme]);
                break;

            case 1:
                background.texture = queenTextture;
                videoPlayer.clip = queenBackground;
                videoPlayer.targetTexture = queenTextture;
                randomTheme = Random.Range(0, QueenThemeList.Count);
                MusicManager.instance.PlayMusic(QueenThemeList[randomTheme]);
                QueenThemeList.Remove(QueenThemeList[randomTheme]);
                break;

            case 2:
                background.texture = kingTextture;
                videoPlayer.clip = kingBackground;
                videoPlayer.targetTexture = kingTextture;
                randomTheme = Random.Range(0, KingThemeList.Count);
                MusicManager.instance.PlayMusic(KingThemeList[randomTheme]);
                KingThemeList.Remove(KingThemeList[randomTheme]);
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