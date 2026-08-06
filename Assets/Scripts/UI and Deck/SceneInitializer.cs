using System.Collections;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    private IEnumerator Start()
    {
        Debug.Log("SceneInitializer Start");

        yield return null;

        Debug.Log("Calling FadeOut");

        LevelManager.instance.FadeOut();
    }
}