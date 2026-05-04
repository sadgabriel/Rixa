using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class DevCheats : MonoBehaviour
{
    [SerializeField] private TextAsset testScenarioFile;

    private TestScenario scenario;

    private static readonly Key[] FactionKeys = new Key[]
    {
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6
    };

    private void Start()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        LoadScenario();
#endif
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
            Debug.Log($"[DevCheat] Loaded scenario: {scenario.name} ({scenario.factions.Count} factions)");
            for (int i = 0; i < scenario.factions.Count; i++)
            {
                Debug.Log($"[DevCheat] F{i + 1}: {scenario.factions[i].name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DevCheat] Failed to load scenario: {e.Message}");
        }
    }

    private void Update()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (scenario == null || Keyboard.current == null) return;

        for (int i = 0; i < FactionKeys.Length; i++)
        {
            if (Keyboard.current[FactionKeys[i]].wasPressedThisFrame)
            {
                AutoSubmit(i);
                break;
            }
        }
#endif
    }

    private void AutoSubmit(int factionIndex)
    {
        if (factionIndex >= scenario.factions.Count)
        {
            Debug.LogWarning($"[DevCheat] Faction index {factionIndex} out of range (scenario has {scenario.factions.Count} factions)");
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
                client.SubmitFactionConcept(faction.concept, faction.name);
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