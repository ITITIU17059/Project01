using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JesterUnlockUI : MonoBehaviour
{
    public static JesterUnlockUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject content;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private GameObject resetJester;
    [SerializeField] private GameObject instantKillJester;

    [SerializeField] private Button continueButton;

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
            continueButton.onClick.AddListener(Hide);
        }

        HideImmediate();
    }
    public void Show()
    {
        ShowUnlock();
    }
    public void ShowUnlock()
    {
        Debug.Log("[JESTER UI] ShowUnlock called.");

        if (resetJester != null)
            resetJester.SetActive(true);

        if (instantKillJester != null)
            instantKillJester.SetActive(true);

        if (titleText != null)
            titleText.text = "JESTER UNLOCKED";

        if (descriptionText != null)
        {
            descriptionText.text =
                "You received 1 Reset Jester\n" +
                "and 1 Instant Kill Jester.";
        }

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (content != null)
            content.SetActive(true);
    }

    public void ShowRankReward()
    {
        Debug.Log("[JESTER UI] ShowRankReward called.");

        if (resetJester != null)
            resetJester.SetActive(true);

        if (instantKillJester != null)
            instantKillJester.SetActive(true);

        if (titleText != null)
            titleText.text = "JESTER CHARGES +1";

        if (descriptionText != null)
        {
            descriptionText.text =
                "Reset Jester +1\n" +
                "Instant Kill Jester +1";
        }

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (content != null)
            content.SetActive(true);
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (content != null)
            content.SetActive(false);

        gameObject.SetActive(false);
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (content != null)
            content.SetActive(false);
    }

    // TEST
    [ContextMenu("TEST Jester Popup")]
    private void TestPopup()
    {
        ShowUnlock();
    }
}