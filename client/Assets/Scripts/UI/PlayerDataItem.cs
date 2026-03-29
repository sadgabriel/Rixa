using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class PlayerDataItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI factionNameText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private StatusIndicator statusIndicator;

    private string playerId;
    private StateManager stateManager;
    private DialogManager dialogManager;

    private void Awake()
    {
        stateManager = StateManager.Instance;
        dialogManager = DialogManager.Instance;
    }

    private void OnEnable()
    {
        stateManager.OnGameStateUpdated += HandleGameStateUpdated;
        Refresh();
    }

    private void OnDisable()
    {
        stateManager.OnGameStateUpdated -= HandleGameStateUpdated;
    }

    public void Setup(string playerId)
    {
        this.playerId = playerId;
        Refresh();
    }

    private void HandleGameStateUpdated(GameState gameState)
    {
        Refresh();
    }

    private void Refresh()
    {
        playerNameText.text = "-";
        factionNameText.text = "-";
        scoreText.text = "0";

        if (string.IsNullOrEmpty(playerId)) return;

        Player player = stateManager.GetPlayerById(playerId);
        if (player != null)
        {
            playerNameText.text = player.Name;
        }
        

        Faction faction = stateManager.GetFactionById(player.FactionId);
        if (faction != null)
        {
            factionNameText.text = string.IsNullOrEmpty(faction.Name) ? "-" : faction.Name;
            scoreText.text = $"{faction.Score}";
        }

        if (stateManager.CurrentUIState == UIState.GAME_LOBBY && player.Ready && !stateManager.IsFirst(playerId))
        {
            statusIndicator.ShowReady();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_CONCEPT_INPUT && !string.IsNullOrEmpty(faction.RawConcept))
        {
            statusIndicator.ShowCompleted();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_FLAW_INPUT && !string.IsNullOrEmpty(faction.RawFlaw))
        {
            statusIndicator.ShowCompleted();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_ATTACK)
        {
            Match match = stateManager.CurrentGameState.Matches.Find(m => m.AttackerId == faction.Id);
            if (match != null && !string.IsNullOrEmpty(match.AttackDescription))
            {
                statusIndicator.ShowCompleted();
            }
            else
            {
                statusIndicator.Hide();
            }
        }
        else if (stateManager.CurrentUIState == UIState.GAME_DEFENSE)
        {
            Match match = stateManager.CurrentGameState.Matches.Find(m => m.DefenderId == faction.Id);
            if (match != null && !string.IsNullOrEmpty(match.DefenseDescription))
            {
                statusIndicator.ShowCompleted();
            }
            else
            {
                statusIndicator.Hide();
            }
        }
        else
        {
            statusIndicator.Hide();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        Player player = stateManager.GetPlayerById(playerId);
        if (player == null) return;

        Faction faction = stateManager.GetFactionById(player.FactionId);
        
        string factionName = faction?.Name ?? "진영명 미정";
        string factionDescription = faction?.Description ?? "";

        List<Resource> resources = faction?.Resources;

        if (resources != null && resources.Count >= 3)
        {
            dialogManager.ShowFactionDialog(factionName, factionDescription, resources[0].Name, resources[0].Count, resources[1].Name, resources[1].Count, resources[2].Name, resources[2].Count, () =>
            {
                dialogManager.CloseTopDialog();
            });
        }
        else
        {
            dialogManager.ShowFactionDialog(factionName, factionDescription, "자원명 1", 0, "자원명 2", 0, "자원명 3", 0, () =>
            {
                dialogManager.CloseTopDialog();
            });
        }
    }
}