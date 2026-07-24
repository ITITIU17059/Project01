using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject emptyState; // hiện khi ô này chưa có reward nào

    
    private InventoryManager owner;
    public RewardSO CurrentReward => currentReward;
    private RewardSO currentReward;
    public void Setup(RewardSO reward)
    {
        currentReward = reward;

        bool isEmpty = reward == null;

        emptyState.SetActive(isEmpty);

        icon.enabled = !isEmpty;

        if (!isEmpty)
            icon.sprite = reward.icon;

        nameText.text =
            isEmpty ? "" : reward.rewardName;
    }
}