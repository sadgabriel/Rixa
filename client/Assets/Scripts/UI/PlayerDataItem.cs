using UnityEngine;
using UnityEngine.EventSystems;

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

        if (stateManager.CurrentUIState == UIState.GAME_LOBBY && player.Ready)
        {
            statusIndicator.ShowReady();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_CONCEPT_INPUT && !string.IsNullOrEmpty(faction.RawConcept))
        {
            statusIndicator.ShowCompleted();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_FLAW_INPUT && string.IsNullOrEmpty(faction.RawFlaw))
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
        
        string factionName = faction?.Name ?? "미정";
        string factionDescription = faction?.Description ?? "";
        
        dialogManager.ShowFactionDialog(factionName, factionDescription, () =>
        {
            dialogManager.CloseTopDialog();
        });
    }
}