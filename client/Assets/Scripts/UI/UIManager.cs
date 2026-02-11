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

    private Dictionary<UIState, Panel> statePanels = new Dictionary<UIState, Panel>();
    private Dictionary<UIContext, List<Panel>> persistentPanels = new Dictionary<UIContext, List<Panel>>()
    {
        { UIContext.App, new List<Panel>() },
        { UIContext.Game, new List<Panel>() }
    };

    private UIContext currentContext = UIContext.App;

    private UIState currentState = UIState.IDLE;
    

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
        
        foreach (Panel panel in FindObjectsByType<Panel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panel.Hide();

            if (panel.IsPersistent)
            {
                UIContext context = panel.PanelContext;
                persistentPanels[context].Add(panel);
            } else
            {
                statePanels[panel.PanelState] = panel;
            }
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
        Debug.Log($"UIManager: Transitioning from {currentState} to {nextState}");
        if (nextState == currentState) return;

        UIContext nextContext = GetContext(nextState);

        if (nextContext != currentContext)
        {
            foreach (Panel panel in persistentPanels[currentContext])
            {
                panel.Hide();
            }
            foreach (Panel panel in persistentPanels[nextContext])
            {
                panel.Show();
            }
        }

        if (statePanels.TryGetValue(currentState, out var oldPanel))
        {
            oldPanel.Hide();
        }

        if (statePanels.TryGetValue(nextState, out var newPanel))
        {
            newPanel.Show();
        }

        currentContext = nextContext;
        currentState = nextState;
    }

    private UIContext GetContext(UIState state)
    {
        if (state == UIState.IDLE || state == UIState.APP_LOBBY)
        {
            return UIContext.App;
        } 
        else
        {
            return UIContext.Game;
        }
    }
}
