using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameClient : MonoBehaviour
{
    [SerializeField] private WsClient wsClient;

    public event Action<ClientState> OnClientStateUpdate;
    public event Action<LobbyState> OnLobbyStateUpdate;
    public event Action<GameState> OnGameStateUpdate;

    private void Awake()
    {
        wsClient.OnMessage += HandleMessage;
    }

    private void OnDestroy()
    {
        wsClient.OnMessage -= HandleMessage;
    }

    private void HandleMessage(string type, JToken data)
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
        }
    }

    private void HandleWelcomeMessage(JToken data)
    {
        ClientState clientState = data["clientState"].ToObject<ClientState>();
        LobbyState lobbyState = data["lobbyState"].ToObject<LobbyState>();
        
        OnClientStateUpdate?.Invoke(clientState);
        OnLobbyStateUpdate?.Invoke(lobbyState);
    }

    private void HandleClientStateMessage(JToken data)
    {
        ClientState clientState = data.ToObject<ClientState>();
        OnClientStateUpdate?.Invoke(clientState);
    }

    private void HandleLobbyStateMessage(JToken data)
    {
        LobbyState lobbyState = data.ToObject<LobbyState>();
        OnLobbyStateUpdate?.Invoke(lobbyState);
    }

    private void HandleGameStateMessage(JToken data)
    {
        GameState gameState = data.ToObject<GameState>();
        OnGameStateUpdate?.Invoke(gameState);
    }
}
