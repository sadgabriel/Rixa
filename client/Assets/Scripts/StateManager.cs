using UnityEngine;
using System;

public enum UIState
{
    IDLE,

    APP_LOBBY,
    GAME_LOBBY,

    GAME_CONTEXT_INPUT,
    GAME_FACTION_CONCEPT_INPUT,
    GAME_FACTION_FLAW_INPUT,

    GAME_CONTEXT_SETUP,
    GAME_ATTACK,
    GAME_DEFENSE,

    GAME_COMPETITION_ANALYZE,
    GAME_COMPETITION_NARRATE,

    GAME_END,
}

public class StateManager : MonoBehaviour
{
    private static StateManager instance;
    public static StateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StateManager>();
            }
            return instance;
        }
    }

    private GameClient gameClient;
    private ClientState currentClientState = null;
    private LobbyState currentLobbyState = null;
    private GameState currentGameState = null;
    [SerializeField] private UIState currentUIState = UIState.IDLE;
    
    public event Action<UIState> OnUIStateUpdated;

    public ClientState CurrentClientState
    {
        get { return currentClientState; }
        private set { currentClientState = value; }
    }

    public LobbyState CurrentLobbyState
    {
        get { return currentLobbyState; }
        private set { currentLobbyState = value; }
    }

    public GameState CurrentGameState
    {
        get { return currentGameState; }
        private set { currentGameState = value; }
    }

    public UIState CurrentUIState
    {
        get { return currentUIState; }
        private set
        {
            currentUIState = value;
            OnUIStateUpdated?.Invoke(currentUIState);
        }
    }

    public Player MyPlayer
    {
        get
        {
            string playerId = CurrentClientState?.PlayerId;
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }
            return CurrentGameState?.Players?.Find(p => p.Id == playerId);
        }
    }

    public bool IsInGame()
    {
        string gameId = CurrentClientState?.GameId;
        if (string.IsNullOrEmpty(gameId) || CurrentGameState?.Id != gameId || MyPlayer == null)
        {
            return false;
        }
        return true;
    }

    public bool IsInLobby()
    {
        if (IsInGame())
        {
            return false;
        }

        return CurrentClientState != null && CurrentLobbyState != null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        gameClient = GameClient.Instance;
        gameClient.OnClientStateUpdated += HandleClientStateUpdated;
        gameClient.OnLobbyStateUpdated += HandleLobbyStateUpdated;
        gameClient.OnGameStateUpdated += HandleGameStateUpdated;
    }

    private void OnDestroy()
    {
        gameClient.OnClientStateUpdated -= HandleClientStateUpdated;
        gameClient.OnLobbyStateUpdated -= HandleLobbyStateUpdated;
        gameClient.OnGameStateUpdated -= HandleGameStateUpdated;

        if (instance == this)
        {
            instance = null;
        }
    }

    private UIState MapPhaseToUIState(string phase)
    {
        switch (phase)
        {
            case "lobby": return UIState.GAME_LOBBY;
            case "context_input": return UIState.GAME_CONTEXT_INPUT;
            case "faction_concept_input": return UIState.GAME_FACTION_CONCEPT_INPUT;
            case "faction_flaw_input": return UIState.GAME_FACTION_FLAW_INPUT;
            case "context_setup": return UIState.GAME_CONTEXT_SETUP;
            case "attack": return UIState.GAME_ATTACK;
            case "defense": return UIState.GAME_DEFENSE;
            case "competition_analyze": return UIState.GAME_COMPETITION_ANALYZE;
            case "competition_narrate": return UIState.GAME_COMPETITION_NARRATE;
            case "end": return UIState.GAME_END;
            default: return UIState.GAME_LOBBY;
        }
    }

    private void RecomputeUIState()
    {
        if (IsInGame())
        {
            CurrentUIState = MapPhaseToUIState(CurrentGameState.Phase);
        }
        else if (IsInLobby())
        {
            CurrentUIState = UIState.APP_LOBBY;
        }
        else
        {
            CurrentUIState = UIState.IDLE;
        }
    }

    private void HandleClientStateUpdated(ClientState state)
    {
        CurrentClientState = state;
        RecomputeUIState();
    }

    private void HandleLobbyStateUpdated(LobbyState state)
    {
        CurrentLobbyState = state;
        RecomputeUIState();
    }

    private void HandleGameStateUpdated(GameState state)
    {
        CurrentGameState = state;
        RecomputeUIState();
    }
}
