using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public void Continue()
    {
        Debug.Log("[INVENTORY] Continue");

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ContinueFromInventory();
        }
    }
}