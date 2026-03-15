using UnityEngine;
using System;
using System.Linq;

public enum UIState
{
    NONE,
    IDLE,

    APP_LOBBY,
    GAME_LOBBY,

    GAME_CONTEXT_INPUT,
    GAME_FACTION_CONCEPT_INPUT,
    GAME_FACTION_FLAW_INPUT,

    GAME_CONTEXT_SETUP,
    GAME_CONTEXT_SETUP_FINISH,
    GAME_ATTACK,
    GAME_DEFENSE,

    GAME_COMPETITION_CALCULATE,
    GAME_COMPETITION_FINISH,

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
    
    public event Action<UIState, bool> OnUIStateUpdated;
    public event Action<ClientState> OnClientStateUpdated;
    public event Action<LobbyState> OnLobbyStateUpdated;
    public event Action<GameState> OnGameStateUpdated;

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
            UIState oldState = currentUIState;
            currentUIState = value;
            OnUIStateUpdated?.Invoke(currentUIState, oldState != currentUIState);
        }
    }

    public Player MyPlayer
    {
        get
        {
            if (currentClientState == null || string.IsNullOrEmpty(currentClientState.PlayerId))
            {
                return null;
            }
            
            if (currentGameState?.Players == null)
            {
                return null;
            }
            
            return currentGameState.Players.Find(p => p.Id == currentClientState.PlayerId);
        }
    }

    public Faction MyFaction
    {
        get
        {
            Player me = MyPlayer;
            if (me == null || string.IsNullOrEmpty(me.FactionId))
            {
                return null;
            }
            
            if (currentGameState?.Factions == null)
            {
                return null;
            }
            
            return currentGameState.Factions.Find(f => f.Id == me.FactionId);
        }
    }

    public Faction AnotherFaction
    {
        get
        {
            if (!IsInGame() || currentGameState.Players == null || currentGameState.Factions == null || currentGameState.SetupOffset == 0)
            {
                return null;
            }

            int myIndex = 0;
            while (myIndex < currentGameState.Players.Count && currentGameState.Players[myIndex].Id != MyPlayer.Id)
            {
                myIndex++;
            }

            if (myIndex < currentGameState.Players.Count)
            {
                int anotherIndex = (myIndex + currentGameState.SetupOffset) % currentGameState.Players.Count;
                Faction anotherFaction = GetFactionById(currentGameState.Players[anotherIndex].FactionId);
                return anotherFaction;
            }

            return null;
        }
    }

    public Match MyAttackMatch
    {
        get
        {
            if (!IsInGame() || currentGameState.Matches == null)
            {
                return null;
            }

            Faction myFaction = MyFaction;
            if (myFaction == null)
            {
                return null;
            }

            return currentGameState.Matches.Find(m => m.AttackerId == myFaction.Id);
        }
    }

    public Match MyDefenseMatch
    {
        get
        {
            if (!IsInGame() || currentGameState.Matches == null)
            {
                return null;
            }

            Faction myFaction = MyFaction;
            if (myFaction == null)
            {
                return null;
            }

            return currentGameState.Matches.Find(m => m.DefenderId == myFaction.Id);
        }
    }

    public bool IsInGame()
    {
        if (currentClientState == null || string.IsNullOrEmpty(currentClientState.GameId))
        {
            return false;
        }
        
        if (currentGameState == null || currentGameState.Id != currentClientState.GameId)
        {
            return false;
        }
        
        if (MyPlayer == null)
        {
            return false;
        }
        
        return true;
    }

    public bool IsInLobby()
    {
        if (currentClientState == null || currentLobbyState == null)
        {
            return false;
        }
        
        if (IsInGame())
        {
            return false;
        }
        
        return true;
    }

    public bool IsLeader()
    {
        if (!IsInGame())
        {
            return false;
        }

        if (MyPlayer?.Id == null || currentGameState?.LeadPlayerId == null || currentGameState.LeadPlayerId != MyPlayer.Id)
        {
            return false;
        }

        return true;
    }

    public Player GetPlayerById(string playerId)
    {
        if (playerId == null ||CurrentGameState == null || CurrentGameState.Players == null)
        {
            return null;
        }
        return CurrentGameState.Players.Find(p => p.Id == playerId);
    }

    public Faction GetFactionById(string factionId)
    {
        if (factionId == null || CurrentGameState == null || CurrentGameState.Factions == null)
        {
            return null;
        }
        return CurrentGameState.Factions.Find(f => f.Id == factionId);
    }

    public Player GetPlayerByFactionId(string factionId)
    {
        if (!IsInGame() || factionId == null)
        {
            return null;
        }

        return CurrentGameState.Players.Find(p => p.FactionId == factionId);
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
        if (gameClient != null)
        {
            gameClient.OnClientStateUpdated -= HandleClientStateUpdated;
            gameClient.OnLobbyStateUpdated -= HandleLobbyStateUpdated;
            gameClient.OnGameStateUpdated -= HandleGameStateUpdated;
        }

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
            case "context_setup_finish": return UIState.GAME_CONTEXT_SETUP_FINISH;
            case "attack": return UIState.GAME_ATTACK;
            case "defense": return UIState.GAME_DEFENSE;
            case "competition_analyze":
            case "competition_narrate": return UIState.GAME_COMPETITION_CALCULATE;
            case "competition_finish": return UIState.GAME_COMPETITION_FINISH;
            case "end": return UIState.GAME_END;
            default: return UIState.GAME_LOBBY;
        }
    }

    private void RecomputeUIState()
    {
        if (IsInGame())
        {
            if (string.IsNullOrEmpty(currentGameState?.Phase))
            {
                Debug.LogWarning("In game but phase is null, defaulting to GAME_LOBBY");
                CurrentUIState = UIState.GAME_LOBBY;
                return;
            }
            CurrentUIState = MapPhaseToUIState(currentGameState.Phase);
            return;
        }
        
        if (IsInLobby())
        {
            CurrentUIState = UIState.APP_LOBBY;
            return;
        }
        
        CurrentUIState = UIState.IDLE;
    }

    private void HandleClientStateUpdated(ClientState state)
    {
        CurrentClientState = state;
        RecomputeUIState();
        OnClientStateUpdated?.Invoke(state);
    }

    private void HandleLobbyStateUpdated(LobbyState state)
    {
        CurrentLobbyState = state;
        RecomputeUIState();
        OnLobbyStateUpdated?.Invoke(state);
    }

    private void HandleGameStateUpdated(GameState state)
    {
        CurrentGameState = state;
        RecomputeUIState();
        OnGameStateUpdated?.Invoke(state);
    }
}
