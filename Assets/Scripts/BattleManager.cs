using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; }

    [Header("References")]
    [SerializeField] private TarvernDeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private Button confirmButton;

    [Header("Spawn Points")]
    [SerializeField] private Transform tavernSpawnPoint;
    public Transform graveyardSpawnPoint;

    [Header("Battle Stats")]
    public int currentShield = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(BattleState.StartBattle);
    }

    #region State Machine

    public void ChangeState(BattleState newState)
    {
        CurrentState = newState;

        switch (CurrentState)
        {
            case BattleState.StartBattle:
                StartBattle();
                break;

            case BattleState.PlayerTurn:
                StartPlayerTurn();
                break;

            case BattleState.BossTurn:
                StartBossTurn();
                break;

            case BattleState.ResolveAttack:
                ResolveBossAttack();
                break;

            case BattleState.CheckBattle:
                CheckBattle();
                break;

            case BattleState.Victory:
                Victory();
                break;

            case BattleState.Defeat:
                Defeat();
                break;
        }
    }

    #endregion

    #region Battle Flow

    private void StartBattle()
    {
        currentShield = 0;

        BossManager.Instance.Initialize();

        StartCoroutine(deckManager.AddCardFromFirst());
    }

    private void StartPlayerTurn()
    {
        Debug.Log("===== PLAYER TURN =====");

        deckManager.DrawCard(handManager);

        confirmButton.interactable = true;
    }

    private void StartBossTurn()
    {
        Debug.Log("===== BOSS TURN =====");

        ChangeState(BattleState.ResolveAttack);
    }

    private void ResolveBossAttack()
    {
        int damage = Mathf.Max(
            0,
            BossManager.Instance.CurrentATK - currentShield
        );

        currentShield = 0;

        if (damage == 0)
        {
            FinishDiscard(true);
            return;
        }

        handManager.StartDiscardPhase(damage);
    }

    private void CheckBattle()
    {
        confirmButton.interactable = false;

        if (BossManager.Instance.IsDead())
        {
            BossFXManager.Instance.PlayDeathFX(
                BossManager.Instance.BossTransform
            );

            bool hasNextBoss = BossManager.Instance.LoadNextBoss();

            if (!hasNextBoss)
            {
                ChangeState(BattleState.Victory);
                return;
            }

            ChangeState(BattleState.PlayerTurn);
            return;
        }

        ChangeState(BattleState.BossTurn);
    }

    #endregion

    #region Player Action

    public void ConfirmPlayCards()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        if (handManager.selectedCards.Count == 0)
            return;

        SoundManager.instance?.PlaySound2D("CardPlay");
        ResolveSelectedCards();
    }

    private void ResolveSelectedCards()
    {
        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent(out CardDisplay display))
            {
                OnCardPlayed(display.cardScriptableObject);
            }
        }

        foreach (GameObject cardObject in handManager.selectedCards)
        {
            if (cardObject.TryGetComponent(out CardDisplay display))
            {
                GraveyardManager.Instance.AddToGraveyard(display.cardScriptableObject);
            }

            CardFXManager.Instance.PlayAnimateToGraveyardFX(
                cardObject,
                graveyardSpawnPoint
            );
        }

        handManager.ClearSelection();

        ChangeState(BattleState.CheckBattle);
    }

    public void FinishDiscard(bool success)
    {
        if (success)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.Defeat);
    }

    public void EndTurn()
    {
        if (CurrentState != BattleState.PlayerTurn)
            return;

        ChangeState(BattleState.BossTurn);
    }

    #endregion

    #region Card Effect

    public void OnCardPlayed(CardSO cardData)
    {
        int value = cardData.value;

        switch (cardData.suit.ToString())
        {
            case "Hearts":
                HealDeck(value);
                AttackBoss(value);
                break;

            case "Diamonds":
                AttackBoss(value);
                DrawBonusCards(value);
                break;

            case "Spades":
                AddShield(value);
                AttackBoss(value);
                break;

            case "Clubs":
                AttackBoss(value * 2);
                break;
        }
    }

    private void AttackBoss(int damage)
    {
        BossManager.Instance.TakeDamage(damage);

        Debug.Log("Boss HP : " + BossManager.Instance.CurrentHP);
    }

    private void AddShield(int value)
    {
        currentShield += value;

        Debug.Log("Shield : " + currentShield);
    }

    private void DrawBonusCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            deckManager.DrawCard(handManager);
    }

    private void HealDeck(int amount)
    {
        if (GraveyardManager.Instance == null)
            return;

        List<CardSO> healedCards =
            GraveyardManager.Instance.PopRandomCards(amount);

        if (healedCards.Count == 0)
            return;

        foreach (CardSO card in healedCards)
            deckManager.allCards.Add(card);

        CardFXManager.Instance.PlayHealDeckFX(
            healedCards,
            graveyardSpawnPoint,
            tavernSpawnPoint
        );

        deckManager.ShuffleDeck();
    }

    #endregion

    #region End Battle

    private void Victory()
    {
        Debug.Log("VICTORY");
    }

    private void Defeat()
    {
        Debug.Log("DEFEAT");
    }

    #endregion
}