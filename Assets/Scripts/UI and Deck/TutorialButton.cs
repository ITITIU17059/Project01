using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialButton : MonoBehaviour
{
    private List<Button> tutorialButtons;
    private GameObject tutorialContainer;
    [SerializeField] private GameObject videoContainer;
    [SerializeField] private TextMeshProUGUI tutorialNameText;
    [SerializeField] private TextMeshProUGUI tutorialContentText;
    [SerializeField] private TextMeshProUGUI tutorialName;
    [SerializeField] private TextMeshProUGUI tutorialContent;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        tutorialContainer = GameObject.FindGameObjectWithTag("TutorialContainer");
        tutorialButtons = new List<Button>();
        tutorialButtons.AddRange(tutorialContainer.GetComponentsInChildren<Button>());
    }

    public void HandleClick()
    {
        GetComponent<Button>().interactable = false;
        foreach (Button tutorialButton in tutorialButtons)
        {
            if (tutorialButton == this.GetComponent<Button>()) continue;
            if (tutorialButton.interactable == false)
            {
                tutorialButton.interactable = true;
                break;
            }
        }
        SetUpVideo();
        SetUpText();
    }

    private void SetUpVideo()
    {
        videoContainer.SetActive(true);
        RawImage rawImage = videoContainer.GetComponentInChildren<RawImage>();
        rawImage.texture = renderTexture;
        videoPlayer.clip = videoClip;
        videoPlayer.targetTexture = renderTexture;
    }

    private void SetUpText()
    {
        tutorialNameText.gameObject.SetActive(true);
        tutorialContentText.gameObject.SetActive(true);
        tutorialNameText.text = tutorialName.text;
        tutorialContentText.text = tutorialContent.text;
    }
}
