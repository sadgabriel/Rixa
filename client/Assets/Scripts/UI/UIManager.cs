using UnityEngine;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private StateManager stateManager;

    private Dictionary<UIState, Panel> panels;

    private UIState current = UIState.IDLE;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        panels = new Dictionary<UIState, Panel>();
        
        foreach (var panel in FindObjectsByType<Panel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panels.Add(panel.PanelState, panel);
            panel.Hide();
        }

        Debug.Log(panels.Count + " panels registered in UIManager.");
    }

    private void Start()
    {
        stateManager = StateManager.Instance;
        stateManager.OnUIStateUpdated += HandleUIStateUpdated;
    }

    private void OnDestroy()
    {
        stateManager.OnUIStateUpdated -= HandleUIStateUpdated;

        if (Instance == this)
        {
            Instance = null;
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
