using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BossFXManager : MonoBehaviour
{
    public static BossFXManager Instance { get; private set; }

    [SerializeField] private GameObject heartVFX;
    [SerializeField] private GameObject diamondVFX;
    [SerializeField] private GameObject clubVFX;
    [SerializeField] private GameObject spadeVFX;
    [SerializeField] private GameObject bossAttackVFXPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private string GetAttackSound(string suit)
    {
        switch (suit)
        {
            case "Hearts":
                return "HeartAttack";

            case "Diamonds":
                return "DiamondAttack";

            case "Clubs":
                return "ClubAttack";

            case "Spades":
                return "SpadeAttack";
        }

        return null;
    }

    public IEnumerator PlayCardSuitFX(string suit, Transform cardTransform)
    {
        GameObject prefab = null;

        switch (suit)
        {
            case "Hearts":
                prefab = heartVFX;
                break;

            case "Diamonds":
                prefab = diamondVFX;
                break;

            case "Spades":
                prefab = spadeVFX;
                break;

            case "Clubs":
                prefab = clubVFX;
                break;
        }

        if (prefab == null)
            yield break;

        GameObject fx = Instantiate(
            prefab,
            cardTransform.position,
            Quaternion.identity);


        Transform boss = BossManager.Instance.BossTransform;

        fx.transform.DOMove(
            boss.position,
            0.35f)
            .SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.35f);

        Destroy(fx);

        string soundID = GetAttackSound(suit);

        if (!string.IsNullOrEmpty(soundID))
        {
            SoundManager.instance?.PlaySound2D(soundID);
        }

        PlayHitFX(boss);
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

    public IEnumerator PlayBossAttackFX()
    {
        BossSO boss = BossManager.Instance.CurrentBoss;

        if (boss.attackVFX == null)
            yield break;

        GameObject fx = Instantiate(
            boss.attackVFX,
            BossManager.Instance.BossTransform.position,
            Quaternion.identity);

        SoundManager.instance?.PlaySound2D(
            boss.attackSoundID);

        yield return fx.transform
            .DOMove(
                BattleManager.Instance.PlayerHitPoint.position,
                boss.attackFlyTime)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        Destroy(fx);

        if (boss.hitVFX != null)
        {
            Instantiate(
                boss.hitVFX,
                BattleManager.Instance.PlayerHitPoint.position,
                Quaternion.identity);
        }

        PlayPlayerHitFX(
            BattleManager.Instance.PlayerHitPoint);

        // Đợi Hit VFX chạy xong
        yield return new WaitForSeconds(0.7f);
    }

    private void PlayPlayerHitFX(Transform player)
    {
        player.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            player.DOShakePosition(
                0.18f,
                0.15f));

        seq.Join(
            player.DOScale(
                0.92f,
                0.08f));

        seq.Append(
            player.DOScale(
                1f,
                0.08f));
    }
}