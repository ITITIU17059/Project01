using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardInfoUI : MonoBehaviour
{
    public static RewardInfoUI Instance;

    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text description;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Sprite sprite, string text)
    {
        icon.sprite = sprite;
        description.text = text;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        icon.sprite = null;
        description.text = null;


        gameObject.SetActive(false);
    }
}