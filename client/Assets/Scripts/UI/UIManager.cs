using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public StateManager stateManager;

    private Dictionary<UIState, Panel> panels;

    private UIState? current;

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
        stateManager.OnUIStateChanged += HandleUiStateChanged;
    }

    private void OnDisable()
    {
        stateManager.OnUIStateChanged -= HandleUiStateChanged;
    }

    private void HandleUiStateChanged(UIState oldState, UIState nextState)
    {
        if (current.HasValue && panels.TryGetValue(current.Value, out var oldPanel))
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
