using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public Slider progressBar;
    public GameObject transitionsContainer;
    private SceneTransition[] transitions;

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

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        yield return transition.AnimateTransitionIn();

        progressBar.gameObject.SetActive(true);

        do
        {
            progressBar.value = scene.progress;
            yield return null;
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        progressBar.gameObject.SetActive(false);
        yield return transition.AnimateTransitionOut();
    }

    public void LoadMainMenu()
    {
        LoadScene("MenuScene", "CrossFade");
    }

    public void LoadBattle()
    {
        LoadScene("BattleScene", "CrossFade");
    }

    public void LoadIntro()
    {
        LoadScene("IntroScene", "CrossFade");
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