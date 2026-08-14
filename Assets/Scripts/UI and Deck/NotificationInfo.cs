using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    public static NotificationInfo Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public IEnumerator SetUp(string notification)
    {
        gameObject.SetActive(true);
        notificationText.text = notification;

        yield return new WaitForSeconds(0.6f);
        gameObject.SetActive(false);
    }
}
