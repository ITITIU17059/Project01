using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;

public class JesterUnlockUI : MonoBehaviour
{
    public static JesterUnlockUI Instance { get; private set; }

    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private CanvasGroup popupCanvasGroup;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button continueButton;

    public bool IsShowing { get; private set; }

    private bool keepHandsHiddenAfterClose = false;

    public void SetKeepHandsHiddenAfterClose(bool value)
    {
        keepHandsHiddenAfterClose = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Continue);
            continueButton.onClick.AddListener(Continue);
        }

        HideImmediate();
    }

    private void Start()
    {
        HideImmediate();
    }


    public void Show()
    {
        ShowUnlock();
    }

    public void ShowUnlock()
    {
        Debug.Log("[JESTER UI] ShowUnlock()");

        if (titleText != null)
        {
            titleText.text = "JESTER UNLOCKED";
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "Two powerful Jester cards have\r\nbeen added to your arsenal.";
        }

        StartCoroutine(Open());
    }


    private IEnumerator Open()
    {
        if (popupRoot == null)
        {
            Debug.LogError(
                "[JESTER UI] Popup Root is NULL!"
            );

            yield break;
        }

        if (popupCanvasGroup == null)
        {
            Debug.LogError(
                "[JESTER UI] Popup Canvas Group is NULL!"
            );

            yield break;
        }

        IsShowing = true;

        transform.SetAsLastSibling();
        SceneTransition transition =
            LevelManager.instance.transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        popupRoot.SetActive(true);

        popupCanvasGroup.alpha = 1f;

        popupCanvasGroup.interactable = true;

        popupCanvasGroup.blocksRaycasts = true;

        LockGameplay();
        yield return transition.AnimateTransitionOut();

        Debug.Log(
            "[JESTER UI] OPENED"
        );
    }

    private void LockGameplay()
    {
        // Khóa + ẩn hand bài thường.
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetInteractable(false);
            HandManager.Instance.SetVisualsSuppressed(true);
        }

        // Khóa + ẩn Jester Hand thật.
        if (JesterHandManager.Instance != null)
        {
            JesterHandManager.Instance
                .SetJesterInteractionLocked(true);

            JesterHandManager.Instance
                .SetVisualsSuppressed(true);
        }
    }

    public void Continue()
    {
        if (!IsShowing)
            return;

        Debug.Log(
            "[JESTER UI] Continue clicked"
        );

        StartCoroutine(Close());
    }

    private IEnumerator Close()
    {
        SceneTransition transition =
            LevelManager.instance.transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;

            popupCanvasGroup.interactable = false;

            popupCanvasGroup.blocksRaycasts = false;
        }

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

        IsShowing = false;

        UnlockGameplay();

        Debug.Log(
            "[JESTER UI] CLOSED"
        );
    }

    private void UnlockGameplay()
    {

        if (keepHandsHiddenAfterClose)
        {
            if (HandManager.Instance != null)
                HandManager.Instance.SetInteractable(false);

            if (JesterHandManager.Instance != null)
                JesterHandManager.Instance.SetJesterInteractionLocked(true);

            return;
        }

        if (HandManager.Instance != null)
        {
            HandManager.Instance.SetVisualsSuppressed(false);
            HandManager.Instance.SetInteractable(true);
        }

        if (JesterHandManager.Instance != null)
        {
            JesterHandManager.Instance.SetVisualsSuppressed(false);
            JesterHandManager.Instance.SetJesterInteractionLocked(false);
            JesterHandManager.Instance.Refresh();
        }
    }

    private void HideImmediate()
    {
        IsShowing = false;

        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;

            popupCanvasGroup.interactable = false;

            popupCanvasGroup.blocksRaycasts = false;
        }

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }
}