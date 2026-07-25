using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField] private string battleScene = "BattleScene";
    [SerializeField] private string inventoryScene = "InventoryScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadBattle()
    {
        SceneManager.LoadScene(battleScene);
    }

    public void LoadInventory()
    {
        SceneManager.LoadScene(inventoryScene);
    }
}