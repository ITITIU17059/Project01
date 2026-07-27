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
        SaveData data = new SaveData()
        {
            stageIndex = stageIndex,
            bossIndex = bossIndex,

            handCards = HandManager.Instance.GetSaveData(),
            deckCards = TarvernDeckManager.Instance.GetDeckSaveData(),
            graveyardCards = GraveyardManager.Instance.GetSaveData(),

            bossSequence = BossManager.Instance.GetBossSequence(),

            ownedRewards = PlayerReward.Instance.GetOwnedRewardNames(),
            equippedRewards = PlayerReward.Instance.GetEquippedRewardNames(),

            currentTraitSelection = TraitSelectionPanelUI.Instance.GetCurrentTraitNames(),
            jackTraitPool = TraitPoolManager.Instance.GetPoolSaveData(BossRank.Jack),
            queenTraitPool = TraitPoolManager.Instance.GetPoolSaveData(BossRank.Queen),
            kingTraitPool = TraitPoolManager.Instance.GetPoolSaveData(BossRank.King)
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Auto Save Success");
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