using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CardDisplay), typeof(CardPhysics))]
public class CardInteraction : MonoBehaviour
{
    [System.Serializable]
    public struct HoverConfig
    {
        public float scale;
        public float moveAmount;
        public float duration;
    }

    [SerializeField]
    private HoverConfig hover = new HoverConfig
    {
        scale = 1.15f,
        moveAmount = 0.6f,
        duration = 0.15f
    };

    [SerializeField] private LayerMask playZoneLayer;

    private Vector3 originalScale;

    private Vector3 handScale;
    private Vector3 playScale;

    private bool isHovered;
    private bool isDragging;

    private HandManager handManager;
    private CardDisplay cardDisplay;
    private CardPhysics cardPhysics;

    public bool isSelectedInCenter = false;

    [HideInInspector]
    public Vector3 splineLocalPosition;

    public bool IsLocked;

    private bool IsJesterCard
    {
        get
        {
            return cardDisplay != null &&
                   cardDisplay.cardScriptableObject != null &&
                   cardDisplay.cardScriptableObject.type ==
                   CardSO.CardType.Jester;
        }
    }

    private void Awake()
    {
        originalScale = transform.localScale;

        handManager =
            Object.FindAnyObjectByType<HandManager>();

        cardDisplay =
            GetComponent<CardDisplay>();

        cardPhysics =
            GetComponent<CardPhysics>();

        handScale = originalScale;

        playScale = Vector3.one;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void MoveTo(
        Vector3 localPos,
        Quaternion rotation,
        float duration = 0.25f)
    {
        splineLocalPosition = localPos;

        if (isDragging || isHovered)
            return;

        transform.DOKill();

        transform
            .DOLocalMove(localPos, duration)
            .SetEase(Ease.OutCubic);

        transform
            .DOLocalRotateQuaternion(rotation, duration)
            .SetEase(Ease.OutCubic);
    }

    public void HandleMouseEnter()
    {
        if (IsLocked)
            return;

        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound2D("CardHover");
        }

        if (isDragging)
            return;

        isHovered = true;

        if (IsJesterCard)
        {
            ShowJesterDescription();

            if (cardDisplay != null)
            {
                cardDisplay.SetSortingOrder(50);
            }

            transform.DOKill();

            if (isSelectedInCenter)
            {
                transform.localScale = playScale;
            }
            else
            {
   
                transform
                    .DOScale(
                        handScale * hover.scale,
                        hover.duration
                    )
                    .SetEase(Ease.OutCubic);
            }

            return;
        }

        if (cardDisplay != null)
        {
            cardDisplay.SetSortingOrder(300);
        }

        if (isSelectedInCenter)
            return;

        transform.DOKill();

        transform
            .DOScale(
                originalScale * hover.scale,
                hover.duration
            )
            .SetEase(Ease.OutCubic);

        transform
            .DOLocalMove(
                splineLocalPosition +
                Vector3.up * hover.moveAmount,
                hover.duration
            )
            .SetEase(Ease.OutCubic);
    }

    public void HandleMouseExit()
    {
        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        if (!isHovered || isDragging)
            return;

        isHovered = false;

        if (IsJesterCard)
        {
            HideJesterDescription();

            transform.DOKill();

            if (isSelectedInCenter)
            {
                transform.localScale = playScale;
            }
            else
            {
                transform
                    .DOScale(
                        handScale,
                        hover.duration
                    )
                    .SetEase(Ease.OutCubic);
            }

            return;
        }

        if (isSelectedInCenter)
        {
            if (handManager != null)
            {
                handManager.RearrangeSelectedCards();
            }

            return;
        }

        transform.DOKill();

        transform
            .DOScale(
                originalScale,
                hover.duration
            )
            .SetEase(Ease.OutCubic);

        transform
            .DOLocalMove(
                splineLocalPosition,
                hover.duration
            )
            .SetEase(Ease.OutCubic);

        if (handManager != null)
        {
            handManager.RepositionAllCards(null);
        }
    }

    public void HandleDragStart()
    {
        if (IsLocked)
            return;

        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        isDragging = true;

        if (cardPhysics != null)
        {
            cardPhysics.ResetPhysics();
        }
    }

    public void HandleDragging(Vector3 targetWorldPos)
    {
        if (IsLocked)
            return;

        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        transform.position = targetWorldPos;

        if (!isSelectedInCenter)
        {
            if (cardPhysics != null)
            {
                cardPhysics.UpdatePendulumRotation();
            }

            if (!IsJesterCard &&
                handManager != null)
            {
                handManager.CheckForCardReorder(gameObject);
            }
        }
    }

