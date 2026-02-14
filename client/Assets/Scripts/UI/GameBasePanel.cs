using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameBasePanel : PersistentPanel
{
    [SerializeField] private Transform playerDataLeftContainer;
    [SerializeField] private Transform playerDataRightContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button mainButton;
    [SerializeField] private GameObject playerDataItemPrefab;

    private List<PlayerDataItem> playerDataItems = new List<PlayerDataItem>();

    private void OnEnable()
    {
        RefreshPlayerData();
        stateManager.OnGameStateUpdated += HandleGameStateUpdated;
    }

    private void OnDisable()
    {
        stateManager.OnGameStateUpdated -= HandleGameStateUpdated;
        ClearPlayerData();
    }

    private void HandleGameStateUpdated(GameState gameState)
    {
        if (gameState?.Players == null) return;

        if (gameState.Players.Count != playerDataItems.Count)
        {
            RefreshPlayerData();
        }
    }

    private void RefreshPlayerData()
    {
        ClearPlayerData();

        GameState gameState = StateManager.Instance.CurrentGameState;
        if (gameState?.Players == null) return;

        for (int i = 0; i < gameState.Players.Count; i++)
        {
            Player player = gameState.Players[i];
            Transform parentContainer = i % 2 == 0 ? playerDataLeftContainer : playerDataRightContainer;

            GameObject itemGO = Instantiate(playerDataItemPrefab, parentContainer);
            PlayerDataItem dataItem = itemGO.GetComponent<PlayerDataItem>();
            dataItem.Setup(player.Id);
            playerDataItems.Add(dataItem);
        }
    }

    private void ClearPlayerData()
    {
        foreach (var item in playerDataItems)
        {
            Destroy(item.gameObject);
        }
        playerDataItems.Clear();
    }
}
