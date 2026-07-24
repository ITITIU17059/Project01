using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject equippedHighlight; // viền/khung sáng khi đang equip

    private RewardSO reward;
    private InventoryManager owner;

    public void Setup(
     RewardSO reward,
     InventoryManager owner)
    {
        this.reward = reward;
        this.owner = owner;

        icon.sprite = reward.icon;
        nameText.text = reward.rewardName;

        button.onClick.RemoveAllListeners();
    }

    public void RefreshEquippedVisual()
    {
        if (equippedHighlight == null || reward == null)
            return;

        equippedHighlight.SetActive(PlayerReward.Instance.IsEquipped(reward));
    }
}