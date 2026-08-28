using System.Collections;
using DG.Tweening;
using UnityEngine;

public class JesterHandManager : MonoBehaviour
{
    public static JesterHandManager Instance { get; private set; }

    [Header("Jester Cards")]
    [SerializeField] private GameObject resetJester;
    [SerializeField] private GameObject instantKillJester;

    [Header("Jester Hand")]
    [SerializeField] private Transform resetHandPoint;
    [SerializeField] private Transform instantKillHandPoint;

    [Header("Jester Play Area")]
    [SerializeField] private Transform jesterPlayArea;

    private GameObject selectedJester;

    // Modal UI visual state for the actual Jester hand cards.
    private bool jesterVisualsSuppressed = false;
    private bool jesterInteractionLockedExternally = false;
    private bool resetJesterWasActive = false;
    private bool instantKillJesterWasActive = false;

    public void SetVisualsSuppressed(bool suppressed)
    {
        if (jesterVisualsSuppressed == suppressed)
            return;

        jesterVisualsSuppressed = suppressed;

        if (suppressed)
        {
            resetJesterWasActive =
                resetJester != null && resetJester.activeSelf;

            instantKillJesterWasActive =
                instantKillJester != null && instantKillJester.activeSelf;

            if (resetJester != null)
                resetJester.SetActive(false);

            if (instantKillJester != null)
                instantKillJester.SetActive(false);
        }
        else
        {
            if (resetJester != null)
                resetJester.SetActive(resetJesterWasActive);

            if (instantKillJester != null)
                instantKillJester.SetActive(instantKillJesterWasActive);

            Refresh();
        }
    }

    private bool resetLocked;
    private bool instantKillLocked;

    private bool executing;

    [Header("Jester Scale")]
    [SerializeField] private Vector3 jesterPlayScale = Vector3.one;

    private Vector3 resetHandScale;
    private Vector3 instantKillHandScale;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (JesterManager.Instance != null)
        {
            JesterManager.Instance.OnChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (JesterManager.Instance != null)
        {
            JesterManager.Instance.OnChanged -= Refresh;
        }
    }

    private void Start()
    {
        if (resetJester != null)
        {
            resetHandScale =
                resetJester.transform.localScale;
        }

        if (instantKillJester != null)
        {
            instantKillHandScale =
                instantKillJester.transform.localScale;
        }

        Refresh();
    }

    public bool HasSelectedJester =>
        selectedJester != null;

    public GameObject SelectedJester =>
        selectedJester;

    public void SelectResetJester()
    {
        if (executing)
            return;

        if (selectedJester != null)
            return;

        if (resetLocked)
            return;

        if (JesterManager.Instance == null)
            return;

        if (!JesterManager.Instance.CanUseReset)
            return;

        SelectJester(resetJester);
    }

    public void SelectInstantKillJester()
    {
        if (executing)
            return;

        if (selectedJester != null)
            return;

        if (instantKillLocked)
            return;

        if (JesterManager.Instance == null)
            return;

        if (!JesterManager.Instance.CanUseInstantKill)
            return;

        SelectJester(instantKillJester);
    }

    private void SelectJester(GameObject jester)
    {
        if (jester == null)
            return;

        selectedJester = jester;

        jester.transform.DOKill();

        Vector3 targetPlayScale = jesterPlayScale;

        if (jesterPlayArea != null)
        {
            jester.transform
                .DOMove(
                    jesterPlayArea.position,
                    0.3f
                )
                .SetEase(Ease.OutCubic);
        }

        jester.transform
            .DORotate(
                Vector3.zero,
                0.3f
            )
            .SetEase(Ease.OutCubic);

        jester.transform
            .DOScale(
                targetPlayScale,
                0.3f
            )
            .SetEase(Ease.OutBack);

        SetSortingOrder(jester, 100);

        SetJesterButtonsInteractable(false);

        Debug.Log(
            $"[JESTER] Selected: {jester.name}"
        );
    }


    public void ConfirmSelectedJester()
    {
        if (executing)
            return;

        if (selectedJester == null)
            return;

        if (BattleManager.Instance == null)
            return;

        if (JesterManager.Instance == null)
            return;

        StartCoroutine(ExecuteSelectedJester());
    }

