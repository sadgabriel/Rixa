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

    private enum UIContext { App, Game }

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
                UIContext context = GetContext(panel.PanelState);
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
        if (nextState == currentState)
        {
            return;
        }

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

        currentContext = nextContext;

        if (statePanels.TryGetValue(currentState, out var oldPanel))
        {
            oldPanel.Hide();
        }

        if (statePanels.TryGetValue(nextState, out var newPanel))
        {
            newPanel.Show();
        }

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
