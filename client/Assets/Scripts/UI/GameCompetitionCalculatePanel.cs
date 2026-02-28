using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameCompetitionCalculatePanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI AttackAttackNameText;
    [SerializeField] private TextMeshProUGUI AttackDefenseNameText;
    [SerializeField] private TextMeshProUGUI DefenseAttackNameText;
    [SerializeField] private TextMeshProUGUI DefenseDefenseNameText;
    [SerializeField] private TextMeshProUGUI AttackAttackText;
    [SerializeField] private TextMeshProUGUI AttackDefenseText;
    [SerializeField] private TextMeshProUGUI DefenseAttackText;
    [SerializeField] private TextMeshProUGUI DefenseDefenseText;

    private void OnEnable()
    {
        Match myAttackMatch = stateManager.MyAttackMatch;
        Match myDefenseMatch = stateManager.MyDefenseMatch;

        if (myAttackMatch != null)
        {
            string DefenderPlayerName = stateManager.GetPlayerByFactionId(myAttackMatch.DefenderId)?.Name;
            AttackAttackNameText.text = $"{DefenderPlayerName}에 대한 공격";
            AttackDefenseNameText.text = $"{DefenderPlayerName}의 방어";
            AttackAttackText.text = myAttackMatch.AttackDescription;
            AttackDefenseText.text = myAttackMatch.DefenseDescription;
        }
        else
        {
            AttackAttackNameText.text = "알 수 없는 공격";
            AttackDefenseNameText.text = "알 수 없는 방어";
            AttackAttackText.text = "";
            AttackDefenseText.text = "";
        }

        if (myDefenseMatch != null)
        {
            string AttackerPlayerName = stateManager.GetPlayerByFactionId(myDefenseMatch.AttackerId)?.Name;
            DefenseAttackNameText.text = $"{AttackerPlayerName}(으)로부터의 공격";
            DefenseDefenseNameText.text = $"{AttackerPlayerName}에 대한 방어";
            DefenseAttackText.text = myDefenseMatch.AttackDescription;
            DefenseDefenseText.text = myDefenseMatch.DefenseDescription;
        }
        else
        {
            DefenseAttackNameText.text = "알 수 없는 공격";
            DefenseDefenseNameText.text = "알 수 없는 방어";
            DefenseAttackText.text = "";
            DefenseDefenseText.text = "";
        }
    }
}