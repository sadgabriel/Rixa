using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameClient : MonoBehaviour
{
    private static GameClient instance;
    public static GameClient Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameClient>();
            }
            return instance;
        }
    }
    private WsClient wsClient;

    public event Action<ClientState> OnClientStateUpdated;
    public event Action<LobbyState> OnLobbyStateUpdated;
    public event Action<GameState> OnGameStateUpdated;

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

        wsClient = WsClient.Instance;
        wsClient.OnMessage += HandleMessage;
        wsClient.OnError += HandleError;
    }

    private void OnDestroy()
    {
        if (wsClient != null)
        {
            wsClient.OnMessage -= HandleMessage;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    public void RequestClientState()
    {
        if (!IsConnected()) return;
        wsClient.Send("client.state", new { });
    }

    public void RequestLobbyState()
    {
        if (!IsConnected()) return;
        wsClient.Send("lobby.state", new { });
    }

    public void CreateGame(string gameName, string playerName)
    {
        if (!IsConnected()) return;
        wsClient.Send("lobby.createGame", new
        {
            gameName,
            playerName
        });
    }

    public void JoinGame(string gameId, string playerName)
    {
        if (!IsConnected()) return;
        wsClient.Send("lobby.joinGame", new
        {
            gameId,
            playerName
        });
    }

    public void LeaveGame()
    {
        if (!IsConnected()) return;
        wsClient.Send("game.leave", new { });
    }

    public void RequestGameState()
    {
        if (!IsConnected()) return;
        wsClient.Send("game.state", new { });
    }

    public void SetReady(bool ready)
    {
        if (!IsConnected()) return;
        Submit(ready ? "ready" : "unready", new { });
    }

    public void GameStart()
    {
        if (!IsConnected()) return;
        Submit("start", new { });
    }

    public void SubmitContext(string contextDescription)
    {
        if (!IsConnected()) return;
        Submit("context", new
        {
            contextDescription
        });
    }

    public void SubmitFactionConcept(string factionConceptDescription, string factionName)
    {
        if (!IsConnected()) return;
        Submit("factionConcept", new
        {
            factionConceptDescription,
            factionName
        });
    }

    public void SubmitFactionFlaw(string factionId, string factionFlawDescription)
    {
        if (!IsConnected()) return;
        Submit("factionFlaw", new
        {
            factionId,
            factionFlawDescription
        });
    }

    public void SubmitAttack(string attackDescription)
    {
        if (!IsConnected()) return;
        Submit("attack", new
        {
            attackDescription
        });
    }

    public void SubmitDefense(string defenseDescription)
    {
        if (!IsConnected()) return;
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

    private bool IsConnected()
    {
        if (wsClient == null || !wsClient.IsConnected)
        {
            Debug.LogWarning("Cannot send message: WebSocket not connected");
            return false;
        }
        return true;
    }

    private void HandleError(string errorMessage)
    {
        Debug.LogWarning($"Server error: {errorMessage}");
    }

    private void HandleMessage(string type, JToken data)
    {
        Debug.Log($"[GameClient] Received: {type}");
        
        try
        {
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
                case "error":
                    HandleError(data.ToString());
                    break;
                default:
                    Debug.LogWarning($"Unknown message type: {type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling message type '{type}': {e.Message}");
        }
    }

    private void HandleWelcomeMessage(JToken data)
    {
        ClientState clientState = null;
        LobbyState lobbyState = null;
        try
        {
            clientState = data["clientState"]?.ToObject<ClientState>();
            lobbyState = data["lobbyState"]?.ToObject<LobbyState>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse welcome message: {e.Message}");
        }

        if (clientState != null)
        {
            OnClientStateUpdated?.Invoke(clientState);
        }
        
        if (lobbyState != null)
        {
            OnLobbyStateUpdated?.Invoke(lobbyState);
        }
    }

    private void HandleClientStateMessage(JToken data)
    {
        ClientState clientState = null;
        try
        {
            clientState = data?.ToObject<ClientState>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse client state: {e.Message}");
        }

        if (clientState != null)
        {
            OnClientStateUpdated?.Invoke(clientState);
        }
    }

    private void HandleLobbyStateMessage(JToken data)
    {
        LobbyState lobbyState = null;
        try
        {
            lobbyState = data?.ToObject<LobbyState>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse lobby state: {e.Message}");
        }

        if (lobbyState != null)
        {
            OnLobbyStateUpdated?.Invoke(lobbyState);
        }
    }

    private void HandleGameStateMessage(JToken data)
    {
        GameState gameState = null;
        
        try
        {
            gameState = data?.ToObject<GameState>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse game state: {e.Message}");
            Debug.LogError($"Data: {data}");
            return;
        }
        
        if (gameState != null)
        {            
            OnGameStateUpdated?.Invoke(gameState);
        }
    }
}