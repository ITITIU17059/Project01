using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

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

    public void SaveProgress(int stageIndex, int bossIndex)
    {
        SaveData data = new();

        data.stageIndex = stageIndex;
        data.bossIndex = bossIndex;

        data.handCards = BattleManager.Instance.HandManager.GetSaveData();

        data.deckCards = BattleManager.Instance.DeckManager.GetDeckSaveData();

        data.graveyardCards = GraveyardManager.Instance.GetSaveData();

        data.bossSequence = BossManager.Instance.GetBossSequence();

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public SaveData LoadProgress()
    {
        if (!File.Exists(SavePath))
            return null;

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }
}