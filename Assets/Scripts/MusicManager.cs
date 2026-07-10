using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource audioSource;
    private string currentTrack;
    private Coroutine fadeCoroutine;

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

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (currentTrack == trackName)
            return;

        AudioClip clip = musicLibrary.GetClipFromName(trackName);

        if (clip == null)
            return;

        currentTrack = trackName;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            AnimateMusicCrossfade(clip, fadeDuration));
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration)
    {
        float startVolume = audioSource.volume;

        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            audioSource.volume =
                Mathf.Lerp(startVolume, 0, t / fadeDuration);

            yield return null;
        }

        audioSource.clip = nextTrack;
        audioSource.loop = true;
        audioSource.Play();

        t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            audioSource.volume =
                Mathf.Lerp(0, startVolume, t / fadeDuration);

            yield return null;
        }

        audioSource.volume = startVolume;
    }
}
