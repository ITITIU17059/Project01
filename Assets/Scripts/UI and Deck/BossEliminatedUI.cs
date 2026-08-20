using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossEliminatedUI : MonoBehaviour
{
    public static BossEliminatedUI Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Screen Effects")]
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Image flashImage;

    [Header("Bars")]
    [SerializeField] private RectTransform leftBar;
    [SerializeField] private RectTransform rightBar;

    [Header("Announcement")]
    [SerializeField] private RectTransform announcement;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI bossText;
    [SerializeField] private TextMeshProUGUI eliminatedText;

    [Header("Lines")]
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine;

    [Header("Sound")]
    [SerializeField] private string impactSoundID = "BossEliminated";

    [Header("Settings")]
    [SerializeField] private float displayDuration = 1.5f;

    [SerializeField] private float slowMotionScale = 0.25f;
    [SerializeField] private float slowMotionDuration = 0.2f;

    private Vector2 leftBarStart;
    private Vector2 rightBarStart;
    private GameObject musicManger;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicManger = GameObject.FindGameObjectWithTag("Music");
        audioSource = musicManger.GetComponent<AudioSource>();

        leftBarStart = leftBar.anchoredPosition;
        rightBarStart = rightBar.anchoredPosition;

        canvasGroup.gameObject.SetActive(false);
    }

    public IEnumerator Play()
    {
        canvasGroup.gameObject.SetActive(true);

        ResetUI();

        yield return PlaySlowMotion();

        yield return PlayImpact();

        yield return new WaitForSecondsRealtime(displayDuration);

        yield return PlayExit();

        canvasGroup.gameObject.SetActive(false);
    }

    private void ResetUI()
    {
        DOTween.Kill(this);

        Time.timeScale = 1f;

        canvasGroup.alpha = 1f;

        SetAlpha(darkOverlay, 0f);
        SetAlpha(flashImage, 0f);

        leftBar.anchoredPosition =
            new Vector2(-2000f, leftBarStart.y);

        rightBar.anchoredPosition =
            new Vector2(2000f, rightBarStart.y);

        announcement.localScale = Vector3.zero;

        bossText.alpha = 0f;
        eliminatedText.alpha = 0f;

        topLine.localScale =
            new Vector3(0f, 1f, 1f);

        bottomLine.localScale =
            new Vector3(0f, 1f, 1f);
    }

    private IEnumerator PlaySlowMotion()
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = slowMotionScale;

        yield return new WaitForSecondsRealtime(
            slowMotionDuration
        );

        Time.timeScale = originalTimeScale;
    }

    private IEnumerator PlayImpact()
    {
        Sequence sequence = DOTween.Sequence();

        /*
         * DARK OVERLAY
         */

        sequence.Append(
            darkOverlay
                .DOFade(0.65f, 0.08f)
                .SetUpdate(true)
        );

        /*
         * FLASH
         */

        sequence.Join(
            flashImage
                .DOFade(0.85f, 0.04f)
                .SetUpdate(true)
        );

        sequence.Append(
            flashImage
                .DOFade(0f, 0.18f)
                .SetUpdate(true)
        );

        /*
         * BARS SLIDE IN
         */

        sequence.Join(
            leftBar
                .DOAnchorPosX(-350f, 0.25f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
        );

        sequence.Join(
            rightBar
                .DOAnchorPosX(350f, 0.25f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
        );

        /*
         * BOSS TEXT
         */

        sequence.Join(
            bossText
                .DOFade(1f, 0.12f)
                .SetUpdate(true)
        );

        /*
         * ANNOUNCEMENT IMPACT
         */

        sequence.Append(
            announcement
                .DOScale(1.45f, 0.08f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
        );
        audioSource.volume = 0;
        SoundManager.instance?.PlaySound2D(impactSoundID);

        sequence.Append(
            announcement
                .DOScale(0.92f, 0.08f)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true)
        );

        sequence.Append(
            announcement
                .DOScale(1f, 0.1f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
        );

        /*
         * ELIMINATED TEXT
         */

        sequence.Join(
            eliminatedText
                .DOFade(1f, 0.05f)
                .SetUpdate(true)
        );

        /*
         * LINES
         */

        sequence.Join(
            topLine
                .DOScaleX(1f, 0.25f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
        );

        sequence.Join(
            bottomLine
                .DOScaleX(1f, 0.25f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true)
        );

        /*
         * IMPACT SHAKE
         */

        sequence.Join(
            announcement
                .DOShakeAnchorPos(
                    0.22f,
                    new Vector2(25f, 8f),
                    20,
                    90f
                )
                .SetUpdate(true)
        );

        /*
         * EXTRA PUNCH
         */

        sequence.Append(
            announcement
                .DOPunchScale(
                    new Vector3(
                        0.12f,
                        0.12f,
                        0f
                    ),
                    0.2f,
                    8,
                    0.6f
                )
                .SetUpdate(true)
        );

        yield return sequence.WaitForCompletion();

    }

    private IEnumerator PlayExit()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            announcement
                .DOScale(1.1f, 0.15f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
        );

        sequence.Join(
            canvasGroup
                .DOFade(0f, 0.3f)
                .SetUpdate(true)
        );

        sequence.Join(
            leftBar
                .DOAnchorPosX(-1000f, 0.3f)
                .SetEase(Ease.InExpo)
                .SetUpdate(true)
        );

        sequence.Join(
            rightBar
                .DOAnchorPosX(1000f, 0.3f)
                .SetEase(Ease.InExpo)
                .SetUpdate(true)
        );

        yield return sequence.WaitForCompletion();
    }

    private void SetAlpha(
        Image image,
        float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;

        color.a = alpha;

        image.color = color;
    }
}