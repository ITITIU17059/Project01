using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Button continueButton;
    [SerializeField] private ButtonManager continueButtonManager;
    [SerializeField] private EventTrigger eventTrigger;

    private void Start()
    {
        LoadVolume();

        MusicManager.instance.PlayMusic("MainMenu");
        RefreshContinueButton();
    }

    public void Play()
    {
        SaveVolume();
        SaveManager.Instance.DeleteSave();

        LevelManager.instance.LoadBattle();
    }

    public void ContinueGame()
    {
        SaveVolume();
        LevelManager.instance.LoadBattle();
    }

    public void Exit()
    {
        SaveVolume();

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

        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        float music =
            PlayerPrefs.GetFloat("Music Volume", 0f);

        float sfx =
            PlayerPrefs.GetFloat("SFX Volume", 0f);

        musicSlider.value = music;
        sfxSlider.value = sfx;

        audioMixer.SetFloat("Music Volume", music);
        audioMixer.SetFloat("SFX Volume", sfx);
    }

    private void RefreshContinueButton()
    {
        bool hasSave = SaveManager.Instance != null &&
                       SaveManager.Instance.HasSave();

        continueButton.interactable = hasSave;
        continueButtonManager.RefreshState();
        eventTrigger.enabled = hasSave;
    }

}