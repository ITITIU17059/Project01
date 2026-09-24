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

    private IEnumerator LoadNextScene()
    {
        SceneTransition transition =
            LevelManager.instance.transitions
            .First(t => t.name == "CrossFade");

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                LevelManager.instance.sceneTransName
            );

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
                Time.deltaTime * 0.6f
            );

            progressBar.value = displayProgress;

            if (timer >= 2f &&
                operation.progress >= 0.9f &&
                displayProgress >= 1f)
            {
                break;
            }

            yield return null;
        }

        // LoadingScene vẫn đang nhìn thấy bình thường ở đây.
        // Bây giờ mới che nó bằng màu đen.
        yield return transition.AnimateTransitionIn();

        // Đảm bảo CrossFade đã render đen
        yield return new WaitForEndOfFrame();

        // Cho Scene mới activate
        operation.allowSceneActivation = true;

        // Chờ Scene mới thực sự hoàn thành
        while (!operation.isDone)
        {
            yield return null;
        }

        // Cho Scene mới có 1 frame để khởi tạo
        yield return null;

        // Mở màn hình
        yield return transition.AnimateTransitionOut();
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