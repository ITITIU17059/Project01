using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardInfoUI : MonoBehaviour
{
    public static RewardInfoUI Instance;

    [SerializeField] private TMP_Text description;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        description.text = text;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        description.text = null;


        gameObject.SetActive(false);
    }
}