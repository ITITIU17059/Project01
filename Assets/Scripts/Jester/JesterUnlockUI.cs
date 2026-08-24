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
            continueButton.onClick.RemoveListener(Hide);
            continueButton.onClick.AddListener(Hide);
        }

        HideImmediate();

        Debug.Log("[JESTER UI] Initialized.");
    }

    //==================================================
    // SHOW FIRST UNLOCK
    //==================================================

    public void Show()
    {
        ShowUnlock();
    }

    public void ShowUnlock()
    {
        Debug.Log("[JESTER UI] SHOW UNLOCK");

        if (titleText != null)
            titleText.text = "JESTER UNLOCKED";

        if (descriptionText != null)
        {
            descriptionText.text =
                "You received 1 Reset Jester\n" +
                "and 1 Instant Kill Jester.";
        }

        if (resetJester != null)
            resetJester.SetActive(true);

        if (instantKillJester != null)
            instantKillJester.SetActive(true);

        ShowPopup();
    }

    //==================================================
    // SHOW RANK REWARD
    //==================================================

    public void ShowRankReward()
    {
        Debug.Log("[JESTER UI] SHOW RANK REWARD");

        if (titleText != null)
            titleText.text = "JESTER CHARGES +1";

        if (descriptionText != null)
        {
            descriptionText.text =
                "Reset Jester   +1\n" +
                "Instant Kill   +1";
        }

        if (resetJester != null)
            resetJester.SetActive(true);

        if (instantKillJester != null)
            instantKillJester.SetActive(true);

        ShowPopup();
    }

    //==================================================
    // SHOW
    //==================================================

    private void ShowPopup()
    {
        if (canvasGroup == null)
        {
            Debug.LogError(
                "[JESTER UI] CanvasGroup is NULL!"
            );

            return;
        }

        if (content != null)
            content.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        transform.SetAsLastSibling();

        Debug.Log("[JESTER UI] POPUP VISIBLE");
    }

    //==================================================
    // HIDE
    //==================================================

    public void Hide()
    {
        Debug.Log("[JESTER UI] HIDE");

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (content != null)
            content.SetActive(false);
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
}