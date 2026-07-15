using UnityEngine;
using UnityEngine.UI;

public class DeckBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    [SerializeField] private Gradient gradient;

    private int maxCards;

    public void Init(int max)
    {
        maxCards = max;
        UpdateBar(max);
    }

    public void UpdateBar(int current)
    {
        float ratio = maxCards <= 0
    ? 0
    : (float)current / maxCards;

        ratio = Mathf.Clamp01(ratio);

        fillImage.fillAmount = ratio;

        fillImage.color = gradient.Evaluate(ratio);
    }
}