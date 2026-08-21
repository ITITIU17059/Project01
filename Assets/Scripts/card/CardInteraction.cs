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

    private bool isHovered;
    private bool isDragging;

    private HandManager handManager;
    private CardDisplay cardDisplay;
    private CardPhysics cardPhysics;

    public bool isSelectedInCenter = false;

    [HideInInspector]
    public Vector3 splineLocalPosition;

    public bool IsLocked;

    //==================================================
    // JESTER CHECK
    //==================================================

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

    //==================================================
    // UNITY
    //==================================================

    private void Awake()
    {
        originalScale = transform.localScale;

        handManager =
            Object.FindAnyObjectByType<HandManager>();

        cardDisplay =
            GetComponent<CardDisplay>();

        cardPhysics =
            GetComponent<CardPhysics>();
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    //==================================================
    // MOVE
    //==================================================

    public void MoveTo(
        Vector3 localPos,
        Quaternion rotation,
        float duration = 0.25f)
    {
        splineLocalPosition = localPos;

        if (isDragging || isHovered)
            return;

        transform.DOKill();

        transform.DOLocalMove(
            localPos,
            duration
        ).SetEase(Ease.OutCubic);

        transform.DOLocalRotateQuaternion(
            rotation,
            duration
        ).SetEase(Ease.OutCubic);
    }

    //==================================================
    // MOUSE ENTER
    //==================================================

    public void HandleMouseEnter()
    {
        if (IsLocked)
            return;

        // JESTER không cần HandManager để hover.
        if (!IsJesterCard)
        {
            if (handManager == null)
                return;

            if (!handManager.CanInteract)
                return;
        }

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound2D(
                "CardHover"
            );
        }

        if (isDragging)
            return;

        isHovered = true;

        // Đưa card lên trên cùng
        if (cardDisplay != null)
        {
            cardDisplay.SetSortingOrder(300);
        }

        // Nếu đang ở giữa sân thì không nhấc lên
        if (isSelectedInCenter)
            return;

        transform.DOKill();

        transform.DOScale(
            originalScale * hover.scale,
            hover.duration
        ).SetEase(Ease.OutCubic);

        if (IsJesterCard)
        {
            // Jester đứng nguyên vị trí trong Jester Hand.
            // Chỉ phóng to khi hover.
            return;
        }

        transform.DOLocalMove(
            splineLocalPosition +
            Vector3.up * hover.moveAmount,
            hover.duration
        ).SetEase(Ease.OutCubic);
    }

    //==================================================
    // MOUSE EXIT
    //==================================================

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

        //==================================================
        // CARD ĐANG Ở GIỮA
        //==================================================

        if (isSelectedInCenter)
        {
            if (!IsJesterCard &&
                handManager != null)
            {
                handManager.RearrangeSelectedCards();
            }

            return;
        }

        //==================================================
        // CARD ĐANG Ở HAND
        //==================================================

        transform.DOKill();

        transform.DOScale(
            originalScale,
            hover.duration
        ).SetEase(Ease.OutCubic);

        if (!IsJesterCard)
        {
            transform.DOLocalMove(
                splineLocalPosition,
                hover.duration
            ).SetEase(Ease.OutCubic);
        }

        // Jester không thuộc HandManager
        if (!IsJesterCard &&
            handManager != null)
        {
            handManager.RepositionAllCards(null);
        }
    }

    //==================================================
    // DRAG START
    //==================================================

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

    //==================================================
    // DRAGGING
    //==================================================

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

            // Chỉ card thường mới reorder Hand
            if (!IsJesterCard &&
                handManager != null)
            {
                handManager.CheckForCardReorder(gameObject);
            }
        }
    }

    //==================================================
    // DRAG END
    //==================================================

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

        //==================================================
        // CLICK
        //==================================================

        if (isClick)
        {
            ToggleCardSelection();
            return;
        }

        //==================================================
        // KIỂM TRA PLAY ZONE
        //==================================================

        Collider2D hitCollider =
            Physics2D.OverlapPoint(
                mouseWorldPos,
                playZoneLayer
            );

        bool isOverPlayZone =
            hitCollider != null &&
            hitCollider.CompareTag("PlayZone");

        //==================================================
        // CARD ĐANG ĐƯỢC CHỌN
        //==================================================

        if (isSelectedInCenter)
        {
            if (!isOverPlayZone)
            {
                isSelectedInCenter = false;

                if (IsJesterCard)
                {
                    if (JesterHandManager.Instance != null)
                    {
                        JesterHandManager.Instance
                            .DeselectJesterCard(gameObject);
                    }
                }
                else
                {
                    if (handManager != null)
                    {
                        handManager.DeselectCard(gameObject);
                    }
                }
            }
            else
            {
                transform.DORotate(
                    Vector3.zero,
                    0.15f
                );

                if (!IsJesterCard &&
                    handManager != null)
                {
                    handManager.RearrangeSelectedCards();
                }
            }

            return;
        }

        //==================================================
        // CARD CHƯA ĐƯỢC CHỌN
        //==================================================

        if (isOverPlayZone)
        {
            isSelectedInCenter = true;

            transform.DORotate(
                Vector3.zero,
                0.15f
            );

            if (IsJesterCard)
            {
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
        }
        else
        {
            transform.DOKill();

            transform.DOScale(
                originalScale,
                hover.duration
            );

            transform.DOLocalMove(
                splineLocalPosition,
                hover.duration
            );

            if (!IsJesterCard &&
                handManager != null)
            {
                handManager.RepositionAllCards(null);
            }
        }
    }

    //==================================================
    // CLICK SELECTION
    //==================================================

    private void ToggleCardSelection()
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

        transform.DOKill();

        transform.DORotate(
            Vector3.zero,
            0.15f
        );

        isSelectedInCenter =
            !isSelectedInCenter;

        //==================================================
        // JESTER
        //==================================================

        if (IsJesterCard)
        {
            if (JesterHandManager.Instance == null)
                return;

            if (isSelectedInCenter)
            {
                JesterHandManager.Instance
                    .SelectJesterCard(gameObject);
            }
            else
            {
                JesterHandManager.Instance
                    .DeselectJesterCard(gameObject);
            }

            return;
        }

        //==================================================
        // CARD THƯỜNG
        //==================================================

        if (isSelectedInCenter)
        {
            handManager.SelectCard(gameObject);
        }
        else
        {
            handManager.DeselectCard(gameObject);
        }
    }

    //==================================================
    // DESELECT
    //==================================================

    public void HandleDeselect()
    {
        isSelectedInCenter = false;
        isHovered = false;

        transform.DOKill();

        transform.DOScale(
            originalScale,
            hover.duration
        ).SetEase(Ease.OutCubic);

        transform.DOLocalMove(
            splineLocalPosition,
            hover.duration
        ).SetEase(Ease.OutCubic);
    }
}