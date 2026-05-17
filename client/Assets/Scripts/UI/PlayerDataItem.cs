using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class PlayerDataItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI factionNameText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private StatusIndicator statusIndicator;
    [SerializeField] private Image attackIcon;
    [SerializeField] private Image defenseIcon;
    [SerializeField] private GameObject bothIcons;
    [SerializeField] private Image selfIcon;

    private string playerId;
    private StateManager stateManager;
    private DialogManager dialogManager;
    private AudioManager audioManager;

    private void Awake()
    {
        stateManager = StateManager.Instance;
        dialogManager = DialogManager.Instance;
        audioManager = AudioManager.Instance;
    }

    private void OnEnable()
    {
        stateManager.OnGameStateUpdated += HandleGameStateUpdated;
        attackIcon.gameObject.SetActive(false);
        defenseIcon.gameObject.SetActive(false);
        bothIcons.SetActive(false);
        selfIcon.gameObject.SetActive(false);
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
        UIState currentState = stateManager.CurrentUIState;
        if (currentState == UIState.GAME_COMPETITION_CALCULATE)
        {
            statusIndicator.Hide();
            return;
        }

        playerNameText.text = "-";
        factionNameText.text = "-";
        scoreText.text = "-";

        if (string.IsNullOrEmpty(playerId)) return;

        if (playerId == stateManager.CurrentClientState.PlayerId)
        {
            selfIcon.gameObject.SetActive(true);
        }
        else
        {
            selfIcon.gameObject.SetActive(false);
        }

        Player player = stateManager.GetPlayerById(playerId);
        if (player == null)
        {
            statusIndicator.Hide();
            return;
        }
        playerNameText.text = player.Name;

        Faction faction = stateManager.GetFactionById(player.FactionId);
        if (faction != null)
        {
            factionNameText.text = string.IsNullOrEmpty(faction.Name) ? "-" : faction.Name;
            scoreText.text = $"{faction.Score}";
        }

        Faction anotherFaction = stateManager.GetAnotherFactionByPlayerId(playerId);

        if (currentState == UIState.GAME_ATTACK)
        {
            attackIcon.gameObject.SetActive(false);
            defenseIcon.gameObject.SetActive(false);
            bothIcons.SetActive(false);

            Match attackMatch = stateManager.MyAttackMatch;
            Match defenseMatch = stateManager.MyDefenseMatch;

            if (attackMatch.DefenderId == faction.Id && defenseMatch.AttackerId == faction.Id)
            {
                bothIcons.SetActive(true);
            }
            else if (attackMatch.DefenderId == faction.Id)
            {
                defenseIcon.gameObject.SetActive(true);
            }
            else if (defenseMatch.AttackerId == faction.Id)
            {
                attackIcon.gameObject.SetActive(true);
            }
        }

        if (stateManager.CurrentUIState == UIState.GAME_LOBBY && player.Ready && !stateManager.IsFirst(playerId))
        {
            statusIndicator.ShowReady();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_CONCEPT_INPUT && !string.IsNullOrEmpty(faction.RawConcept))
        {
            statusIndicator.ShowCompleted();
        }
        else if (stateManager.CurrentUIState == UIState.GAME_FACTION_FLAW_INPUT && !string.IsNullOrEmpty(anotherFaction?.RawFlaw))
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
        audioManager.PlayButtonClick();
        if (string.IsNullOrEmpty(playerId)) return;

        Player player = stateManager.GetPlayerById(playerId);
        if (player == null) return;

        Faction faction = stateManager.GetFactionById(player.FactionId);
        
        string factionName = faction?.Name ?? "진영명 미정";
        string factionDescription = faction?.Description ?? "";

        List<Resource> resources = faction?.Resources;

        if (resources != null && resources.Count >= 3)
        {
            dialogManager.ShowFactionDialog(factionName, factionDescription, resources[0].Name, resources[0].Count, resources[0].Description, resources[1].Name, resources[1].Count, resources[1].Description, resources[2].Name, resources[2].Count, resources[2].Description);
        }
        else
        {
            dialogManager.ShowFactionDialog(factionName, factionDescription, "자원명 1", 0, "자원 설명 1", "자원명 2", 0, "자원 설명 2", "자원명 3", 0, "자원 설명 3");
        }
    }
}