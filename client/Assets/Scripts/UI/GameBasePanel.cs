using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameBasePanel : PersistentPanel
{
    [SerializeField] private Transform playerDataLeftContainer;
    [SerializeField] private Transform playerDataRightContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private GameObject playerDataItemPrefab;

    private List<PlayerDataItem> playerDataItems = new List<PlayerDataItem>();

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame && !dialogManager.IsDialogOpen)
            {
                mainButton.onClick.Invoke();    
            }
        }
    }

    private void OnEnable()
    {
        RefreshPlayerData();

        if (stateManager != null)
        {
            SetupUIForState(stateManager.CurrentUIState);
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

    private void HandleUIStateUpdated(UIState newState, bool stateChanged)
    {
        if (!stateChanged) return;
        
        SetupUIForState(newState);
    }

    private void HandleGameStateUpdated(GameState gameState)
    {
        if (gameState?.Players == null) return;

        if (gameState.Players.Count != playerDataItems.Count)
        {
            RefreshPlayerData();
        }
        
        UpdateUIForState(stateManager.CurrentUIState);
    }

    private void SetupUIForState(UIState state)
    {
        SetupExitButton(state);
        SetupMainButton(state);
        SetupInputField(state);
    }

    private void UpdateUIForState(UIState state)
    {
        UpdateMainButton(state);
        UpdateInputField(state);
    }

    private void SetupExitButton(UIState state)
    {
        ExitButton.gameObject.SetActive(state == UIState.GAME_LOBBY);
    }

    private void SetupMainButton(UIState state)
    {
        mainButton.onClick.RemoveAllListeners();
        mainButton.interactable = true;

        var buttonText = mainButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "제출";

        switch (state)
        {
            case UIState.GAME_LOBBY:
                SetupLobbyButton(buttonText);
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
            case UIState.GAME_CONTEXT_SETUP_FINISH:
                buttonText.text = "준비 완료";
                mainButton.onClick.AddListener(OnContextSetupFinishButtonClicked);
                break;
            case UIState.GAME_ATTACK:
                mainButton.onClick.AddListener(OnSubmitAttackButtonClicked);
                break;
            case UIState.GAME_DEFENSE:
                mainButton.onClick.AddListener(OnSubmitDefenseButtonClicked);
                break;
            case UIState.GAME_COMPETITION_FINISH:
                buttonText.text = "준비 완료";
                mainButton.onClick.AddListener(OnCompetitionFinishButtonClicked);
                break;
            default:
                mainButton.interactable = false;
                break;
        }
    }

    private void SetupLobbyButton(TextMeshProUGUI buttonText)
    {
        Player myPlayer = stateManager.MyPlayer;
        
        if (stateManager.IsFirst())
        {
            buttonText.text = "게임 시작";
            mainButton.interactable = stateManager.CanGameStart();
            mainButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            bool isReady = myPlayer?.Ready ?? false;
            buttonText.text = isReady ? "준비 취소" : "준비";
            mainButton.onClick.AddListener(OnReadyButtonClicked);
        }
    }

    private void SetupInputField(UIState state)
    {
        inputField.text = string.Empty;
        inputField.interactable = true;

        switch (state)
        {
            case UIState.GAME_CONTEXT_INPUT:
                if (!stateManager.IsLeader())
                {
                    inputField.interactable = false;
                }
                break;
            case UIState.GAME_FACTION_CONCEPT_INPUT:
            case UIState.GAME_FACTION_FLAW_INPUT:
            case UIState.GAME_ATTACK:
            case UIState.GAME_DEFENSE:
                break;
            default:
                inputField.interactable = false;
                break;
        }
    }

    private void UpdateMainButton(UIState state)
    {
        if (state == UIState.GAME_LOBBY)
        {
            UpdateLobbyButton();
            return;
        }
        
        if (state == UIState.GAME_CONTEXT_SETUP_FINISH || state == UIState.GAME_COMPETITION_FINISH)
        {
            UpdateReadyButton(state);
            return;
        }
        
        UpdateSubmitButton(state);
    }

    private void UpdateLobbyButton()
    {
        Player myPlayer = stateManager.MyPlayer;
        var buttonText = mainButton.GetComponentInChildren<TextMeshProUGUI>();
        
        mainButton.onClick.RemoveAllListeners();
        mainButton.interactable = true;
        
        if (stateManager.IsFirst())
        {
            buttonText.text = "게임 시작";
            mainButton.interactable = stateManager.CanGameStart();
            mainButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            bool isReady = myPlayer?.Ready ?? false;
            buttonText.text = isReady ? "준비 취소" : "준비";
            mainButton.onClick.AddListener(OnReadyButtonClicked);
        }
    }

    private void UpdateReadyButton(UIState state)
    {
        GameState gameState = stateManager.CurrentGameState;
        if (gameState?.Players == null) return;
        
        var buttonText = mainButton.GetComponentInChildren<TextMeshProUGUI>();
        
        int readyCount = 0;
        int totalCount = gameState.Players.Count;
        
        foreach (var player in gameState.Players)
        {
            bool isReady = state == UIState.GAME_CONTEXT_SETUP_FINISH
                ? player.ContextSetupFinishReady
                : player.CompetitionFinishReady;
                
            if (isReady) readyCount++;
        }
        
        buttonText.text = $"다음 ({readyCount}/{totalCount})";
        
        bool hasMyPlayerReady = state == UIState.GAME_CONTEXT_SETUP_FINISH
            ? (stateManager.MyPlayer?.ContextSetupFinishReady ?? false)
            : (stateManager.MyPlayer?.CompetitionFinishReady ?? false);
            
        mainButton.interactable = !hasMyPlayerReady;
    }

    private void UpdateSubmitButton(UIState state)
    {
        bool hasSubmitted = HasAlreadySubmitted(state);
        mainButton.interactable = !hasSubmitted;
    }

    private void UpdateInputField(UIState state)
    {
        bool hasSubmitted = HasAlreadySubmitted(state);
        
        inputField.interactable = !hasSubmitted;
        
        if (hasSubmitted)
        {
            inputField.text = GetSubmittedText(state);
        }
    }

    private bool HasAlreadySubmitted(UIState state)
    {
        Faction myFaction = stateManager.MyFaction;
        Faction anotherFaction = stateManager.AnotherFaction;
        Match myAttackMatch = stateManager.MyAttackMatch;
        Match myDefenseMatch = stateManager.MyDefenseMatch;

        switch (state)
        {
            case UIState.GAME_CONTEXT_INPUT:
                return !stateManager.IsLeader() ||stateManager.CurrentGameState?.Context?.RawContextDescription != null;
            case UIState.GAME_FACTION_CONCEPT_INPUT:
                return myFaction != null && !string.IsNullOrEmpty(myFaction.RawConcept);
            case UIState.GAME_FACTION_FLAW_INPUT:
                return anotherFaction != null && !string.IsNullOrEmpty(anotherFaction.RawFlaw);
            case UIState.GAME_ATTACK:
                return myAttackMatch != null && !string.IsNullOrEmpty(myAttackMatch.AttackDescription);
            case UIState.GAME_DEFENSE:
                return myDefenseMatch != null && !string.IsNullOrEmpty(myDefenseMatch.DefenseDescription);
            default:
                return true;
        }
    }

    private string GetSubmittedText(UIState state)
    {
        Faction myFaction = stateManager.MyFaction;
        Faction anotherFaction = stateManager.AnotherFaction;
        Match myAttackMatch = stateManager.MyAttackMatch;
        Match myDefenseMatch = stateManager.MyDefenseMatch;

        switch (state)
        {
            case UIState.GAME_FACTION_CONCEPT_INPUT:
                return myFaction?.RawConcept ?? string.Empty;
            case UIState.GAME_FACTION_FLAW_INPUT:
                return anotherFaction?.RawFlaw ?? string.Empty;
            case UIState.GAME_ATTACK:
                return myAttackMatch?.AttackDescription ?? string.Empty;
            case UIState.GAME_DEFENSE:
                return myDefenseMatch?.DefenseDescription ?? string.Empty;
            default:
                return string.Empty;
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

    public void OnExitButtonClicked()
    {
        audioManager.PlayButtonClick();
        dialogManager.ShowConfirmationDialog(
            "게임에서 나가시겠습니까?",
            onConfirm: () =>
            {
                gameClient.LeaveGame();
                dialogManager.CloseTopDialog();
            }
        );
    }

    private void OnReadyButtonClicked()
    {
        audioManager.PlayButtonClick();
        Player myPlayer = stateManager.MyPlayer;
        bool isReady = myPlayer?.Ready ?? false;
        
        gameClient.SetReady(!isReady);
    }

    private void OnStartButtonClicked()
    {
        audioManager.PlayButtonClick();
        gameClient.GameStart();
    }

    private void OnSubmitContextButtonClicked()
    {
        audioManager.PlayButtonClick();
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
        audioManager.PlayButtonClick();
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
            }
        );
    }

    private void OnSubmitFactionFlawButtonClicked()
    {
        audioManager.PlayButtonClick();
        string flaw = inputField.text.Trim();
        if (string.IsNullOrEmpty(flaw))
        {
            Debug.LogWarning("Faction flaw input is empty");
            return;
        }

        gameClient.SubmitFactionFlaw(stateManager.AnotherFaction.Id, flaw);
    }

    private void OnContextSetupFinishButtonClicked()
    {
        audioManager.PlayButtonClick();
        gameClient.SetContextSetupFinishReady();
    }

    private void OnSubmitAttackButtonClicked()
    {
        audioManager.PlayButtonClick();
        string attack = inputField.text.Trim();
        if (string.IsNullOrEmpty(attack))
        {
            Debug.LogWarning("Attack input is empty");
            return;
        }
        gameClient.SubmitAttack(attack);
    }

    private void OnSubmitDefenseButtonClicked()
    {
        audioManager.PlayButtonClick();
        string defense = inputField.text.Trim();
        if (string.IsNullOrEmpty(defense))
        {
            Debug.LogWarning("Defense input is empty");
            return;
        }
        gameClient.SubmitDefense(defense);
    }

    private void OnCompetitionFinishButtonClicked()
    {
        audioManager.PlayButtonClick();
        gameClient.SetCompetitionFinishReady();
    }
}