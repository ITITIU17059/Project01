using UnityEngine;

public class EndingController : MonoBehaviour
{
    public void BackToMainMenu()
    {
        LevelManager.instance.LoadScene("MenuScene");
    }
}