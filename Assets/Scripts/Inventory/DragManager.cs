using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance;

    [SerializeField] private Image dragIcon;

    private RewardSO draggingReward;
    private int draggingEquipSlot = -1;

    public RewardSO DraggingReward => draggingReward;
    public int DraggingEquipSlot => draggingEquipSlot;

    private void Awake()
    {
        Instance = this;

        dragIcon.gameObject.SetActive(false);
    }

    public void BeginDrag(RewardSO reward, int equipSlot = -1)
    {
        draggingReward = reward;
        draggingEquipSlot = equipSlot;

        dragIcon.sprite = reward.icon;
        dragIcon.gameObject.SetActive(true);
    }

    public void Drag(Vector2 position)
    {
      

        dragIcon.rectTransform.position = position;
    }

    public void EndDrag()
    {
        draggingReward = null;
        draggingEquipSlot = -1;

        dragIcon.gameObject.SetActive(false);
    }
}