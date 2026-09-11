using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossChatBalloon : MonoBehaviour
{
    public static BossChatBalloon Instance { get; set; }
    [SerializeField] private Image iconChat;
    [SerializeField] private TextMeshProUGUI textChat;
    [SerializeField] private GameObject chatBox;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetUp(string bossText)
    {
        StartCoroutine(TurnOnChatBox(bossText));
    }

    public IEnumerator TurnOnChatBox(string bossText)
    {
        AudioSource audioSource = GameObject.FindGameObjectWithTag("AudioSource")
                                    .GetComponent<AudioSource>();
        iconChat.sprite = BossManager.Instance.CurrentBoss.bossIcon;
        textChat.text = bossText;
        iconChat.gameObject.SetActive(true);
        textChat.gameObject.SetActive(true);
        chatBox.SetActive(true);

        yield return new WaitWhile(() => audioSource.isPlaying);

        iconChat.gameObject.SetActive(false);
        textChat.gameObject.SetActive(false);
        chatBox.SetActive(false);

    }
}
