using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameRoomItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.7f, 0.9f, 1f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);

    [SerializeField] private Color waitingColor = Color.green;
    [SerializeField] private Color playingColor = Color.red;

    private Image backgroundImage;
    private AppLobbyPanel parentPanel;

    private LobbyGameInfo roomData;
    private bool isSelected = false;
    private bool isInteractable = true;

    public LobbyGameInfo RoomData => roomData;
    public bool IsSelected
    {
        get { return isSelected; }
    }

    public void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }

    public void Setup(LobbyGameInfo data, AppLobbyPanel panel)
    {
        roomData = data;
        parentPanel = panel;
        
        gameNameText.text = data.Name;
        playerCountText.text = $"{data.PlayerCount}/6";
        
        if (data.State == "playing")
        {
            statusText.text = "진행중";
            statusText.color = playingColor;
        }
        else
        {
            statusText.text = "대기중";
            statusText.color = waitingColor;
        }
        
        isInteractable = data.State == "waiting" && data.PlayerCount < 6;
        
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (!isInteractable)
        {
            backgroundImage.color = disabledColor;
        }
        else if (isSelected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        parentPanel?.OnRoomItemClicked(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        backgroundImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;
        backgroundImage.color = normalColor;
    }
}