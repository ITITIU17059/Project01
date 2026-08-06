using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;
    private SceneTransition[] transitions;
    public Slider progressBar;

    private void Start()
    {
        transitions = LevelManager.instance.transitions;
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(LevelManager.instance.sceneTransName);

        SceneTransition transition =
            LevelManager.instance.transitions.First(t => t.name == "CrossFade");

        operation.allowSceneActivation = false;

        float timer = 0f;
        float displayProgress = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            float target =
                Mathf.Clamp01(operation.progress / 0.9f);

            displayProgress = Mathf.MoveTowards(
                displayProgress,
                target,
                Time.deltaTime * 0.6f);

            progressBar.value = displayProgress;

            if (timer >= 2f &&
                operation.progress >= 0.9f &&
                displayProgress >= 1f)
            {
                break;
            }

            yield return null;
        }
        yield return transition.AnimateTransitionIn();

        operation.allowSceneActivation = true;
    }

    IEnumerator AnimateLoadingText()
    {
        string text = "Loading";

        while (true)
        {
            loadingText.text = text;
            yield return new WaitForSeconds(0.3f);

            loadingText.text = text + ".";
            yield return new WaitForSeconds(0.3f);

            loadingText.text = text + "..";
            yield return new WaitForSeconds(0.3f);

            loadingText.text = text + "...";
            yield return new WaitForSeconds(0.3f);
        }
    }
}