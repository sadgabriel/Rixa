using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataItem : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI factionNameText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    private string playerId;

    private void Update()
    {
        Refresh();
    }

    public void Setup(string playerId)
    {
        this.playerId = playerId;
        Refresh();
    }

    public void Refresh()
    {
        playerNameText.text = "알 수 없음";
        factionNameText.text = "미정";
        scoreText.text = "점수: 0";

        if (string.IsNullOrEmpty(playerId)) return;

        Player player = StateManager.Instance.GetPlayerById(playerId);
        if (player == null) return;
        playerNameText.text = player.Name;

        Faction faction = StateManager.Instance.GetFactionById(player.FactionId);
        if (faction == null) return;
        factionNameText.text = faction.Name;
        scoreText.text = $"점수: {faction.Score}";
    }

    public void OnClick()
    {
        DialogManager dialogManager = DialogManager.Instance;

        Faction faction = StateManager.Instance.GetFactionById(StateManager.Instance.GetPlayerById(playerId)?.FactionId);
        dialogManager.ShowFactionDialog(faction?.Name ?? "미정", faction?.Description ?? "설명 없음", () =>
        {
            dialogManager.CloseTopDialog();
        });
    }
}