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

    public void PlayClick()
    {
        SoundManager.instance.PlaySound2D("Click");
    }
}
