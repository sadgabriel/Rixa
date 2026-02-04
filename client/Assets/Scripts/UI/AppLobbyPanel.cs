using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppLobbyPanel : Panel
{
    [SerializeField] private GameObject gameRoomItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private List<GameRoomItem> activeRoomItems = new List<GameRoomItem>();
    private GameRoomItem selectedRoom = null;

    protected override void Start()
    {
        base.Start();
        joinGameButton.interactable = false;
        RefreshLobbyList();
    }

    public void RefreshLobbyList()
    {
        ClearRoomList();

        LobbyState lobbyState = stateManager.CurrentLobbyState;
        
        if (lobbyState == null || lobbyState.Games == null)
        {
            Debug.LogWarning("LobbyState or Games is null");
            return;
        }

        foreach (var gameInfo in lobbyState.Games)
        {
            CreateRoomItem(gameInfo);
        }

        selectedRoom = null;
        joinGameButton.interactable = false;
    }

    private void CreateRoomItem(LobbyGameInfo gameInfo)
    {
        GameObject newItem = Instantiate(gameRoomItemPrefab, contentParent);
        GameRoomItem itemComponent = newItem.GetComponent<GameRoomItem>();
        itemComponent.Setup(gameInfo, this);
        
        activeRoomItems.Add(itemComponent);
    }

    private void ClearRoomList()
    {
        foreach (var item in activeRoomItems)
        {
            Destroy(item.gameObject);
        }
        activeRoomItems.Clear();
    }

    public void OnRoomItemClicked(GameRoomItem clickedItem)
    {
        if (selectedRoom != null)
        {
            selectedRoom.SetSelected(false);
        }

        if (selectedRoom == clickedItem)
        {
            selectedRoom = null;
            joinGameButton.interactable = false;
        }
        else
        {
            selectedRoom = clickedItem;
            selectedRoom.SetSelected(true);
            joinGameButton.interactable = true;
        }
    }

    public void OnCreateGameClicked()
    {
        dialogManager.ShowCreateGameDialog(
            onConfirm: (gameName, playerName) =>
            {
                gameClient.CreateGame(gameName, playerName);
                dialogManager.CloseTopDialog();
            },
            onCancel: () =>
            {
                dialogManager.CloseTopDialog();
            }
        );
    }

    public void OnJoinGameClicked()
    {
        if (selectedRoom == null) return;

        dialogManager.ShowJoinGameDialog(
            onConfirm: (playerName) =>
            {
                gameClient.JoinGame(selectedRoom.RoomData.Id, playerName);
                dialogManager.CloseTopDialog();
            },
            onCancel: () =>
            {
                dialogManager.CloseTopDialog();
            }
        );
    }
}