using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private Image image;
    public Button button;
    public void onImageHover()
    {
        image.color = Color.grey;
    }

    public void onImageExit()
    {
        image.color = Color.white;
    }

    public void onImageClick()
    {
        image.color = Color.darkGray;
    }

    public void PlayHover()
    {
        SoundManager.instance.PlaySound2D("Hover");
    }

    public void PlayClick(string audioName)
    {
        SoundManager.instance.PlaySound2D(audioName);
    }



    public void RefreshState()
    {
        image.color = button.interactable
            ? Color.white
            : new Color(1f, 1f, 1f, 0.4f);
    }
}
