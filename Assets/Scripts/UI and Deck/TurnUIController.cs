using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TurnUIController : MonoBehaviour
{
    public static TurnUIController Instance;

    [Header("UI")]
    [SerializeField] private RectTransform banner;
    [SerializeField] private RectTransform turn;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image bannerImage;
    [SerializeField] private Image turnImage;

    [Header("Sprites")]
    [SerializeField] private Sprite yourTurn;
    [SerializeField] private Sprite enemyTurn;
    [SerializeField] private Sprite discardTurn;

    [SerializeField] private Sprite yourTurnControl;
    [SerializeField] private Sprite enemyTurnControl;
    [SerializeField] private Sprite discardTurnControl;

    private Sequence currentSequence;

    private const float StartX = -1600f;
    private const float CenterX = 0f;
    private const float EndX = 1600f;

    [Header("Banner")]
    [SerializeField] private Image victoryImage;
    [SerializeField] private Image defeatImage;

    void Awake()
    {
        Instance = this;

        victoryImage.gameObject.SetActive(false);
        defeatImage.gameObject.SetActive(false);
    }

    public void ShowYourTurn()
    {
        Show(yourTurn, "YourTurn");
        ShowTurn(yourTurnControl);
    }

    public void ShowEnemyTurn()
    {
        Show(enemyTurn, "EnemyTurn");
        ShowTurn(enemyTurnControl);
    }

    public void ShowDiscardTurn()
    {
        Show(discardTurn, "DiscardTurn");
        ShowTurn(discardTurnControl);
    }

    public IEnumerator ShowVictory()
    {
        Image banner = victoryImage;

        RectTransform rect = banner.rectTransform;
        CanvasGroup group = banner.GetComponent<CanvasGroup>();

        banner.gameObject.SetActive(true);

        rect.localScale = Vector3.one * 0.5f;
        rect.anchoredPosition = new Vector2(0, 700);

        group.alpha = 0;

        Sequence seq = DOTween.Sequence();

        seq.Append(group.DOFade(1, 0.25f));

        seq.Join(
            rect.DOAnchorPos(Vector2.zero, 0.45f)
                .SetEase(Ease.OutBack)
        );

        seq.Join(
            rect.DOScale(1.15f, 0.35f)
        );

        seq.Append(
            rect.DOScale(1f, 0.15f)
        );

        seq.Append(
            rect.DOShakeScale(
                0.25f,
                0.06f
            )
        );

        yield return seq.WaitForCompletion();

        yield return new WaitForSeconds(1.2f);

        Sequence end = DOTween.Sequence();

        end.Join(group.DOFade(0, 0.35f));

        end.Join(
            rect.DOAnchorPos(
                new Vector2(0, -600),
                0.35f
            )
        );

        yield return end.WaitForCompletion();

        banner.gameObject.SetActive(false);
    }

    public IEnumerator ShowDefeat()
    {
        Image banner = defeatImage;

        RectTransform rect = banner.rectTransform;
        CanvasGroup group = banner.GetComponent<CanvasGroup>();

        banner.gameObject.SetActive(true);

        rect.localScale = Vector3.one * 1.4f;
        rect.anchoredPosition = new Vector2(0, 700);

        group.alpha = 0;

        Sequence seq = DOTween.Sequence();

        seq.Append(group.DOFade(1, 0.15f));

        seq.Join(
            rect.DOAnchorPos(Vector2.zero, 0.35f)
                .SetEase(Ease.OutBounce)
        );

        seq.Join(
            rect.DOScale(1f, 0.35f)
        );

        seq.Append(
            rect.DOShakePosition(
                0.4f,
                18,
                30
            )
        );

        yield return seq.WaitForCompletion();

        yield return new WaitForSeconds(1f);

        Sequence end = DOTween.Sequence();

        end.Join(group.DOFade(0, 0.35f));

        end.Join(
            rect.DOScale(0.8f, 0.35f)
        );

        yield return end.WaitForCompletion();

        banner.gameObject.SetActive(false);
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

    private void ShowTurn(Sprite sprite)
    {
        if (turnImage.sprite == null)
        {
            turnImage.sprite = yourTurnControl;
            return;
        }

        Sequence currentSequence = DOTween.Sequence();

        currentSequence.Append(turn.DOScale(new Vector2(0f, 0f), 0.4f).SetEase(Ease.InOutSine));
        turnImage.sprite = sprite;
        currentSequence.Append(turn.DOScale(new Vector2(1f, 1f), 0.4f).SetEase(Ease.InOutSine));
    }
}