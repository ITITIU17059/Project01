using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;

    public override IEnumerator AnimateTransitionIn()
    {
        crossFade.transform.SetAsLastSibling();

        yield return crossFade
            .DOFade(1f, 1f)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }

    public override IEnumerator AnimateTransitionOut()
    {
        crossFade.transform.SetAsLastSibling();

        yield return crossFade
            .DOFade(0f, 1f)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }
}