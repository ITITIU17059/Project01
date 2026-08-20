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
    [SerializeField] private GameObject blockSuccessVFX;
    [SerializeField] private GameObject blockFailVFX;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private string GetAttackSound(CardSO.Suit suit)
    {
        switch (suit)
        {
            case CardSO.Suit.Hearts:
                return "HeartAttack";

            case CardSO.Suit.Diamonds:
                return "DiamondAttack";

            case CardSO.Suit.Clubs:
                return "ClubAttack";

            case CardSO.Suit.Spades:
                return "SpadeAttack";
        }

        return null;
    }

    public IEnumerator PlayCardSuitFX(
     CardSO.Suit suit,
     Transform cardTransform)
    {
        GameObject prefab = null;

        switch (suit)
        {
            case CardSO.Suit.Hearts:
                prefab = heartVFX;
                break;

            case CardSO.Suit.Diamonds:
                prefab = diamondVFX;
                break;

            case CardSO.Suit.Clubs:
                prefab = clubVFX;
                break;

            case CardSO.Suit.Spades:
                prefab = spadeVFX;
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
            0.6f)
            .SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.6f);

        Destroy(fx);

        string soundID = GetAttackSound(suit);

        if (!string.IsNullOrEmpty(soundID))
            SoundManager.instance?.PlaySound2D(soundID);

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

    public IEnumerator PlayDeathFX(Transform boss)
    {
        boss.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            boss.DOScale(1.3f, 0.2f));

        seq.Join(
            boss.DOShakeRotation(0.2f, 20));

        seq.Append(
            boss.DOScale(0, 0.3f));

        yield return seq.WaitForCompletion();
    }

    public IEnumerator PlayBossAttackFX()
    {
        BossSO boss = BossManager.Instance.CurrentBoss;

        if (boss.attackVFX == null)
            yield break;

        GameObject fx = Instantiate(
            boss.attackVFX,
            BossManager.Instance.BossTransform.position,
            boss.attackVFX.transform.rotation);

        SoundManager.instance?.PlaySound2D(
            boss.attackSoundID);

        yield return fx.transform
            .DOMove(
                BattleManager.Instance.PlayerHitPoint.position,
                boss.attackFlyTime)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        PlayPlayerHitFX(BattleManager.Instance.PlayerHitPoint);

        Animator animator = fx.GetComponent<Animator>();

        float duration = 0.6f;

        if (animator != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                duration = Mathf.Max(duration, clip.length);
            }
        }

        yield return new WaitForSeconds(duration);

        Destroy(fx);
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

    public IEnumerator PlayCollectRewardFX(
    BossDisplay bossDisplay,
    CardSO rewardCard,
    Transform target)
    {
        //---------------------------------
        // Clone artwork
        //---------------------------------

        SpriteRenderer original = bossDisplay.Artwork;

        GameObject reward =
            Instantiate(
                original.gameObject,
                original.transform.position,
                original.transform.rotation);

        SpriteRenderer render =
            reward.GetComponent<SpriteRenderer>();

        render.sprite = rewardCard.cardSprite;
        render.sortingLayerName = "UI";
        render.sortingOrder = 999;

        reward.transform.localScale =
            original.transform.lossyScale;

        //---------------------------------
        // Path
        //---------------------------------

        Vector3 start = reward.transform.position;

        Vector3 end = target.position;

        Vector3 mid =
            (start + end) * 0.5f +
            Vector3.up * 1.3f;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            reward.transform
            .DOPath(
                new[]
                {
                start,
                mid,
                end
                },
                0.8f,
                PathType.CatmullRom)
            .SetEase(Ease.InOutQuad));

        seq.Join(
            reward.transform
            .DOScale(
                target.lossyScale,
                0.8f));

        seq.Join(
            reward.transform
            .DORotate(
                new Vector3(0, 0, 360),
                0.8f,
                RotateMode.FastBeyond360));

        yield return seq.WaitForCompletion();

        target.DOPunchScale(
            Vector3.one * 0.12f,
            0.2f);

        Destroy(reward);
    }
    public IEnumerator PlayBlockSuccessFX()
    {
        Transform player =
            BattleManager.Instance.PlayerHitPoint;

        SoundManager.instance?.PlaySound2D("BlockSuccess");

        yield return PlayAnimatorFX(
            blockSuccessVFX,
            player.position,
            Quaternion.identity);

        player.DOPunchScale(
            Vector3.one * 0.08f,
            0.2f);
    }

    public IEnumerator PlayBlockFailFX()
    {
        Transform player =
            BattleManager.Instance.PlayerHitPoint;

        SoundManager.instance?.PlaySound2D("BlockFail");

        yield return PlayAnimatorFX(
            blockFailVFX,
            player.position,
            Quaternion.identity);

        player.DOShakePosition(
            0.3f,
            0.2f);
    }

    private IEnumerator PlayAnimatorFX(
    GameObject prefab,
    Vector3 position,
    Quaternion rotation)
    {
        if (prefab == null)
            yield break;

        GameObject fx = Instantiate(
            prefab,
            position,
            rotation);

        float duration = GetAnimatorDuration(fx);

        yield return new WaitForSeconds(duration);

        Destroy(fx);
    }

    private float GetAnimatorDuration(GameObject obj)
    {
        Animator animator = obj.GetComponent<Animator>();

        if (animator == null ||
            animator.runtimeAnimatorController == null)
        {
            return 0.5f;
        }

        float duration = 0f;

        foreach (AnimationClip clip
            in animator.runtimeAnimatorController.animationClips)
        {
            duration = Mathf.Max(duration, clip.length);
        }

        return duration;
    }
}