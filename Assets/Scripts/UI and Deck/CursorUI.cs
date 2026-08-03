using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUI : MonoBehaviour
{
    public static CursorUI Instance { get; private set; }
    [SerializeField] private InputActionReference poiterPOsitionAction;
    private RectTransform _cursorTransform;
    private Canvas _parentCanvas;
    private RectTransform _canvasRecTransform;
    private Camera _canvasCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _cursorTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();

        if (_parentCanvas != null)
        {
            _canvasRecTransform = _parentCanvas.GetComponent<RectTransform>();
            _canvasCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null
            : _parentCanvas.worldCamera;
        }
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        poiterPOsitionAction.action.performed += OnPointerPositionChanged;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        poiterPOsitionAction.action.performed -= OnPointerPositionChanged;
    }

    private void OnPointerPositionChanged(InputAction.CallbackContext ctx)
    {
        if (_cursorTransform == null || _canvasRecTransform == null) return;

        var mousePosition = ctx.ReadValue<Vector2>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRecTransform, mousePosition, _canvasCamera, out var localPoint
        ))
        {
            _cursorTransform.anchoredPosition = localPoint;
        }
    }
}
