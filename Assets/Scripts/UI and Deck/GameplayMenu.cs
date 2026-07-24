using DG.Tweening;
using UnityEngine;

public class GameplayMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPause;

    public void Open()
    {
        isPause = true;

        Time.timeScale = 0f;

        pausePanel.transform.localScale = Vector3.zero;

        pausePanel.SetActive(true);

        pausePanel.transform
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack);
    }

    public void Resume()
    {
        isPause = false;

        Time.timeScale = 1f;

        pausePanel.transform
            .DOScale(0f, 0.2f)
            .OnComplete(() =>
        {
            pausePanel.SetActive(false);
        });
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        SaveManager.Instance.SaveProgress(
            BossManager.Instance.CurrentStageIndex,
            BossManager.Instance.CurrentBossIndex);

        LevelManager.instance.LoadMainMenu();
    }
}