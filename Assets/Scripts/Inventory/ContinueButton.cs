using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public void Continue()
    {
        Debug.Log("CONTINUE CLICK");

        BossManager.Instance.LoadNextBoss();

        BattleManager.Instance.ContinueFromInventory();
    }
}