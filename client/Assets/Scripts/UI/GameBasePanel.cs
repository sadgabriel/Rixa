using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

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

        if (stateManager != null)
        {
            ConfigureMainButton(stateManager.CurrentUIState);
        }

        stateManager.OnGameStateUpdated += HandleGameStateUpdated;
        stateManager.OnUIStateUpdated += HandleUIStateUpdated;
    }

    private void OnDisable()
    {
        stateManager.OnGameStateUpdated -= HandleGameStateUpdated;
        stateManager.OnUIStateUpdated -= HandleUIStateUpdated;
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

        GameState gameState = stateManager.CurrentGameState;
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

    private void HandleUIStateUpdated(UIState newState)
    {
        ConfigureMainButton(newState);
    }

    private void ConfigureMainButton(UIState state)
    {
        switch (state)
        {
            case UIState.GAME_LOBBY:
                SetupReadyButton();
                break;
            case UIState.GAME_CONTEXT_INPUT:
                SetupSubmitContextButton();
                break;
            default:
                mainButton.gameObject.SetActive(false);
                break;
        }
    }

    private void SetupReadyButton()
    {
        mainButton.gameObject.SetActive(true);
        mainButton.onClick.RemoveAllListeners();
        
        Player myPlayer = stateManager.MyPlayer;
        bool isReady = myPlayer?.Ready ?? false;
        
        var buttonText = mainButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        buttonText.text = isReady ? "준비 취소" : "준비";

        mainButton.onClick.AddListener(OnReadyButtonClicked);
    }
    private void OnReadyButtonClicked()
    {
        Player myPlayer = stateManager.MyPlayer;
        bool isReady = myPlayer?.Ready ?? false;
        
        gameClient.SetReady(!isReady);
    }

    private void SetupSubmitContextButton()
    {
        mainButton.gameObject.SetActive(true);
        mainButton.onClick.RemoveAllListeners();

        var buttonText = mainButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        buttonText.text = "제출";

        mainButton.onClick.AddListener(OnSubmitContextButtonClicked);
    }

    private void OnSubmitContextButtonClicked()
    {
        string context = inputField.text.Trim();
        if (string.IsNullOrEmpty(context))
        {
            Debug.LogWarning("Context input is empty");
            return;
        }

        gameClient.SubmitContext(context);
    }
}
