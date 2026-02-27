using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameAttackPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;
    private void OnEnable()
    {
        Match myAttackMatch = stateManager.MyAttackMatch;
        Player defenderPlayer = stateManager.GetPlayerByFactionId(myAttackMatch.DefenderId);
        Faction defenderFaction = stateManager.GetFactionById(myAttackMatch.DefenderId);

        if (defenderPlayer != null && defenderFaction != null)
        {
            nameText.text = $"{defenderPlayer.Name}의 진영, {defenderFaction.Name}을(를) 공격합니다";
            factionDescriptionText.text = defenderFaction.Description;
        }
        else
        {
            nameText.text = "알 수 없는 방어자를 공격합니다";
            factionDescriptionText.text = "";
        }
    }
}