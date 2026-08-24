using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JesterUnlockUI : MonoBehaviour
{
    public static JesterUnlockUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform content;

    [Header("Jester Cards")]
    [SerializeField] private RectTransform resetJester;
    [SerializeField] private RectTransform instantKillJester;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Button")]
    [SerializeField] private Button continueButton;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float popupDuration = 0.45f;

    private bool isShowing;

    private Vector3 contentOriginalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (content != null)
        {
            contentOriginalScale =
                content.localScale;
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                Hide
            );
        }

        HideImmediate();
    }

    public void Show()
    {
        if (isShowing)
            return;

        StartCoroutine(
            ShowRoutine()
        );
    }

    private IEnumerator ShowRoutine()
    {
        isShowing = true;

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (content != null)
        {
            content.DOKill();
            content.localScale =
                contentOriginalScale * 0.75f;
        }

        if (titleText != null)
        {
            titleText.text =
                "JESTER UNLOCKED";
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "You received two powerful Jester cards.";
        }

        Sequence sequence =
            DOTween.Sequence();

        if (canvasGroup != null)
        {
            sequence.Join(
                canvasGroup.DOFade(
                    1f,
                    fadeDuration
                )
            );
        }

        if (content != null)
        {
            sequence.Join(
                content.DOScale(
                    contentOriginalScale,
                    popupDuration
                )
                .SetEase(
                    Ease.OutBack
                )
            );
        }

        yield return sequence
            .WaitForCompletion();
    }

    public void Hide()
    {
        if (!isShowing)
            return;

        StartCoroutine(
            HideRoutine()
        );
    }

    private IEnumerator HideRoutine()
    {
        isShowing = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        Sequence sequence =
            DOTween.Sequence();

        if (canvasGroup != null)
        {
            sequence.Join(
                canvasGroup.DOFade(
                    0f,
                    fadeDuration
                )
            );
        }

        if (content != null)
        {
            sequence.Join(
                content.DOScale(
                    contentOriginalScale * 0.85f,
                    fadeDuration
                )
                .SetEase(
                    Ease.InCubic
                )
            );
        }

        yield return sequence
            .WaitForCompletion();

        gameObject.SetActive(false);

        // Cho BattleManager biết popup đã đóng
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnJesterUnlockPopupClosed();
        }
    }

    private void HideImmediate()
    {
        isShowing = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (content != null)
        {
            content.localScale =
                contentOriginalScale;
        }

        gameObject.SetActive(false);
    }
}