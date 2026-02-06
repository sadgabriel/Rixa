using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameClient : MonoBehaviour
{
    public static GameClient Instance { get; private set; }
    private WsClient wsClient;

    public event Action<ClientState> OnClientStateUpdated;
    public event Action<LobbyState> OnLobbyStateUpdated;
    public event Action<GameState> OnGameStateUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        wsClient = WsClient.Instance;
        wsClient.OnMessage += HandleMessage;
    }

    private void OnDestroy()
    {
        wsClient.OnMessage -= HandleMessage;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RequestClientState()
    {
        wsClient.Send("client.state", new { });
    }

    public void RequestLobbyState()
    {
        wsClient.Send("lobby.state", new { });
    }

    public void CreateGame(string gameName, string playerName)
    {
        wsClient.Send("lobby.createGame", new
        {
            gameName,
            playerName
        });
    }

    public void JoinGame(string gameId, string playerName)
    {
        wsClient.Send("lobby.joinGame", new
        {
            gameId,
            playerName
        });
    }

    public void LeaveGame()
    {
        wsClient.Send("game.leave", new { });
    }

    public void RequestGameState()
    {
        wsClient.Send("game.state", new { });
    }

    public void SetReady(bool ready)
    {
        Submit(ready ? "ready" : "unready", new { });
    }

    public void SubmitContext(string contextDescription)
    {
        Submit("context", new
        {
            contextDescription
        });
    }

    public void SubmitFactionConcept(string factionConceptDescription, string factionName)
    {
        Submit("factionConcept", new
        {
            factionConceptDescription,
            factionName
        });
    }

    public void SubmitFactionFlaw(string factionFlawDescription)
    {
        Submit("factionFlaw", new
        {
            factionFlawDescription
        });
    }

    public void SubmitAttack(string attackDescription)
    {
        Submit("attack", new
        {
            attackDescription
        });
    }

    public void SubmitDefense(string defenseDescription)
    {
        Submit("defense", new
        {
            defenseDescription
        });
    }

    private void Submit(string kind, object payload)
    {
        wsClient.Send("game.submit", new
        {
            kind,
            payload
        });
    }

    private void HandleMessage(string type, JToken data)
    {
        Debug.Log($"Handling WebSocket Message: Type={type}, Data={data.ToString(Formatting.None)}");
        switch (type)
        {
            case "welcome":
                HandleWelcomeMessage(data);
                break;
            case "client.state":
                HandleClientStateMessage(data);
                break;
            case "lobby.state":
                HandleLobbyStateMessage(data);
                break;
            case "game.state":
                HandleGameStateMessage(data);
                break;
        }
    }

    private void HandleWelcomeMessage(JToken data)
    {
        ClientState clientState = data["clientState"].ToObject<ClientState>();
        LobbyState lobbyState = data["lobbyState"].ToObject<LobbyState>();

        OnClientStateUpdated?.Invoke(clientState);
        OnLobbyStateUpdated?.Invoke(lobbyState);
    }

    private void HandleClientStateMessage(JToken data)
    {
        ClientState clientState = data.ToObject<ClientState>();
        OnClientStateUpdated?.Invoke(clientState);
    }

    private void HandleLobbyStateMessage(JToken data)
    {
        LobbyState lobbyState = data.ToObject<LobbyState>();
        OnLobbyStateUpdated?.Invoke(lobbyState);
    }

    private void HandleGameStateMessage(JToken data)
    {
        GameState gameState = data.ToObject<GameState>();
        OnGameStateUpdated?.Invoke(gameState);
    }
}
