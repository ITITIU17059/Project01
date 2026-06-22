using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private Image image;
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
}
