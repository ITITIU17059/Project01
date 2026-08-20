using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CardDisplay), typeof(CardPhysics))]
public class CardInteraction : MonoBehaviour
{
    [System.Serializable]
    public struct HoverConfig { public float scale; public float moveAmount; public float duration; }
    [SerializeField] private HoverConfig hover = new() { scale = 1.15f, moveAmount = 0.6f, duration = 0.15f };
    [SerializeField] private LayerMask playZoneLayer;

    private Vector3 originalScale;
    private bool isHovered, isDragging;
    private HandManager handManager;
    private CardDisplay cardDisplay;
    private CardPhysics cardPhysics;

    public bool isSelectedInCenter = false;
    [HideInInspector] public Vector3 splineLocalPosition;
    public bool IsLocked;
    private void Awake()
    {
        originalScale = transform.localScale;
        handManager = Object.FindAnyObjectByType<HandManager>();
        cardDisplay = GetComponent<CardDisplay>();
        cardPhysics = GetComponent<CardPhysics>();
    }
    private void OnDestroy()
    {
        transform.DOKill();
    }
    public void MoveTo(Vector3 localPos, Quaternion rotation, float duration = 0.25f)
    {
        splineLocalPosition = localPos;

        if (isDragging || isHovered)
            return;

        transform.DOKill();

        transform.DOLocalMove(localPos, duration)
                 .SetEase(Ease.OutCubic);

        transform.DOLocalRotateQuaternion(rotation, duration)
                 .SetEase(Ease.OutCubic);
    }

    public void HandleMouseEnter()
    {

        if (IsLocked)
            return;
        if (!handManager.CanInteract)
            return;
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySound2D("CardHover");
        }

        if (isDragging) return; // Đang kéo thì bỏ qua
        isHovered = true;

        // Đẩy layer hiển thị lên rất cao để Collider của nó luôn nằm trên cùng, dễ Click
        if (cardDisplay)
        {
            cardDisplay.SetSortingOrder(300); // Tăng hẳn lên 300 để đè lên mâm giữa (100+)
        }

        // Nếu bài đang ở giữa bàn đấu thì KHÔNG nhấc vị trí lên (chỉ làm nổi layer để dễ click)
        if (isSelectedInCenter) return;

        // Nếu bài ở dưới tay thì mới phóng to nhẹ và nhấc lên như cũ
        transform.DOKill();
        transform.DOScale(originalScale * hover.scale, hover.duration).SetEase(Ease.OutCubic);
        transform.DOLocalMove(splineLocalPosition + Vector3.up * hover.moveAmount, hover.duration).SetEase(Ease.OutCubic);
    }

    public void HandleMouseExit()
    {
        if (!handManager.CanInteract)
            return;
        if (!isHovered || isDragging) return;
        isHovered = false;

        if (isSelectedInCenter)
        {
            // Nếu bài ở giữa bàn, gọi HandManager tính toán trả lại layer mâm giữa chuẩn (100 + i)
            if (handManager) handManager.RearrangeSelectedCards();
            return;
        }

        // Nếu bài dưới tay thì thu nhỏ và trả về vị trí cũ bình thường
        transform.DOKill();
        transform.DOScale(originalScale, hover.duration).SetEase(Ease.OutCubic);
        transform.DOLocalMove(splineLocalPosition, hover.duration).SetEase(Ease.OutCubic);

        if (handManager) handManager.RepositionAllCards(null);
    }

    public void HandleDragStart()
    {
        if (IsLocked)
            return;
        if (!handManager.CanInteract)
            return;
        isDragging = true;
        cardPhysics.ResetPhysics();
    }

    public void HandleDragging(Vector3 targetWorldPos)
    {
        if (IsLocked)
            return;
        if (!handManager.CanInteract)
            return;
        transform.position = targetWorldPos;

        if (!isSelectedInCenter)
        {
            cardPhysics.UpdatePendulumRotation();
            if (handManager) handManager.CheckForCardReorder(gameObject);
        }
    }

    public void HandleDragEnd(bool isClick, Vector3 mouseWorldPos)
    {
        if (IsLocked)
            return;
        if (!handManager.CanInteract)
            return;
        if (!isDragging) return;
        isDragging = false;
        isHovered = false;

        if (isClick)
        {
            ToggleCardSelection();
            return;
        }

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos, playZoneLayer);
        bool isOverPlayZone = (hitCollider != null && hitCollider.CompareTag("PlayZone"));

        if (isSelectedInCenter)
        {
            if (!isOverPlayZone)
            {
                isSelectedInCenter = false;
                if (handManager) handManager.DeselectCard(gameObject);
            }
            else
            {
                transform.DORotate(Vector3.zero, 0.15f);
                if (handManager) handManager.RearrangeSelectedCards();
            }
        }
        else
        {
            if (isOverPlayZone)
            {
                isSelectedInCenter = true;
                transform.DORotate(Vector3.zero, 0.15f);
                if (handManager) handManager.SelectCard(gameObject);
            }
            else
            {
                transform.DOKill();
                transform.DOScale(originalScale, hover.duration);
                transform.DOLocalMove(splineLocalPosition, hover.duration);
                if (handManager) handManager.RepositionAllCards(null);
            }
        }
    }

    private void ToggleCardSelection()
    {
        if (IsLocked)
            return;
        if (!handManager.CanInteract)
            return;
        if (!handManager) return;
        transform.DOKill();
        transform.DORotate(Vector3.zero, 0.15f);

        isSelectedInCenter = !isSelectedInCenter;
        if (isSelectedInCenter) handManager.SelectCard(gameObject);
        else handManager.DeselectCard(gameObject);
    }

    public void HandleDeselect()
    {
        isSelectedInCenter = false;
        isHovered = false;

        // Diệt các Tween Scale cũ đang lỗi và co về kích thước gốc chuẩn của Prefab
        transform.DOKill();
        transform.DOScale(originalScale, hover.duration).SetEase(Ease.OutCubic);
        transform.DOLocalMove(splineLocalPosition, hover.duration).SetEase(Ease.OutCubic);
    }
}