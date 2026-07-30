using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using DG.Tweening;
using System;
using UnityEngine.UI;
public class HandManager : MonoBehaviour
{
    public GameObject LockedCard { get; private set; }
    public static HandManager Instance { get; private set; }
    [SerializeField] private int defaultMaxHandSize = 8;
    public int maxHandSize;
    [SerializeField] private int spacingValue;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Button confirmDiscardButton;
    private bool isDiscardMode = false;
    private int discardTarget = 0;
    private int discardCurrent = 0;

    [SerializeField] private Transform playPreviewArea;
    private int totalCardValue;

    [NonSerialized] public List<GameObject> handCards = new();
    public List<GameObject> selectedCards = new();

    private void Awake()
    {
        maxHandSize = defaultMaxHandSize;
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        spacingValue = Mathf.Max(maxHandSize, spacingValue);
        totalCardValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private bool canInteract = true;
    public bool CanInteract => canInteract;
    public bool IsDiscardMode => isDiscardMode;

    public void SetInteractable(bool value)
    {
        canInteract = value;
    }
    public void SelectCard(GameObject cardObject)
    {
        if (isDiscardMode)
        {
            SelectDiscardCard(cardObject);
            return;
        }

        // Chỉ giới hạn ở lượt đánh thêm
        if (BattleManager.Instance.IsExtraAttack &&
            selectedCards.Count >= 1)
        {
            return;
        }

        if (handCards.Contains(cardObject))
        {
            var cardObjectValue = cardObject.GetComponent<CardDisplay>().cardScriptableObject.value;
            if (!CanSelectCard(cardObjectValue))
            {
                Debug.Log("Card phải cùng value (Ace được phép đi kèm)");

                cardObject.transform.DOKill();
                cardObject.transform.DOScale(Vector3.one, 0.2f)
                                    .SetEase(Ease.OutCubic);
                if (cardObject.TryGetComponent<CardInteraction>(out var interact))
                {
                    interact.HandleDeselect();
                }

                return;
            }

            handCards.Remove(cardObject);
            selectedCards.Add(cardObject);
            SoundManager.instance?.PlaySound2D("CardSelect");
            totalCardValue += cardObjectValue;

            // Ép kích thước bài về chuẩn ngay khi vừa được chọn
            cardObject.transform.DOKill();
            cardObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);

            // Bật Layer hiển thị cao lên
            if (cardObject.TryGetComponent<CardDisplay>(out var display))
            {
                display.SetSortingOrder(50 + selectedCards.Count);
            }

            RearrangeSelectedCards();
            RepositionAllCards(null);
        }
    }

    // Hàm trả bài từ vùng chờ về lại trên tay (Nếu người chơi đổi ý click hủy)
    public void DeselectCard(GameObject cardObject)
    {
        if (!canInteract && !isDiscardMode)
            return;


        if (isDiscardMode)
        {
            DeselectDiscardCard(cardObject);
            return;
        }

        if (selectedCards.Contains(cardObject))
        {
            var cardObjectValue = cardObject.GetComponent<CardDisplay>()
                                            .cardScriptableObject.value;

            selectedCards.Remove(cardObject);
            handCards.Add(cardObject);
            SoundManager.instance?.PlaySound2D("CardSelect");

            totalCardValue -= cardObjectValue;

            if (cardObject.TryGetComponent<CardInteraction>(out var interact))
                interact.HandleDeselect();

            if (cardObject.TryGetComponent<CardDisplay>(out var display))
                display.SetSortingOrder(10 + handCards.Count);

            RearrangeSelectedCards();
            RepositionAllCards(null);
        }
    }

