using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform tooltipPanel;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(RewardSO reward)
    {
        if (reward == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        titleText.text =
            reward.rewardName;

        descriptionText.text =
            reward.description;

        tooltipPanel.anchoredPosition =
            new Vector2(41, 38);
    }

    public void Show(CardSO card)
    {
        if (card == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        titleText.text =
            card.cardName;

        descriptionText.text =
            card.description;

        tooltipPanel.anchoredPosition =
            new Vector2(41, 38);
    }

    public void Hide()
    {
        if (titleText != null)
            titleText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        gameObject.SetActive(false);
    }
}