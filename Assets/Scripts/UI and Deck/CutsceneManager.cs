using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneFrame
    {
        [Header("Visual")]
        public Sprite sprite;

        [Header("Voice")]
        public AudioClip voiceClip;

        [Header("Playback")]
        public bool waitVoiceFinish = true;

        [Min(0.5f)]
        public float duration = 3f;
    }

    [Header("UI")]
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private Image fadePanel;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Background Music")]
    [SerializeField] private string introMusicName = "IntroTheme";

    [Header("Frames")]
    [SerializeField] private CutsceneFrame[] frames;

    [Header("Transition")]
    [SerializeField] private float fadeTime = 0.4f;

    private bool isEnding;

    private void Start()
    {
        Color color = fadePanel.color;
        color.a = 0;
        fadePanel.color = color;
        currentImage.sprite = frames[0].sprite;

        currentImage.color = Color.white;

        Color c = nextImage.color;
        c.a = 0;
        nextImage.color = c;

        if (MusicManager.instance != null)
        {
            MusicManager.instance.SetMusicMultiplier(0.25f);
            MusicManager.instance.PlayMusic(introMusicName, 1f);
        }

        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        if (frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        // ===== Frame đầu =====
        currentImage.sprite = frames[0].sprite;
        currentImage.color = Color.white;

        if (frames[0].voiceClip != null)
        {
            voiceSource.clip = frames[0].voiceClip;
            voiceSource.Play();
        }

        if (frames[0].waitVoiceFinish && frames[0].voiceClip != null)
        {
            while (voiceSource.isPlaying)
                yield return null;
        }
        else
        {
            yield return new WaitForSeconds(frames[0].duration);
        }

        // ===== Các frame còn lại =====
        for (int i = 1; i < frames.Length; i++)
        {
            CutsceneFrame frame = frames[i];

            yield return ChangeImage(frame.sprite);

            if (frame.voiceClip != null)
            {
                voiceSource.Stop();
                voiceSource.clip = frame.voiceClip;
                voiceSource.Play();
            }

            if (frame.waitVoiceFinish && frame.voiceClip != null)
            {
                while (voiceSource.isPlaying)
                    yield return null;
            }
            else
            {
                yield return new WaitForSeconds(frame.duration);
            }
        }

        EndCutscene();
    }

    public void Skip()
    {
        EndCutscene();
    }

    private void EndCutscene()
    {
        if (isEnding)
            return;

        isEnding = true;

        StopAllCoroutines();

        if (voiceSource != null)
            voiceSource.Stop();


        if (MusicManager.instance != null)
            MusicManager.instance.SetMusicMultiplier(1f);

        if (LevelManager.instance != null)
            LevelManager.instance.LoadIntro();
    }

    private IEnumerator ChangeImage(Sprite nextSprite)
    {
        // Fade sang đen
        yield return fadePanel
            .DOFade(1f, 0.25f)
            .WaitForCompletion();

        // Đổi ảnh
        currentImage.sprite = nextSprite;

        // Fade trở lại
        yield return fadePanel
            .DOFade(0f, 0.25f)
            .WaitForCompletion();
    }
}