using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioMixer audioMixer;
    private void Start()
    {
        LoadVolume();
        MusicManager.instance.PlayMusic("MainMenu");
    }

    public void Play()
    {
        LevelManager.instance.LoadScene("SampleScene", "CrossFade");
        MusicManager.instance.PlayMusic("Gameplay");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music Volume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFX Volume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("Music Volume", out float musicVolume);
        PlayerPrefs.SetFloat("Music Volume", musicVolume);

        audioMixer.GetFloat("SFX Volume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFX Volume", sfxVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("Music Volume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFX Volume");
    }
}