    public void HandleDragEnd(
        bool isClick,
        Vector3 mouseWorldPos)
    {
        if (IsLocked)
            return;

        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        if (!isDragging)
            return;

        isDragging = false;
        isHovered = false;

        if (isClick)
        {
            ToggleCardSelection();
            return;
        }

        Collider2D hitCollider =
            Physics2D.OverlapPoint(
                mouseWorldPos,
                playZoneLayer
            );

        bool isOverPlayZone =
            hitCollider != null &&
            hitCollider.CompareTag("PlayZone");


        if (isSelectedInCenter)
        {

            if (IsJesterCard)
            {
                if (isOverPlayZone)
                {
                    // Vẫn nằm trong Play Zone
                    transform.DORotate(
                        Vector3.zero,
                        0.15f
                    );

                    transform.DOScale(
                        playScale,
                        hover.duration
                    ).SetEase(Ease.OutCubic);

                    return;
                }

                transform.DOKill();

                transform.DORotate(
                    Vector3.zero,
                    0.15f
                );

                transform.DOScale(
                    playScale,
                    hover.duration
                ).SetEase(Ease.OutCubic);

                JesterHandManager.Instance?.ReturnJesterToPlayZone(
                    gameObject
                );

                return;
            }


            if (!isOverPlayZone)
            {
                isSelectedInCenter = false;

                if (handManager != null)
                {
                    handManager.DeselectCard(gameObject);
                }
            }
            else
            {
                transform.DORotate(
                    Vector3.zero,
                    0.15f
                );

                if (handManager != null)
                {
                    handManager.RearrangeSelectedCards();
                }
            }

            return;
        }

        if (isOverPlayZone)
        {
            isSelectedInCenter = true;

            transform.DOKill();

            transform
                .DORotate(
                    Vector3.zero,
                    0.15f
                );

            if (IsJesterCard)
            {
                transform
                    .DOScale(
                        playScale,
                        0.2f
                    )
                    .SetEase(Ease.OutBack);

                if (JesterHandManager.Instance != null)
                {
                    JesterHandManager.Instance
                        .SelectJesterCard(gameObject);
                }
            }
            else
            {
                if (handManager != null)
                {
                    handManager.SelectCard(gameObject);
                }
            }

            return;
        }

        transform.DOKill();

        if (IsJesterCard)
        {
            if (JesterHandManager.Instance != null)
            {
                JesterHandManager.Instance.DeselectJesterCard(gameObject);
            }
            else
            {
                transform
                    .DOScale(
                        handScale,
                        hover.duration
                    )
                    .SetEase(Ease.OutCubic);
            }

            return;
        }

        transform
            .DOScale(
                originalScale,
                hover.duration
            )
            .SetEase(Ease.OutCubic);

        transform
            .DOLocalMove(
                splineLocalPosition,
                hover.duration
            )
            .SetEase(Ease.OutCubic);

        if (handManager != null)
        {
            handManager.RepositionAllCards(null);
        }
    }

    private void ToggleCardSelection()
    {
        if (IsLocked)
            return;

        if (IsJesterCard)
        {
            if (JesterHandManager.Instance == null)
                return;

            if (JesterHandManager.Instance.HasSelectedJester)
            {
                if (JesterHandManager.Instance.SelectedJester
                    == gameObject)
                {
                    isSelectedInCenter = false;

                    JesterHandManager.Instance
                        .DeselectJesterCard(gameObject);
                }

                return;
            }

            bool canSelect =
                JesterManager.Instance != null &&
                JesterManager.Instance.IsUnlocked;

            if (!canSelect)
                return;

            transform.DOKill();

            transform.DORotate(
                Vector3.zero,
                0.15f
            );

            isSelectedInCenter = true;

            transform
                .DOScale(
                    playScale,
                    0.2f
                )
                .SetEase(Ease.OutBack);

            JesterHandManager.Instance
                .SelectJesterCard(gameObject);

            return;
        }

        if (handManager == null)
            return;

        if (!handManager.CanInteract)
            return;

        transform.DOKill();

        transform.DORotate(
            Vector3.zero,
            0.15f
        );

        isSelectedInCenter =
            !isSelectedInCenter;

        if (isSelectedInCenter)
        {
            handManager.SelectCard(gameObject);
        }
        else
        {
            handManager.DeselectCard(gameObject);
        }
    }


    public void HandleDeselect()
    {
        isSelectedInCenter = false;
        isHovered = false;

        transform.DOKill();

        if (IsJesterCard)
        {
            transform
                .DOScale(
                    handScale,
                    hover.duration
                )
                .SetEase(Ease.OutCubic);
        }
        else
        {
            transform
                .DOScale(
                    originalScale,
                    hover.duration
                )
                .SetEase(Ease.OutCubic);
        }

        transform
            .DOLocalMove(
                splineLocalPosition,
                hover.duration
            )
            .SetEase(Ease.OutCubic);
    }


    private void ShowJesterDescription()
    {
        if (!IsJesterCard)
            return;

        if (cardDisplay == null)
            return;

        if (cardDisplay.cardScriptableObject == null)
            return;

        if (JesterDescriptionUI.Instance == null)
            return;

        JesterDescriptionUI.Instance.Show(
            cardDisplay.cardScriptableObject
        );
    }

    private void HideJesterDescription()
    {
        if (!IsJesterCard)
            return;

        if (JesterDescriptionUI.Instance != null)
        {
            JesterDescriptionUI.Instance.Hide();
        }
    }
}