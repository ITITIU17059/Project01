using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using DG.Tweening;
using System;
public class HandManager : MonoBehaviour
{
    public int maxHandSize = 8;
    [SerializeField] private int spacingValue;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform cardSpawnPoint;
    private bool isDiscardMode = false;
    private int discardTarget = 0;
    private int discardCurrent = 0;

    [SerializeField] private Transform playPreviewArea;
    private int totalCardValue;

    [NonSerialized] public List<GameObject> handCards = new();
    public List<GameObject> selectedCards = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spacingValue = Mathf.Max(maxHandSize, spacingValue);
        totalCardValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectCard(GameObject cardObject)
    {

        if (isDiscardMode)
        {
            SelectDiscardCard(cardObject);
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

    // Logic xếp các lá bài nằm chờ ở giữa bàn (Dàn hàng ngang đều nhau)
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

            // --- BỔ SUNG: Cập nhật Layer tăng dần cho các lá ở giữa bàn ---
            // Đặt mốc layer bắt đầu từ 100 để chắc chắn đè lên toàn bộ bài dưới tay (layer dưới 50)
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

            // Giao cho Display quản lý layer
            display.SetSortingOrder(i);

            // Giao cho Interaction quản lý di chuyển vật lý
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

        Debug.Log("Discard Target = " + targetValue);

        int total = 0;

        foreach (GameObject card in handCards)
        {
            total += card.GetComponent<CardDisplay>()
                         .cardScriptableObject.value;
        }

        if (total < targetValue)
        {
            BattleManager.Instance.FinishDiscard(false);
            return;
        }
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

        //     confirmDiscardButton.interactable =
        // discardCurrent >= discardTarget;

        RearrangeSelectedCards();

        Debug.Log($"Discard = {discardCurrent}/{discardTarget}");

        if (discardCurrent >= discardTarget)
        {
            FinishDiscard();
        }
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

        Debug.Log($"Discard = {discardCurrent}/{discardTarget}");
    }

    private void FinishDiscard()
    {
        foreach (GameObject card in selectedCards)
        {
            CardSO so = card.GetComponent<CardDisplay>().cardScriptableObject;

            GraveyardManager.Instance.AddToGraveyard(so);

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                card,
                BattleManager.Instance.graveyardSpawnPoint
            );
        }

        selectedCards = new List<GameObject>();

        selectedCards.Clear();
        totalCardValue = 0;

        discardCurrent = 0;
        discardTarget = 0;

        isDiscardMode = false;

        RepositionAllCards(null);

        BattleManager.Instance.FinishDiscard(true);
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
}
