using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public GameObject transitionsContainer;
    [NonSerialized] public SceneTransition[] transitions;
    public string sceneTransName;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void FadeOut()
    {
        Debug.Log("FadeOut called");
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        Debug.Log("FadeOutRoutine");

        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionOut();

        Debug.Log("FadeOut Finished");
    }

    public void LoadScene(string sceneName)
    {
        sceneTransName = sceneName;
        StartCoroutine(LoadLoadingScene());
    }

    private IEnumerator LoadLoadingScene()
    {
        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        yield return SceneManager.LoadSceneAsync("LoadingScene");

        yield return transition.AnimateTransitionOut();
    }

    public void LoadMainMenu()
    {
        LoadScene("MenuScene");
    }

    public void LoadBattle()
    {
        LoadScene("BattleScene");
    }

    public void LoadIntro()
    {
        LoadScene("IntroScene");
    }


    public void LoadSceneAdditive(string sceneName)
    {
        StartCoroutine(LoadSceneAdditiveAsync(sceneName));
    }

    private IEnumerator LoadSceneAdditiveAsync(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
            yield break;

        yield return SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive);
    }

    public void UnloadSceneAdditive(string sceneName)
    {
        StartCoroutine(UnloadSceneAdditiveAsync(sceneName));
    }

    private IEnumerator UnloadSceneAdditiveAsync(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield break;

        yield return SceneManager.UnloadSceneAsync(sceneName);
    }
}