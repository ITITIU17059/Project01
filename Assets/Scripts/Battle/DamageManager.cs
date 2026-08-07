using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DamageManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI first_number_player;
    [SerializeField] private TextMeshProUGUI second_number_player;
    [SerializeField] private TextMeshProUGUI sign_player;
    [SerializeField] private Image icon_player;
    [SerializeField] private TextMeshProUGUI first_number_card;
    [SerializeField] private TextMeshProUGUI second_number_card;
    [SerializeField] private TextMeshProUGUI sign_card;
    [SerializeField] private Image icon_card;
    [SerializeField] private TextMeshProUGUI first_number_heal;
    [SerializeField] private TextMeshProUGUI second_number_heal;
    [SerializeField] private TextMeshProUGUI sign_heal;
    [SerializeField] private Image icon_heal;
    [SerializeField] private TextMeshProUGUI first_number_heal_boss;
    [SerializeField] private TextMeshProUGUI second_number_heal_boss;
    [SerializeField] private TextMeshProUGUI sign_heal_boss;
    [SerializeField] private Image icon_heal_boss;
    [SerializeField] private TextMeshProUGUI first_number_attack_boss;
    [SerializeField] private TextMeshProUGUI second_number_attack_boss;
    [SerializeField] private TextMeshProUGUI sign_attack_boss;
    [SerializeField] private Image icon_attack_boss;

    public IEnumerator ShowTakenDamage(int damage)
    {
        yield return StartCoroutine(CalculateNumber(first_number_player, second_number_player, sign_player, icon_player, damage));
    }

    public IEnumerator ShowAdditionCard(int amount)
    {
        yield return StartCoroutine(CalculateNumber(first_number_card, second_number_card, sign_card, icon_card, amount));
    }

    public IEnumerator ShowHealCard(int amount)
    {
        yield return StartCoroutine(CalculateNumber(first_number_heal, second_number_heal, sign_heal, icon_heal, amount));
    }

    public IEnumerator ShowBossHeal(int amount)
    {
        yield return StartCoroutine(CalculateNumber(first_number_heal_boss, second_number_heal_boss, sign_heal_boss, icon_heal_boss, amount));
    }

    public IEnumerator ShowBossAttack(int amount)
    {
        yield return StartCoroutine(CalculateNumber(first_number_attack_boss, second_number_attack_boss, sign_attack_boss, icon_attack_boss, amount));
    }


    private void AnimateNumber(TextMeshProUGUI numberText)
    {
        numberText.rectTransform.DOLocalMoveY(5f, 0.4f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            numberText.rectTransform.DOLocalMoveY(0f, 0.2f);
        });
    }

    private IEnumerator TurnOffText(TextMeshProUGUI first_number_player, TextMeshProUGUI second_number_player
    , TextMeshProUGUI sign, Image icon)
    {
        yield return new WaitForSeconds(1f);

        first_number_player.gameObject.SetActive(false);
        second_number_player.gameObject.SetActive(false);
        sign.gameObject.SetActive(false);
        icon.gameObject.SetActive(false);
    }

    private IEnumerator CalculateNumber(TextMeshProUGUI first_number_player, TextMeshProUGUI second_number_player,
    TextMeshProUGUI sign, Image icon, int number)
    {
        sign.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);

        if (number / 10 == 0)
        {
            first_number_player.text = number.ToString();
            first_number_player.gameObject.SetActive(true);
            AnimateNumber(first_number_player);
            yield return StartCoroutine(TurnOffText(first_number_player, second_number_player, sign, icon));
            yield break;
        }

        int remain = 0;
        remain = number % 10;
        number /= 10;
        second_number_player.text = remain.ToString();
        remain = number % 10;
        first_number_player.text = remain.ToString();
        first_number_player.gameObject.SetActive(true);
        second_number_player.gameObject.SetActive(true);

        AnimateNumber(first_number_player);

        yield return new WaitForSeconds(0.3f);

        AnimateNumber(second_number_player);

        yield return StartCoroutine(TurnOffText(first_number_player, second_number_player, sign, icon));
    }
}
