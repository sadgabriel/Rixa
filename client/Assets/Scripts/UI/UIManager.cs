using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UIManager>();
            }
            return instance;
        }
    }

    private StateManager stateManager;

    private Dictionary<UIState, Panel> panels;

    private UIState current = UIState.IDLE;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        panels = new Dictionary<UIState, Panel>();
        
        foreach (var panel in FindObjectsByType<Panel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panels.Add(panel.PanelState, panel);
            panel.Hide();
        }

        stateManager = StateManager.Instance;
        stateManager.OnUIStateUpdated += HandleUIStateUpdated;
    }

    private void OnDestroy()
    {
        stateManager.OnUIStateUpdated -= HandleUIStateUpdated;

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleUIStateUpdated(UIState nextState)
    {
        if (nextState == current)
        {
            return;
        }

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