    private IEnumerator ExecuteSelectedJester()
    {
        executing = true;

        GameObject usedJester =
            selectedJester;

        bool skillStarted = false;
        bool instantKillUsed =
            usedJester == instantKillJester;

        yield return CardResolver.PlaySuitFX(
            HandManager.Instance.selectedCards);

        if (usedJester == resetJester)
        {
            if (!JesterManager.Instance.CanUseReset)
            {
                CancelSelection();
                executing = false;
                yield break;
            }

            skillStarted =
                BattleManager.Instance.UseJesterReset();
        }
        else if (usedJester == instantKillJester)
        {
            if (!JesterManager.Instance.CanUseInstantKill)
            {
                CancelSelection();
                executing = false;
                yield break;
            }

            skillStarted =
                BattleManager.Instance.UseJesterInstantKill();
        }

        if (!skillStarted)
        {
            CancelSelection();
            executing = false;
            yield break;
        }

        // Instant Kill must enter the normal boss-death pipeline immediately.
        // Do not wait for the Jester return animation: waiting here can race
        // with other end-of-turn state changes and leave the next boss reload
        // out of sync.
        if (instantKillUsed &&
            BattleManager.Instance != null &&
            BattleManager.Instance.CurrentState ==
                BattleState.PlayerTurn)
        {
            BattleManager.Instance.ChangeState(
                BattleState.CheckBattle);
        }

        if (usedJester == resetJester)
        {
            yield return new WaitUntil(
                () =>
                    BattleManager.Instance.CurrentState
                        != BattleState.PlayerTurn ||
                    BattleManager.Instance.Hand.handCards.Count >= 8
            );
        }

        LockUsedJester(usedJester);

        if (HandManager.Instance != null)
        {
            HandManager.Instance.selectedCards.Remove(usedJester);
        }

        yield return StartCoroutine(
            ReturnJesterToHand(usedJester));

        selectedJester = null;
        executing = false;

        Refresh();
    }

