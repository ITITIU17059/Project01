using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Transform titlePosition;
    [SerializeField] private GameObject pressStart;

    [SerializeField] private Image pressStartImg;
    [SerializeField] private GameObject pressZone;

    private void Start()
    {
        MusicManager.instance.PlayMusic("IntroTheme2");
        pressStart.SetActive(false);
        MoveTitle();
    }

    private void MoveTitle()
    {
        transform.DOMoveX(titlePosition.position.x, 1f)
            .SetEase(Ease.OutBounce)
            .OnComplete(ShowPressStart);
    }

    private void ShowPressStart()
    {
        pressStart.SetActive(true);

        Color c = pressStartImg.color;
        c.a = 1;
        pressStartImg.color = c;

        Sequence seq = DOTween.Sequence();

        seq.Append(pressStartImg.DOFade(0f, 1.2f));
        seq.Join(pressStart.transform.DOScale(1.05f, 1.2f));

        seq.SetLoops(-1, LoopType.Yoyo);
    }

    public void PressZoneButton()
    {
        LevelManager.instance.LoadMainMenu();
    }
}