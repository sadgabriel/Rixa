using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameDefensePanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI attackText;
    private void OnEnable()
    {
        Match myDefenseMatch = stateManager.MyDefenseMatch;
        Player attackerPlayer = stateManager.GetPlayerByFactionId(myDefenseMatch.AttackerId);
        Faction AttackerFaction = stateManager.GetFactionById(myDefenseMatch.AttackerId);

        if (attackerPlayer != null && AttackerFaction != null)
        {
            nameText.text = $"{attackerPlayer.Name}의 진영, {AttackerFaction.Name}(으)로부터 공격받습니다";
            attackText.text = myDefenseMatch.AttackDescription;
        }
        else
        {
            nameText.text = "알 수 없는 공격자로부터 공격받습니다";
            attackText.text = "";
        }
    }
}