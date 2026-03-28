using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class DevCheats : MonoBehaviour
{
    [SerializeField] private TextAsset testScenarioFile;
    
    private TestScenario scenario;
    private int faction1Index = 0;
    private int faction2Index = 1;
    
    private void Start()
    {
        LoadScenario();
    }
    
    private void LoadScenario()
    {
        if (testScenarioFile == null)
        {
            Debug.LogError("[DevCheat] Test scenario file not assigned!");
            return;
        }
        
        try
        {
            scenario = JsonConvert.DeserializeObject<TestScenario>(testScenarioFile.text);
            Debug.Log($"[DevCheat] Loaded scenario: {scenario.name}");
            Debug.Log($"[DevCheat] F1: {scenario.factions[faction1Index].name}");
            Debug.Log($"[DevCheat] F2: {scenario.factions[faction2Index].name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DevCheat] Failed to load scenario: {e.Message}");
        }
    }
    
    private void Update()
    {
        if (scenario == null || Keyboard.current == null) return;
        
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            AutoSubmit(faction1Index);
        }
        
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            AutoSubmit(faction2Index);
        }
    }
    
    private void AutoSubmit(int factionIndex)
    {
        if (factionIndex >= scenario.factions.Count)
        {
            Debug.LogWarning($"[DevCheat] Faction index {factionIndex} out of range");
            return;
        }
        
        FactionTestData faction = scenario.factions[factionIndex];
        UIState state = StateManager.Instance.CurrentUIState;
        
        SubmitForState(state, faction);
    }
    
    private void SubmitForState(UIState state, FactionTestData faction)
    {
        GameClient client = GameClient.Instance;
        StateManager stateManager = StateManager.Instance;
        
        switch (state)
        {
            case UIState.GAME_CONTEXT_INPUT:
                client.SubmitContext(faction.context);
                Debug.Log($"[DevCheat] {faction.name} submitted context");
                break;
            case UIState.GAME_FACTION_CONCEPT_INPUT:
                client.SubmitFactionConcept(faction.concept, faction.conceptName);
                Debug.Log($"[DevCheat] {faction.name} submitted concept");
                break;
            case UIState.GAME_FACTION_FLAW_INPUT:
                Faction anotherFaction = stateManager.AnotherFaction;
                if (anotherFaction != null)
                {
                    client.SubmitFactionFlaw(anotherFaction.Id, faction.flaw);
                    Debug.Log($"[DevCheat] {faction.name} submitted flaw");
                }
                else
                {
                    Debug.LogWarning($"[DevCheat] Cannot submit flaw: AnotherFaction is null");
                }
                break;
            case UIState.GAME_ATTACK:
                client.SubmitAttack(faction.attack);
                Debug.Log($"[DevCheat] {faction.name} submitted attack");
                break;
            case UIState.GAME_DEFENSE:
                client.SubmitDefense(faction.defense);
                Debug.Log($"[DevCheat] {faction.name} submitted defense");
                break;
            default:
                Debug.LogWarning($"[DevCheat] No submit method for state {state}");
                break;
        }
    }
}