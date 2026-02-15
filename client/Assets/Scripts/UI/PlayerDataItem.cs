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
        playerNameText.text = "알 수 없음";
        factionNameText.text = "미정";
        scoreText.text = "점수: 0";

        if (string.IsNullOrEmpty(playerId)) return;

        Player player = stateManager.GetPlayerById(playerId);
        if (player == null) return;
        playerNameText.text = player.Name;

        if (player.Ready)
        {
            statusIndicator.ShowReady();
        }
        else
        {
            statusIndicator.Hide();
        }

        Faction faction = stateManager.GetFactionById(player.FactionId);
        if (faction == null) return;
        factionNameText.text = faction.Name;
        scoreText.text = $"점수: {faction.Score}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        Player player = stateManager.GetPlayerById(playerId);
        if (player == null) return;

        Faction faction = stateManager.GetFactionById(player.FactionId);
        
        string factionName = faction?.Name ?? "미정";
        string factionDescription = faction?.Description ?? "설명 없음";
        
        dialogManager.ShowFactionDialog(factionName, factionDescription, () =>
        {
            dialogManager.CloseTopDialog();
        });
    }
}