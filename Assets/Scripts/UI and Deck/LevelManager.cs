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

    private bool isLoadingScene = false;

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

        if (isLoadingScene)
        {
            Debug.LogWarning($"[LevelManager] Đang loading scene, bỏ qua lệnh LoadScene('{sceneName}') gọi trùng.");
            return;
        }

        sceneTransName = sceneName;
        StartCoroutine(LoadLoadingScene());
    }

    private IEnumerator LoadLoadingScene()
    {
        isLoadingScene = true;

        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        yield return SceneManager.LoadSceneAsync("LoadingScene");

        yield return transition.AnimateTransitionOut();

        isLoadingScene = false;
    }

    private IEnumerator LoadInventory()
    {
        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        yield return transition.AnimateTransitionIn();
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

        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        AsyncOperation scene = SceneManager.LoadSceneAsync(
            sceneName, LoadSceneMode.Additive);

        scene.allowSceneActivation = false;

        yield return transition.AnimateTransitionIn();

        scene.allowSceneActivation = true;

        yield return transition.AnimateTransitionOut();
    }

    public void UnloadSceneAdditive(string sceneName)
    {
        StartCoroutine(UnloadSceneAdditiveAsync(sceneName));
    }

    private IEnumerator UnloadSceneAdditiveAsync(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield break;

        SceneTransition transition =
            transitions.First(t => t.name == "CrossFade");

        yield return transition.AnimateTransitionIn();

        AsyncOperation scene = SceneManager.UnloadSceneAsync(sceneName);

        yield return scene;

        yield return transition.AnimateTransitionOut();
    }
}