using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventoryAnimationManager : MonoBehaviour
{
    [SerializeField]
    private Image flyingIcon;

    [SerializeField]
    private Canvas canvas;

    public IEnumerator PlayEquip(
        RewardSlot from,
        EquipSlot to,
        Action onFinish)
    {
        flyingIcon.gameObject.SetActive(true);

        flyingIcon.sprite = from.Reward.icon;

        flyingIcon.transform.position =
            from.IconRect.position;

        yield return flyingIcon.transform
            .DOMove(
                to.IconRect.position,
                .35f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        flyingIcon.gameObject.SetActive(false);

        onFinish?.Invoke();
    }

    public IEnumerator PlayUnequip(
        EquipSlot from,
        RewardSlot to,
        Action onFinish)
    {
        flyingIcon.gameObject.SetActive(true);

        flyingIcon.sprite =
            from.CurrentReward.icon;

        flyingIcon.transform.position =
            from.IconRect.position;

        yield return flyingIcon.transform
            .DOMove(
                to.IconRect.position,
                .35f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        flyingIcon.gameObject.SetActive(false);

        onFinish?.Invoke();
    }
}