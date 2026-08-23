using TMPro;
using UnityEngine;

public class JesterDescriptionUI : MonoBehaviour
{
    public static JesterDescriptionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Hide();
    }

    public void Show(CardSO card)
    {
        if (card == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        titleText.text = card.cardName;
        descriptionText.text = card.description;
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