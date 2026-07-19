using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button confirmButton;

    [Header("Button Animation")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.08f;

    private RectTransform buttonRect;

    private void Start()
    {
        buttonRect = confirmButton.GetComponent<RectTransform>();

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
    }

    private void OnConfirmButtonClicked()
    {
        PlayButtonEffect();

        BattleManager.Instance?.ConfirmPlayCards();
    }

    private void PlayButtonEffect()
    {
        buttonRect.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            buttonRect.DOScale(pressScale, pressDuration)
        );

        seq.Append(
            buttonRect.DOScale(1f, pressDuration)
                     .SetEase(Ease.OutBack)
        );
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }
    }
}