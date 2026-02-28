using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class GameCompetitionFinishPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI AttackNameText;
    [SerializeField] private TextMeshProUGUI DefenseNameText;
    [SerializeField] private TextMeshProUGUI AttackDescriptionText;
    [SerializeField] private TextMeshProUGUI DefenseDescriptionText;
    [SerializeField] private TextMeshProUGUI AttackResultText;
    [SerializeField] private TextMeshProUGUI DefenseResultText;

    private void OnEnable()
    {
        Match myAttackMatch = stateManager.MyAttackMatch;
        Match myDefenseMatch = stateManager.MyDefenseMatch;

        if (myAttackMatch != null)
        {
            string DefenderPlayerName = stateManager.GetPlayerByFactionId(myAttackMatch.DefenderId)?.Name;
            AttackNameText.text = $"{DefenderPlayerName}에 대한 공격 결과";
            AttackDescriptionText.text = myAttackMatch.DisplayNarrative;
            AttackResultText.text = GetOutcomeText(myAttackMatch);
        }
        else
        {
            AttackNameText.text = "알 수 없는 공격 결과";
            AttackDescriptionText.text = "";
            AttackResultText.text = "";
        }

        if (myDefenseMatch != null)
        {
            string AttackerPlayerName = stateManager.GetPlayerByFactionId(myDefenseMatch.AttackerId)?.Name;
            DefenseNameText.text = $"{AttackerPlayerName}(으)로부터의 공격 결과";
            DefenseDescriptionText.text = myDefenseMatch.DisplayNarrative;
            DefenseResultText.text = GetOutcomeText(myDefenseMatch);
        }
        else
        {
            DefenseNameText.text = "알 수 없는 방어 결과";
            DefenseDescriptionText.text = "";
            DefenseResultText.text = "";
        }
    }

    private string GetOutcomeText(Match match)
    {
        if (match == null || string.IsNullOrEmpty(match.WinnerId))
        {
            return "알 수 없음";
        }

        if (match.WinnerId == match.AttackerId)
        {
            return $"공격 성공\n공격자 승점 +1\n방어자 자원 손실: {match.LostResource ?? "없음"}";
        }
        else
        {
            return "공격 실패";
        }
    }
}