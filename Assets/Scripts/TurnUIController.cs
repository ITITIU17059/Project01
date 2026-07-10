using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TurnUIController : MonoBehaviour
{
    public static TurnUIController Instance;

    [Header("UI")]
    [SerializeField] private RectTransform banner;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image bannerImage;

    [Header("Sprites")]
    [SerializeField] private Sprite yourTurn;
    [SerializeField] private Sprite enemyTurn;
    [SerializeField] private Sprite discardTurn;

    private Sequence currentSequence;

    private const float StartX = -1600f;
    private const float CenterX = 0f;
    private const float EndX = 1600f;

    void Awake()
    {
        Instance = this;
    }

    public void ShowYourTurn()
    {
        Show(yourTurn, "YourTurn");
    }

    public void ShowEnemyTurn()
    {
        Show(enemyTurn, "EnemyTurn");
    }

    public void ShowDiscardTurn()
    {
        Show(discardTurn, "DiscardTurn");
    }

    private void Show(Sprite sprite, string soundId)
    {
        bannerImage.enabled = true;
        currentSequence?.Kill();

        bannerImage.sprite = sprite;

        banner.anchoredPosition = new Vector2(StartX, 0);

        banner.localScale = Vector3.one * 0.8f;

        canvasGroup.alpha = 1f;

        SoundManager.instance?.PlaySound2D(soundId);

        currentSequence = DOTween.Sequence();

        // Trượt vào
        currentSequence.Append(
            banner.DOAnchorPosX(CenterX, 0.4f)
                  .SetEase(Ease.OutCubic));

        // Nảy nhẹ
        currentSequence.Join(
            banner.DOScale(1.08f, 0.25f)
                  .SetEase(Ease.OutBack));

        currentSequence.Append(
            banner.DOScale(1f, 0.1f));

        // Dừng giữa màn hình
        currentSequence.AppendInterval(0.8f);

        // Trượt ra
        currentSequence.Append(
            banner.DOAnchorPosX(EndX, 0.35f)
                  .SetEase(Ease.InCubic));

        currentSequence.Join(
            canvasGroup.DOFade(0, 0.35f));
    }
}