    public void RearrangeSelectedCards()
    {
        if (selectedCards.Count == 0) return;

        float spacing = 1.2f;
        float startX = -((selectedCards.Count - 1) * spacing) / 2f;
        Vector3 centerPos = playPreviewArea != null ? playPreviewArea.position : Vector3.zero;

        for (int i = 0; i < selectedCards.Count; i++)
        {
            Vector3 targetPos = centerPos + new Vector3(startX + (i * spacing), 0, 0);

            selectedCards[i].transform.DOKill();

            selectedCards[i].transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutCubic);
            selectedCards[i].transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
            selectedCards[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutCubic);

            if (selectedCards[i].TryGetComponent<CardDisplay>(out var display))
            {
                display.SetSortingOrder(100 + i);
            }
        }
    }

    public void AddCardToHand(CardSO cardData)
    {
        if (handCards.Count >= maxHandSize) return;

        GameObject newCard = Instantiate(cardPrefab, cardSpawnPoint.position, cardSpawnPoint.rotation);
        SoundManager.instance?.PlaySound2D("CardDraw");
        handCards.Add(newCard);
        TraitManager.Instance.InvokeBossEvent(
    TraitEventType.Draw,
    1);

        TraitManager.Instance.InvokeRewardEvent(
            TraitEventType.Draw,
            1);
        newCard.GetComponent<CardDisplay>().cardScriptableObject = cardData;

        RepositionAllCards(null);
    }

    public void CheckForCardReorder(GameObject draggedCard)
    {
        int idx = handCards.IndexOf(draggedCard);
        if (idx == -1) return;

        if (idx > 0 && draggedCard.transform.position.x < handCards[idx - 1].transform.position.x)
        {
            SwapCards(idx, idx - 1, draggedCard);
        }
        else if (idx < handCards.Count - 1 && draggedCard.transform.position.x > handCards[idx + 1].transform.position.x)
        {
            SwapCards(idx, idx + 1, draggedCard);
        }
    }

    private void SwapCards(int currentIdx, int targetIdx, GameObject draggedCard)
    {
        (handCards[targetIdx], handCards[currentIdx]) = (handCards[currentIdx], handCards[targetIdx]);
        RepositionAllCards(draggedCard);
    }

    public void RepositionAllCards(GameObject ignoreCard)
    {
        if (splineContainer == null || splineContainer.Spline == null) return;
        if (handCards.Count == 0) return;

        if (handCards.Count == 0) return;

        float spacing = 1f / spacingValue;
        float firstPos = 0.5f - spacing * (handCards.Count - 1) / 2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < handCards.Count; i++)
        {
            CardDisplay display = handCards[i].GetComponent<CardDisplay>();
            if (!display) continue;

            CardInteraction interaction = handCards[i].GetComponent<CardInteraction>();
            if (!display || !interaction) continue;

            float p = firstPos + i * spacing;
            Vector3 localPos = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            display.SetSortingOrder(i);

            if (handCards[i] == ignoreCard)
            {
                interaction.splineLocalPosition = localPos;
            }
            else
            {
                interaction.MoveTo(localPos, rotation, 0.2f);
            }
        }
    }

    public void StartDiscardPhase(int targetValue)
    {
        isDiscardMode = true;

        discardTarget = targetValue;
        discardCurrent = 0;

        selectedCards.Clear();
        totalCardValue = 0;

        int totalValue = 0;

        foreach (GameObject card in handCards)
        {
            totalValue += card.GetComponent<CardDisplay>()
                              .cardScriptableObject.value;
        }

        if (totalValue < discardTarget)
        {
            BattleManager.Instance.FinishDiscard(false);
            return;
        }

        confirmDiscardButton.gameObject.SetActive(true);
        confirmDiscardButton.interactable = false;

        RearrangeSelectedCards();
    }

    private void SelectDiscardCard(GameObject cardObject)
    {
        if (selectedCards.Contains(cardObject))
            return;

        handCards.Remove(cardObject);
        selectedCards.Add(cardObject);

        int value = cardObject.GetComponent<CardDisplay>()
                              .cardScriptableObject.value;

        discardCurrent += value;

        RearrangeSelectedCards();

        Debug.Log($"Discard = {discardCurrent}/{discardTarget}");
        confirmDiscardButton.interactable =
    discardCurrent >= discardTarget;

    }

    private void DeselectDiscardCard(GameObject cardObject)
    {
        if (!selectedCards.Contains(cardObject))
            return;

        selectedCards.Remove(cardObject);
        handCards.Add(cardObject);

        int value = cardObject.GetComponent<CardDisplay>()
                              .cardScriptableObject.value;

        discardCurrent -= value;

        if (discardCurrent < 0)
            discardCurrent = 0;

        if (cardObject.TryGetComponent<CardInteraction>(out var interact))
            interact.HandleDeselect();

        if (cardObject.TryGetComponent<CardDisplay>(out var display))
            display.SetSortingOrder(10 + handCards.Count);

        RearrangeSelectedCards();
        RepositionAllCards(null);
        confirmDiscardButton.interactable =
    discardCurrent >= discardTarget;
        Debug.Log($"Discard = {discardCurrent}/{discardTarget}");
    }
    public void ConfirmDiscard()
    {
        if (!isDiscardMode)
            return;

        if (discardCurrent < discardTarget)
            return;

        FinishDiscard();
    }
    private void FinishDiscard()
    {
        if (PlayerReward.Instance.HasReward(TraitID.Q_ROYAL_TAX)
    && selectedCards.Count > 1)
        {
            ReturnRandomSelectedCard();
        }
        foreach (GameObject card in selectedCards)
        {
            CardSO so = card.GetComponent<CardDisplay>().cardScriptableObject;

            GraveyardManager.Instance.AddToGraveyard(so);

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                card,
                BattleManager.Instance.graveyardSpawnPoint
            );
        }
   
        selectedCards.Clear();
        totalCardValue = 0;

        discardCurrent = 0;
        discardTarget = 0;

        isDiscardMode = false;

        RepositionAllCards(null);

        BattleManager.Instance.FinishDiscard(true);
        confirmDiscardButton.gameObject.SetActive(false);
    }

    private bool CanSelectCard(int newValue)
    {
        // Chưa chọn lá nào -> luôn được chọn
        if (selectedCards.Count == 0)
            return true;

        int aceCount = 0;
        int mainValue = -1;
        int total = 0;

        foreach (GameObject card in selectedCards)
        {
            int value = card.GetComponent<CardDisplay>()
                            .cardScriptableObject.value;

            total += value;

            if (value == 1)
                aceCount++;
            else if (mainValue == -1)
                mainValue = value;
        }

        // ==========================
        // Chọn Ace
        // ==========================

        if (newValue == 1)
        {
            // Chỉ được 1 Ace Companion
            if (mainValue != -1)
                return aceCount == 0;

            // Chỉ toàn Ace
            return aceCount + 1 <= 10;
        }

        // ==========================
        // Đã có Ace nhưng chưa có bài chính
        // ==========================

        if (mainValue == -1)
        {
            return true;
        }

        // ==========================
        // Phải cùng số
        // ==========================

        if (newValue != mainValue)
            return false;

        // Nếu đã có Ace Companion thì bỏ giới hạn <=10
        if (aceCount == 1)
            return true;

        return total + newValue <= 10;
    }
    public void ResetAllCardHover()
    {
        foreach (GameObject card in handCards)
        {
            CardInteraction interaction = card.GetComponent<CardInteraction>();

            if (interaction != null)
            {
                interaction.HandleDeselect();
            }
        }
    }
    public void CancelCurrentSelection()
    {
        while (selectedCards.Count > 0)
        {
            DeselectCard(selectedCards[0]);
        }
    }
    public void ClearSelection()
    {
        selectedCards.Clear();
        totalCardValue = 0;
    }

    public List<string> GetSaveData()
    {
        List<string> data = new();

        foreach (GameObject card in handCards)
        {
            CardSO so =
                card.GetComponent<CardDisplay>().cardScriptableObject;

            data.Add(so.name);
        }

        return data;
    }

    public void LoadHand(List<string> data)
    {
        foreach (GameObject card in handCards)
            Destroy(card);

        handCards.Clear();

        foreach (string cardName in data)
        {
            CardSO card =
                Resources.Load<CardSO>("CardSO/" + cardName);
            if (card == null)
            {
                card = Resources.Load<CardSO>("RewardCardSO/" + cardName);
            }

            AddCardToHand(card);
        }
    }
    public void LockHighestCard()
    {
        UnlockCard();

        GameObject highest = null;
        int highestValue = -1;

        foreach (GameObject obj in handCards)
        {
            if (obj == null)
                continue;

            CardDisplay display = obj.GetComponent<CardDisplay>();

            if (display == null)
                continue;

            if (display.cardScriptableObject.value > highestValue)
            {
                highestValue = display.cardScriptableObject.value;
                highest = obj;
            }
        }

        if (highest == null)
            return;

        LockedCard = highest;

        CardInteraction interaction =
            highest.GetComponent<CardInteraction>();

        if (interaction != null)
            interaction.IsLocked = true;
    }
    public void UnlockCard()
    {
        if (LockedCard == null)
            return;

        CardInteraction interaction =
            LockedCard.GetComponent<CardInteraction>();

        if (interaction != null)
            interaction.IsLocked = false;

        LockedCard = null;
    }
    public CardSO ReturnRandomSelectedCard()
    {
        if (selectedCards.Count <= 1)
            return null;

        int index = UnityEngine.Random.Range(0, selectedCards.Count);

        GameObject obj = selectedCards[index];

        CardSO card =
            obj.GetComponent<CardDisplay>().cardScriptableObject;

        selectedCards.RemoveAt(index);

        handCards.Add(obj);

        obj.transform.DOKill();
        obj.transform.localScale = Vector3.one;
        obj.transform.rotation = Quaternion.identity;

        if (obj.TryGetComponent<CardInteraction>(out var interact))
            interact.HandleDeselect();

        if (obj.TryGetComponent<CardDisplay>(out var display))
            display.SetSortingOrder(10 + handCards.Count);

        RepositionAllCards(null);

        return card;
    }
}
