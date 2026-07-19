using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundLibrary soundLibrary;
    public static SoundManager instance;
    [SerializeField] private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        if (audioSource == null)
            audioSource = transform.Find("AudioSource").GetComponent<AudioSource>();
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(soundLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        audioSource.PlayOneShot(soundLibrary.GetClipFromName(soundName));
    }

    public void PlayHover()
    {
        PlaySound2D("Hover");
    }

    public void PlayClick()
    {
        PlaySound2D("Click");
    }
}
