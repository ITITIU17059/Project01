using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardFXManager : MonoBehaviour
{
    public static CardFXManager Instance { get; private set; }

    [Header("Visual Prefabs")]
    [SerializeField] private GameObject visualCardPrefab; // Kéo Prefab lá bài chuẩn của bạn vào đây
    [SerializeField] private Transform canvasTransform;     // Kéo Object Canvas vào đây để bài UI không bị lệch layer

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Hiệu ứng bay bài từ Mộ (Graveyard) về Bộ bài rút (Tavern)
    /// </summary>
    public void PlayHealDeckFX(List<CardSO> healedCards, Transform graveyardSpawn, Transform tavernSpawn)
    {
        if (graveyardSpawn == null || tavernSpawn == null)
        {
            Debug.LogError("Thiếu tham chiếu SpawnPoint cho hiệu ứng bài bay!");
            return;
        }

        StartCoroutine(HealDeckRoutine(healedCards, graveyardSpawn.position, tavernSpawn.position, tavernSpawn));
    }

    private IEnumerator HealDeckRoutine(List<CardSO> healedCards, Vector3 startPos, Vector3 targetPos, Transform tavernTransform)
    {
        Vector3 absoluteOriginalScale = Vector3.one;
        Transform visualTarget = null;

        if (tavernTransform != null && tavernTransform.parent != null)
        {
            visualTarget = tavernTransform.parent.Find("CardBack");
            if (visualTarget != null)
            {
                absoluteOriginalScale = visualTarget.localScale;
            }
        }

        foreach (CardSO cardData in healedCards)
        {
            GameObject ghostCard = CreateGhostCard(cardData, startPos, Vector3.one, 500);

            Sequence cardSequence = DOTween.Sequence();

            cardSequence.Append(ghostCard.transform.DOMove(targetPos, 0.6f).SetEase(Ease.OutQuad));
            cardSequence.Join(ghostCard.transform.DORotate(new Vector3(0, 0, Random.Range(-45f, 45f)), 0.6f));
            cardSequence.Join(ghostCard.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InQuad));

            cardSequence.OnComplete(() =>
            {
                Destroy(ghostCard);

                if (visualTarget != null)
                {
                    visualTarget.DOKill();
                    visualTarget.localScale = absoluteOriginalScale;

                    Sequence punchSeq = DOTween.Sequence();

                    punchSeq.Append(visualTarget.DOScale(absoluteOriginalScale * 1.15f, 0.06f).SetEase(Ease.OutQuad));
                    punchSeq.Append(visualTarget.DOScale(absoluteOriginalScale, 0.12f).SetEase(Ease.InQuad));
                }
            });

            yield return new WaitForSeconds(0.15f);
        }
    }

    private GameObject CreateGhostCard(CardSO cardData, Vector3 pos, Vector3 scale, int sortingOrder)
    {
        GameObject ghost;

        if (visualCardPrefab != null)
        {
            // Sinh bài trực tiếp bên dưới Canvas để không bị lỗi layer UI
            ghost = Instantiate(visualCardPrefab, pos, Quaternion.identity, canvasTransform);
            ghost.transform.localScale = scale;
        }
        else
        {
            // Phương án dự phòng nếu chưa kéo Prefab vào ô cấu hình
            ghost = new GameObject("GhostCard_FX");
            ghost.transform.SetParent(canvasTransform, false);
            ghost.transform.position = pos;
            ghost.transform.localScale = scale;

            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = cardData.cardSprite;
            sr.sortingOrder = sortingOrder;
        }

        // Vô hiệu hóa tính tương tác chuột của lá bài ảo này để tránh việc người chơi cố tình click khi đang bay
        if (ghost.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        if (ghost.TryGetComponent<MonoBehaviour>(out var comp) && comp.GetType().Name.Contains("Input"))
        {
            Destroy(comp);
        }

        return ghost;
    }

    public void PlayAnimateToGraveyardFX(GameObject cardObject, Transform graveyardSpawn)
    {
        if (cardObject == null) return;

        cardObject.transform.DOKill();

        if (cardObject.TryGetComponent<Collider2D>(out var col)) col.enabled = false;

        Vector3 targetPos = graveyardSpawn != null ? graveyardSpawn.position : Vector3.zero;
        Sequence discardSequence = DOTween.Sequence();

        discardSequence.Append(cardObject.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutQuad));

        float randomTilt = Random.Range(-35f, 35f);

        discardSequence.Join(cardObject.transform.DORotate(new Vector3(0, 0, randomTilt), 0.5f).SetEase(Ease.OutCubic));
        discardSequence.Join(cardObject.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuad));

        discardSequence.OnComplete(() =>
        {
            Destroy(cardObject);

            if (graveyardSpawn != null && graveyardSpawn.parent != null)
            {
                Transform visualTarget = graveyardSpawn.parent.Find("CardBack");

                if (visualTarget != null)
                {
                    visualTarget.DOKill();

                    Vector3 originalCardBackScale = visualTarget.localScale;
                    Sequence punchSeq = DOTween.Sequence();

                    punchSeq.Append(visualTarget.DOScale(originalCardBackScale * 1.2f, 0.08f).SetEase(Ease.OutQuad));
                    punchSeq.Append(visualTarget.DOScale(originalCardBackScale, 0.12f).SetEase(Ease.InQuad));
                }
            }
        });
    }
}