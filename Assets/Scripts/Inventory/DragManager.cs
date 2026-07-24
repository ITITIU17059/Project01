using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance;

    [SerializeField] private Image dragIcon;

    private RewardSO draggingReward;

    public RewardSO DraggingReward => draggingReward;

    private void Awake()
    {
        Instance = this;

        dragIcon.gameObject.SetActive(false);
    }

    public void BeginDrag(RewardSO reward)
    {
        draggingReward = reward;

        dragIcon.sprite = reward.icon;
        dragIcon.SetNativeSize();

        dragIcon.gameObject.SetActive(true);
    }

    public void Drag(Vector2 position)
    {
        Debug.Log(position);

        dragIcon.rectTransform.position = position;
    }

    public void EndDrag()
    {
        draggingReward = null;
        dragIcon.gameObject.SetActive(false);
    }
}