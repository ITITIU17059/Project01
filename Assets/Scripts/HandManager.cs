using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using DG.Tweening;
using System;
public class HandManager : MonoBehaviour
{
    public int maxHandSize;
    [SerializeField] private int spacingValue;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform cardSpawnPoint;

    [SerializeField] private Transform playPreviewArea;
    private int totalCardValue;

    private List<GameObject> handCards = new();
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
        if (handCards.Contains(cardObject))
        {
            var cardObjectValue = cardObject.GetComponent<CardDisplay>().cardScriptableObject.value;
            if (selectedCards.Count != 0)
            {
                foreach (var card in selectedCards)
                {
                    var cardValue = card.GetComponent<CardDisplay>().cardScriptableObject.value;
                    if (cardObjectValue == 1) break;
                    if (cardValue != cardObjectValue || totalCardValue + cardObjectValue > 10)
                    {
                        print("Card can phai co cung value hoac di cung voi la Ace");
                        // Ép kích thước bài về chuẩn ngay khi vừa được chọn
                        cardObject.transform.DOKill();
                        cardObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
                        if (cardObject.TryGetComponent<CardInteraction>(out var interact))
                        {
                            interact.HandleDeselect();
                        }
                        return;
                    }
                }
            }

            handCards.Remove(cardObject);
            selectedCards.Add(cardObject);
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
        if (selectedCards.Contains(cardObject))
        {
            var cardObjectValue = cardObject.GetComponent<CardDisplay>().cardScriptableObject.value;
            selectedCards.Remove(cardObject);
            handCards.Add(cardObject);
            totalCardValue -= cardObjectValue;

            if (cardObject.TryGetComponent<CardInteraction>(out var interact))
            {
                interact.HandleDeselect();
            }

            if (cardObject.TryGetComponent<CardDisplay>(out var display))
            {
                display.SetSortingOrder(10 + handCards.Count);
            }

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
}
