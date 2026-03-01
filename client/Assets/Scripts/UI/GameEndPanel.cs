using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI winnerText;

    private void OnEnable()
    {
        Player winner = null;
        Faction winnerFaction = null;
        int highestScore = int.MinValue;

        if (stateManager?.CurrentGameState?.Players == null || stateManager?.CurrentGameState?.Factions == null)
        {
            winnerText.text = "게임 결과를 불러올 수 없습니다.";
            return;
        }

        foreach (var player in stateManager.CurrentGameState.Players)
        {
            Faction faction = stateManager.GetFactionById(player.FactionId);
            if (faction.Score > highestScore)
            {
                highestScore = faction.Score;
                winner = player;
                winnerFaction = faction;
            }
        }
        winnerText.text = $"{winner.Name}님이 {highestScore}점으로 승리하셨습니다!";
    }
}