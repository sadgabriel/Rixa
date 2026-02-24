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

        if (stateManager != null)
        {
            ConfigureMainButton(stateManager.CurrentUIState);
            ConfigureInputField(stateManager.CurrentUIState);
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

    private void HandleUIStateUpdated(UIState newState, bool stateChanged)
    {
        ConfigureMainButton(newState, stateChanged);
        ConfigureInputField(newState, stateChanged);
    }

    private void ConfigureMainButton(UIState state, bool stateChanged = true)
    {
        Faction myFaction = stateManager.MyFaction;
        var buttonText = mainButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (stateChanged)
        {
            mainButton.interactable = true;
            mainButton.onClick.RemoveAllListeners();
            buttonText.text = "제출";
            switch (state)
            {
                case UIState.GAME_LOBBY:
                    Player myPlayer = stateManager.MyPlayer;
                    bool isReady = myPlayer?.Ready ?? false;
                    buttonText.text = isReady ? "준비 취소" : "준비";
                    mainButton.onClick.AddListener(OnReadyButtonClicked);
                    break;
                case UIState.GAME_CONTEXT_INPUT:
                    if (!stateManager.IsLeader())
                    {
                        mainButton.interactable = false;
                    }
                    mainButton.onClick.AddListener(OnSubmitContextButtonClicked);
                    break;
                case UIState.GAME_FACTION_CONCEPT_INPUT:
                    mainButton.onClick.AddListener(OnSubmitFactionConceptButtonClicked);
                    break;
                case UIState.GAME_FACTION_FLAW_INPUT:
                    mainButton.onClick.AddListener(OnSubmitFactionFlawButtonClicked);
                    break;
                default:
                    mainButton.interactable = false;
                    break;
            }
        } else {
            switch (state)
            {
                case UIState.GAME_FACTION_CONCEPT_INPUT:
                    
                    if (myFaction != null && !string.IsNullOrEmpty(myFaction.RawConcept))
                    {
                        mainButton.interactable = false;
                    }
                    break;
                case UIState.GAME_FACTION_FLAW_INPUT:
                    if (myFaction != null && !string.IsNullOrEmpty(myFaction.RawFlaw))
                    {
                        mainButton.interactable = false;
                    }
                    break;
                
            }
        }
    }

    private void ConfigureInputField(UIState state, bool stateChanged = true)
    {
        Faction myFaction = stateManager.MyFaction;
        if (stateChanged)
        {
            inputField.interactable = true;
            inputField.text = string.Empty;

            switch (state)
            {
                case UIState.GAME_LOBBY:
                    inputField.interactable = false;
                    break;
                case UIState.GAME_CONTEXT_INPUT:
                    if (!stateManager.IsLeader())
                    {
                        inputField.interactable = false;
                    }
                    break;
                default:
                    break;
            }
        } else
        {
            switch (state)
            {
                case UIState.GAME_FACTION_CONCEPT_INPUT:
                    if (myFaction != null && !string.IsNullOrEmpty(myFaction.RawConcept))
                    {
                        inputField.text = myFaction.RawConcept;
                        inputField.interactable = false;
                    }
                    break;
                case UIState.GAME_FACTION_FLAW_INPUT:
                    if (myFaction != null && !string.IsNullOrEmpty(myFaction.RawFlaw))
                    {
                        inputField.text = myFaction.RawFlaw;
                        inputField.interactable = false;
                    }
                    break;
            }
        }
        
    }
    private void OnReadyButtonClicked()
    {
        Player myPlayer = stateManager.MyPlayer;
        bool isReady = myPlayer?.Ready ?? false;
        
        gameClient.SetReady(!isReady);
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

    private void OnSubmitFactionConceptButtonClicked()
    {
        string concept = inputField.text.Trim();
        if (string.IsNullOrEmpty(concept))
        {
            Debug.LogWarning("Faction concept input is empty");
            return;
        }

        dialogManager.ShowFactionNameInputDialog(
            onConfirm: factionName =>
            {
                if (string.IsNullOrEmpty(factionName))
                {
                    Debug.LogWarning("Faction name input is empty");
                    return;
                }
                gameClient.SubmitFactionConcept(concept, factionName);
                dialogManager.CloseTopDialog();
            },
            onCancel: () =>
            {
                dialogManager.CloseTopDialog();
            }
        );
    }

    private void OnSubmitFactionFlawButtonClicked()
    {
        string flaw = inputField.text.Trim();
        if (string.IsNullOrEmpty(flaw))
        {
            Debug.LogWarning("Faction flaw input is empty");
            return;
        }

        gameClient.SubmitFactionFlaw(flaw);
    }
}
