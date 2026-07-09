using UnityEngine;
using DG.Tweening;

public class BossFXManager : MonoBehaviour
{
    public static BossFXManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySpawnFX(Transform boss)
    {
        boss.DOKill();

        Vector3 originScale = Vector3.one;

        boss.localScale = Vector3.zero;

        boss.rotation = Quaternion.Euler(0, 0, -180);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            boss.DOScale(originScale * 1.25f, 0.35f)
        );

        seq.Join(
            boss.DORotate(Vector3.zero, 0.45f)
        );

        seq.Append(
            boss.DOScale(originScale, 0.15f)
        );
    }

    public void PlayHitFX(Transform boss)
    {
        boss.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            boss.DOShakePosition(
                0.18f,
                0.2f,
                20,
                90,
                false,
                true
            )
        );

        seq.Join(
            boss.DOScale(0.93f, 0.09f)
        );

        seq.Append(
            boss.DOScale(1f, 0.09f)
        );
    }

    public void PlayDeathFX(Transform boss)
    {
        boss.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            boss.DOScale(1.3f, 0.2f)
        );

        seq.Join(
            boss.DOShakeRotation(0.2f, 20)
        );

        seq.Append(
            boss.DOScale(0, 0.3f)
        );
    }
}