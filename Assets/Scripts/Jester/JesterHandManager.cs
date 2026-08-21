using System.Collections;
using DG.Tweening;
using UnityEngine;

public class JesterHandManager : MonoBehaviour
{
    public static JesterHandManager Instance { get; private set; }

    //==================================================
    // JESTER CARDS
    //==================================================

    [Header("Jester Cards")]
    [SerializeField] private GameObject resetJester;
    [SerializeField] private GameObject instantKillJester;

    //==================================================
    // JESTER HAND POSITIONS
    //==================================================

    [Header("Jester Hand")]
    [SerializeField] private Transform resetHandPoint;
    [SerializeField] private Transform instantKillHandPoint;

    //==================================================
    // PLAY AREA
    //==================================================

    [Header("Jester Play Area")]
    [SerializeField] private Transform jesterPlayArea;

    //==================================================
    // STATE
    //==================================================

    private GameObject selectedJester;

    private bool resetLocked;
    private bool instantKillLocked;

    private bool executing;

    //==================================================
    // UNITY
    //==================================================

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
        Refresh();
    }

    //==================================================
    // PUBLIC STATE
    //==================================================

    public bool HasSelectedJester =>
        selectedJester != null;

    public GameObject SelectedJester =>
        selectedJester;

    //==================================================
    // SELECT RESET
    //==================================================

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

    //==================================================
    // SELECT INSTANT KILL
    //==================================================

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

    //==================================================
    // SELECT
    //==================================================

    private void SelectJester(GameObject jester)
    {
        if (jester == null)
            return;

        selectedJester = jester;

        jester.transform.DOKill();

        // Bay lên khu vực đánh
        if (jesterPlayArea != null)
        {
            jester.transform.DOMove(
                jesterPlayArea.position,
                0.3f
            ).SetEase(Ease.OutCubic);
        }

        jester.transform.DORotate(
            Vector3.zero,
            0.3f
        ).SetEase(Ease.OutCubic);

        jester.transform.DOScale(
            Vector3.one,
            0.3f
        ).SetEase(Ease.OutCubic);

        SetSortingOrder(jester, 100);

        // Tắt interaction của 2 Jester trong lúc đang chọn
        SetJesterButtonsInteractable(false);

        Debug.Log(
            $"[JESTER] Selected: {jester.name}"
        );
    }

    //==================================================
    // CONFIRM
    //==================================================

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

    //==================================================
    // EXECUTE
    //==================================================

    private IEnumerator ExecuteSelectedJester()
    {
        executing = true;

        GameObject usedJester = selectedJester;

        // ---------------------------------------------
        // RESET
        // ---------------------------------------------

        if (usedJester == resetJester)
        {
            if (!JesterManager.Instance.CanUseReset)
            {
                CancelSelection();
                yield break;
            }

            // Skill thật sự chỉ được kích hoạt ở Confirm.
            BattleManager.Instance.UseJesterReset();

            yield return new WaitForSeconds(0.4f);
        }

        // ---------------------------------------------
        // INSTANT KILL
        // ---------------------------------------------

        else if (usedJester == instantKillJester)
        {
            if (!JesterManager.Instance.CanUseInstantKill)
            {
                CancelSelection();
                yield break;
            }

            // Skill thật sự chỉ được kích hoạt ở Confirm.
            BattleManager.Instance.UseJesterInstantKill();

            yield return new WaitForSeconds(0.4f);
        }

        // ---------------------------------------------
        // LOCK
        // ---------------------------------------------

        LockUsedJester(usedJester);

        // ---------------------------------------------
        // RETURN TO JESTER HAND
        // ---------------------------------------------

        yield return ReturnJesterToHand(usedJester);

        selectedJester = null;
        executing = false;

        Refresh();

        Debug.Log(
            $"[JESTER] Finished: {usedJester.name}"
        );
    }

    //==================================================
    // RETURN TO HAND
    //==================================================

    private IEnumerator ReturnJesterToHand(
        GameObject jester)
    {
        if (jester == null)
            yield break;

        Transform returnPoint = null;

        if (jester == resetJester)
        {
            returnPoint = resetHandPoint;
        }
        else if (jester == instantKillJester)
        {
            returnPoint = instantKillHandPoint;
        }

        if (returnPoint != null)
        {
            jester.transform.DOKill();

            jester.transform.DOMove(
                returnPoint.position,
                0.3f
            ).SetEase(Ease.OutCubic);

            jester.transform.DORotate(
                returnPoint.rotation.eulerAngles,
                0.3f
            ).SetEase(Ease.OutCubic);

            jester.transform.DOScale(
                returnPoint.localScale,
                0.3f
            ).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(0.3f);
        }
    }

    //==================================================
    // LOCK
    //==================================================

    private void LockUsedJester(GameObject jester)
    {
        if (jester == null)
            return;

        if (jester == resetJester)
        {
            resetLocked =
                JesterManager.Instance == null ||
                !JesterManager.Instance.CanUseReset;
        }

        if (jester == instantKillJester)
        {
            instantKillLocked =
                JesterManager.Instance == null ||
                !JesterManager.Instance.CanUseInstantKill;
        }

        CardDisplay display =
            jester.GetComponent<CardDisplay>();

        if (display != null)
        {
            display.SetFade(
                jester == resetJester
                    ? resetLocked
                    : instantKillLocked
            );
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

    //==================================================
    // CANCEL
    //==================================================

    public void CancelSelection()
    {
        if (executing)
            return;

        if (selectedJester == null)
            return;

        GameObject jester = selectedJester;

        selectedJester = null;

        Transform returnPoint = null;

        if (jester == resetJester)
            returnPoint = resetHandPoint;

        if (jester == instantKillJester)
            returnPoint = instantKillHandPoint;

        if (returnPoint != null)
        {
            jester.transform.DOKill();

            jester.transform.DOMove(
                returnPoint.position,
                0.25f
            ).SetEase(Ease.OutCubic);

            jester.transform.DORotate(
                returnPoint.rotation.eulerAngles,
                0.25f
            ).SetEase(Ease.OutCubic);
        }

        Refresh();
    }

    //==================================================
    // REFRESH
    //==================================================

    public void Refresh()
    {
        if (JesterManager.Instance == null)
            return;

        bool unlocked =
            JesterManager.Instance.IsUnlocked;

        if (resetJester != null)
        {
            resetJester.SetActive(unlocked);

            CardDisplay display =
                resetJester.GetComponent<CardDisplay>();

            if (display != null)
            {
                display.SetFade(resetLocked);
            }
        }

        if (instantKillJester != null)
        {
            instantKillJester.SetActive(unlocked);

            CardDisplay display =
                instantKillJester.GetComponent<CardDisplay>();

            if (display != null)
            {
                display.SetFade(instantKillLocked);
            }
        }

        SetJesterButtonsInteractable(
            !executing &&
            selectedJester == null
        );
    }

    //==================================================
    // BUTTON / CARD INTERACTION
    //==================================================

    private void SetJesterButtonsInteractable(
        bool value)
    {
        if (resetJester != null)
        {
            UnityEngine.UI.Button button =
                resetJester.GetComponent<UnityEngine.UI.Button>();

            if (button != null)
            {
                button.interactable =
                    value &&
                    !resetLocked &&
                    JesterManager.Instance != null &&
                    JesterManager.Instance.CanUseReset;
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
                    !instantKillLocked &&
                    JesterManager.Instance != null &&
                    JesterManager.Instance.CanUseInstantKill;
            }
        }
    }

    //==================================================
    // SORTING
    //==================================================

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
    //==================================================
    // CARD INTERACTION
    //==================================================

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

        if (selectedJester != jester)
            return;

        selectedJester = null;

        Transform returnPoint = null;

        if (jester == resetJester)
            returnPoint = resetHandPoint;

        else if (jester == instantKillJester)
            returnPoint = instantKillHandPoint;

        if (returnPoint != null)
        {
            jester.transform.DOKill();

            jester.transform.DOMove(
                returnPoint.position,
                0.25f
            ).SetEase(Ease.OutCubic);

            jester.transform.DORotate(
                returnPoint.rotation.eulerAngles,
                0.25f
            ).SetEase(Ease.OutCubic);

            jester.transform.DOScale(
                returnPoint.localScale,
                0.25f
            ).SetEase(Ease.OutCubic);
        }

        Refresh();
    }
}