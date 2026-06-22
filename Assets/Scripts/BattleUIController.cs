using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button confirmButton;

    void Start()
    {
        if (confirmButton != null)
        {
            // Lắng nghe sự kiện Click chuột vào nút
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
    }

    private void OnConfirmButtonClicked()
    {
        // Gọi hàm xác nhận đánh bài từ BattleManager đã sửa đổi ở lượt trước
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ConfirmPlayCards();
        }
        else
        {
            Debug.LogError("Không tìm thấy BattleManager Instance trong Scene!");
        }
    }

    void OnDestroy()
    {
        // Hủy lắng nghe sự kiện để tránh rò rỉ bộ nhớ (Memory Leak)
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }
    }
}