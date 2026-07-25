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
        gameObject.SetActive(true);

        titleText.text = reward.rewardName;
        descriptionText.text = reward.description;
        tooltipPanel.anchoredPosition = new Vector2(41, 38);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector2 offset = new Vector2(25f, -25f);

    }
}