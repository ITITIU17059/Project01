using UnityEngine;

[RequireComponent(typeof(CardInteraction))]
public class CardInputHandler : MonoBehaviour
{
    private Camera mainCamera;
    private CardInteraction interaction;
    private Vector3 clickStartMousePos;

    void Awake()
    {
        mainCamera = Camera.main;
        interaction = GetComponent<CardInteraction>();
    }

    // Unity tự động gọi khi chuột đi vào Collider của chính Object này
    private void OnMouseEnter()
    {
        interaction.HandleMouseEnter();
    }

    // Unity tự động gọi khi chuột rời khỏi Collider của chính Object này
    private void OnMouseExit()
    {
        interaction.HandleMouseExit();
    }

    // Unity tự động gọi khi nhấn click chuột xuống Collider này
    private void OnMouseDown()
    {
        clickStartMousePos = Input.mousePosition;
        interaction.HandleDragStart();
    }

    // Unity tự động gọi mỗi frame khi đang giữ và kéo chuột
    private void OnMouseDrag()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        interaction.HandleDragging(new Vector3(mousePos.x, mousePos.y, 0));
    }

    // Unity tự động gọi khi thả chuột ra khỏi Collider
    private void OnMouseUp()
    {
        float mouseMoveDistance = Vector3.Distance(clickStartMousePos, Input.mousePosition);
        bool isClick = mouseMoveDistance < 5f;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        interaction.HandleDragEnd(isClick, mouseWorldPos);
    }
}