    private IEnumerator ReturnJesterToHand(
       GameObject jester)
    {
        if (jester == null)
            yield break;

        Transform returnPoint = null;
        Vector3 targetHandScale;

        if (jester == resetJester)
        {
            returnPoint = resetHandPoint;
            targetHandScale = resetHandScale;
        }
        else if (jester == instantKillJester)
        {
            returnPoint = instantKillHandPoint;
            targetHandScale = instantKillHandScale;
        }
        else
        {
            yield break;
        }

        if (returnPoint != null)
        {
            CardInteraction interaction =
                jester.GetComponent<CardInteraction>();

            if (interaction != null)
            {
                interaction.isSelectedInCenter = false;
                interaction.IsLocked = true;
            }

            jester.transform.DOKill();

            jester.transform
                .DOMove(
                    returnPoint.position,
                    0.3f
                )
                .SetEase(Ease.OutCubic);

            jester.transform
                .DORotate(
                    returnPoint.rotation.eulerAngles,
                    0.3f
                )
                .SetEase(Ease.OutCubic);

            jester.transform
                .DOScale(
                    targetHandScale,
                    0.3f
                )
                .SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void LockUsedJester(GameObject jester)
    {
        if (jester == null)
            return;

        if (jester == resetJester)
        {
            resetLocked = true;
        }

        if (jester == instantKillJester)
        {
            instantKillLocked = true;
        }

        CardDisplay display =
            jester.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetFade(true);
        }

        CardInteraction interaction =
            jester.GetComponent<CardInteraction>();

        if (interaction != null)
        {
            interaction.IsLocked = true;
        }

        Debug.Log(
            $"[JESTER] Locked: {jester.name}"
        );
    }

    public void CancelSelection()
    {
        if (executing)
            return;

        if (selectedJester == null)
            return;

        GameObject jester = selectedJester;

        selectedJester = null;

        if (HandManager.Instance != null)
        {
            HandManager.Instance.selectedCards.Remove(jester);
        }

        Transform returnPoint = null;
        Vector3 targetHandScale;

        if (jester == resetJester)
        {
            returnPoint = resetHandPoint;
            targetHandScale = resetHandScale;
        }
        else if (jester == instantKillJester)
        {
            returnPoint = instantKillHandPoint;
            targetHandScale = instantKillHandScale;
        }
        else
        {
            return;
        }

        if (returnPoint != null)
        {
            CardInteraction interaction =
                jester.GetComponent<CardInteraction>();

            if (interaction != null)
            {
                interaction.isSelectedInCenter = false;
                interaction.IsLocked = false;
            }

            jester.transform.DOKill();

            jester.transform
                .DOMove(
                    returnPoint.position,
                    0.25f
                )
                .SetEase(Ease.OutCubic);

            jester.transform
                .DORotate(
                    returnPoint.rotation.eulerAngles,
                    0.25f
                )
                .SetEase(Ease.OutCubic);

            jester.transform
                .DOScale(
                    targetHandScale,
                    0.25f
                )
                .SetEase(Ease.OutCubic);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (JesterManager.Instance == null)
            return;

        bool unlocked =
            JesterManager.Instance.IsUnlocked;

        resetLocked =
            !JesterManager.Instance.CanUseReset;

        instantKillLocked =
            !JesterManager.Instance.CanUseInstantKill;

        if (resetJester != null)
        {
            resetJester.SetActive(
                jesterVisualsSuppressed
                    ? false
                    : unlocked
            );

            CardDisplay display =
                resetJester.GetComponent<CardDisplay>();

            if (display != null)
            {
                display.SetFade(resetLocked);
            }

            CardInteraction interaction =
                resetJester.GetComponent<CardInteraction>();

            if (interaction != null)
            {
                interaction.IsLocked =
                    resetLocked ||
                    jesterVisualsSuppressed ||
                    jesterInteractionLockedExternally ||
                    executing ||
                    selectedJester != null;
            }
        }

        if (instantKillJester != null)
        {
            instantKillJester.SetActive(
                jesterVisualsSuppressed
                    ? false
                    : unlocked
            );

            CardDisplay display =
                instantKillJester.GetComponent<CardDisplay>();

            if (display != null)
            {
                display.SetFade(instantKillLocked);
            }

            CardInteraction interaction =
                instantKillJester.GetComponent<CardInteraction>();

            if (interaction != null)
            {
                interaction.IsLocked =
                    instantKillLocked ||
                    jesterVisualsSuppressed ||
                    jesterInteractionLockedExternally ||
                    executing ||
                    selectedJester != null;
            }
        }

        SetJesterButtonsInteractable(
            !executing &&
            selectedJester == null
        );
    }

    private void SetJesterButtonsInteractable(
        bool value
    )
    {
        bool canUseReset =
            JesterManager.Instance != null &&
            JesterManager.Instance.CanUseReset;

        bool canUseInstantKill =
            JesterManager.Instance != null &&
            JesterManager.Instance.CanUseInstantKill;

        if (resetJester != null)
        {
            UnityEngine.UI.Button button =
                resetJester.GetComponent<UnityEngine.UI.Button>();

            if (button != null)
            {
                button.interactable =
                    value &&
                    canUseReset &&
                    !resetLocked;
            }
        }

        if (instantKillJester != null)
        {
            UnityEngine.UI.Button button =
                instantKillJester.GetComponent<UnityEngine.UI.Button>();

            if (button != null)
            {
                button.interactable =
                    value &&
                    canUseInstantKill &&
                    !instantKillLocked;
            }
        }
    }

    private void SetSortingOrder(
        GameObject obj,
        int order)
    {
        if (obj == null)
            return;

        CardDisplay display =
            obj.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetSortingOrder(order);
        }
    }

    public void SelectJesterCard(GameObject jester)
    {
        if (jester == null)
            return;

        if (executing)
            return;

        if (selectedJester != null)
            return;

        if (jester == resetJester)
        {
            SelectResetJester();
            return;
        }

        if (jester == instantKillJester)
        {
            SelectInstantKillJester();
            return;
        }
    }

    public void DeselectJesterCard(GameObject jester)
    {
        if (jester == null)
            return;

        if (executing)
            return;

        selectedJester = null;

        StartCoroutine(ReturnJesterToHand(jester));
        Refresh();
    }
    public void SetJesterInteractionLocked(bool locked)
    {
        // This lock must survive JesterManager.OnChanged -> Refresh().
        // Without this flag, consuming a Jester charge calls Refresh() and
        // immediately unlocks the other Jester while the battle/trait modal
        // is still active.
        jesterInteractionLockedExternally = locked;

        SetInteractionLock(resetJester, locked);
        SetInteractionLock(instantKillJester, locked);
    }

    private void SetInteractionLock(
        GameObject jester,
        bool locked)
    {
        if (jester == null)
            return;

        CardInteraction interaction =
            jester.GetComponent<CardInteraction>();

        if (interaction == null)
            return;

        interaction.IsLocked = locked;

        if (locked)
        {
            jester.transform.DOKill();

            if (JesterDescriptionUI.Instance != null)
            {
                JesterDescriptionUI.Instance.Hide();
            }
        }
    }
    public void ReturnJesterToPlayZone(GameObject jester)
    {
        if (jester == null)
            return;

        if (jesterPlayArea == null)
        {
            Debug.LogWarning(
                "[JESTER] Jester Play Area chưa được gán!"
            );

            return;
        }

        CardInteraction interaction =
            jester.GetComponent<CardInteraction>();

        if (interaction != null)
        {
            interaction.isSelectedInCenter = true;
        }

        jester.transform.DOKill();

        jester.transform
            .DOMove(
                jesterPlayArea.position,
                0.25f
            )
            .SetEase(Ease.OutCubic);

        jester.transform
            .DORotate(
                jesterPlayArea.rotation.eulerAngles,
                0.2f
            )
            .SetEase(Ease.OutCubic);

        jester.transform
            .DOScale(
                jesterPlayScale,
                0.2f
            )
            .SetEase(Ease.OutCubic);

        Debug.Log(
            $"[JESTER] {jester.name} returned to Play Zone."
        );
    }

}