using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameEndPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI winnerText;

    private void OnEnable()
    {
        if (stateManager?.CurrentGameState?.Players == null || stateManager?.CurrentGameState?.Factions == null)
        {
            winnerText.text = "게임 결과를 불러올 수 없습니다.";
            return;
        }

        int highestScore = int.MinValue;
        foreach (var player in stateManager.CurrentGameState.Players)
        {
            Faction faction = stateManager.GetFactionById(player.FactionId);
            if (faction.Score > highestScore)
            {
                highestScore = faction.Score;
            }
        }

        var winners = new List<Player>();
        foreach (var player in stateManager.CurrentGameState.Players)
        {
            Faction faction = stateManager.GetFactionById(player.FactionId);
            if (faction.Score == highestScore)
            {
                winners.Add(player);
            }
        }

        if (winners.Count == 1)
        {
            winnerText.text = $"{winners[0].Name}님이 {highestScore}점으로 승리하셨습니다!";
        }
        else
        {
            string names = string.Join(", ", winners.ConvertAll(w => w.Name));
            winnerText.text = $"{names}님이 {highestScore}점으로 공동 우승하셨습니다!";
        }
    }
}