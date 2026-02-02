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

        createGameButton.onClick.AddListener(OnCreateGameClicked);
        joinGameButton.onClick.AddListener(OnJoinGameClicked);

        joinGameButton.interactable = false;
    }

    private void OnEnable()
    {
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

    private void OnCreateGameClicked()
    {
        Debug.Log("게임 생성 버튼 클릭");
        
        // TODO: 게임 생성 요청
        // GameClient를 통해 서버에 게임 생성 요청
    }

    private void OnJoinGameClicked()
    {
        if (selectedRoom == null) return;

        string gameId = selectedRoom.RoomData.Id;
        Debug.Log($"게임 참여: {selectedRoom.RoomData.Name} (ID: {gameId})");
        
        // TODO: 게임 참여 요청
        // GameClient를 통해 서버에 게임 참여 요청
    }
}