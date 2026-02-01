using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public StateManager stateManager;

    private Dictionary<UIState, Panel> panels;

    private UIState current = UIState.IDLE;

    private void Awake()
    {
        panels = new Dictionary<UIState, Panel>();
        
        foreach (var panel in GetComponentsInChildren<Panel>(true))
        {
            panels.Add(panel.PanelState, panel);
            panel.Hide();
        }
    }

    private void OnEnable()
    {
        stateManager.OnUIStateUpdated += HandleUIStateUpdated;
    }

    private void OnDisable()
    {
        stateManager.OnUIStateUpdated -= HandleUIStateUpdated;
    }

    private void HandleUIStateUpdated(UIState nextState)
    {
        if (panels.TryGetValue(current, out var oldPanel))
        {
            oldPanel.Hide();
        }

        if (panels.TryGetValue(nextState, out var newPanel))
        {
            newPanel.Show();
        }

        current = nextState;
    }
}
