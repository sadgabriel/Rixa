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

    private Dictionary<UIState, NonPersistentPanel> nonPersistentPanels = new Dictionary<UIState, NonPersistentPanel>();
    private Dictionary<UIContext, List<PersistentPanel>> persistentPanels = new Dictionary<UIContext, List<PersistentPanel>>()
    {
        { UIContext.App, new List<PersistentPanel>() },
        { UIContext.Game, new List<PersistentPanel>() }
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

            if (panel is PersistentPanel persistentPanel)
            {
                UIContext context = persistentPanel.PanelContext;
                persistentPanels[context].Add(persistentPanel);
            } else if (panel is NonPersistentPanel nonPersistentPanel)
            {
                nonPersistentPanels[nonPersistentPanel.PanelState] = nonPersistentPanel;
            }
        }

        stateManager = StateManager.Instance;
        stateManager.OnUIStateUpdated += HandleUIStateUpdated;
    }

    private void OnDestroy()
    {
        if (stateManager != null)
        {
            stateManager.OnUIStateUpdated -= HandleUIStateUpdated;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleUIStateUpdated(UIState nextState, bool stateChanged)
    {
        Debug.Log($"[UIManager] UIState updated: {currentState} → {nextState}");
        if (!stateChanged) return;

        UIContext nextContext = GetContext(nextState);

        if (nextContext != currentContext)
        {
            Debug.Log($"[UIManager] Context changed: {currentContext} → {nextContext}");
            foreach (Panel panel in persistentPanels[currentContext])
            {
                panel.Hide();
            }
            foreach (Panel panel in persistentPanels[nextContext])
            {
                panel.Show();
            }
        }

        if (nonPersistentPanels.TryGetValue(currentState, out var oldPanel))
        {
            oldPanel.Hide();
        }

        if (nonPersistentPanels.TryGetValue(nextState, out var newPanel))
        {
            newPanel.Show();
        }
        else if (nextState != UIState.IDLE && nextState != UIState.None)
        {
            Debug.LogWarning($"[UIManager] No panel registered for state: {nextState}");
        }

        currentContext = nextContext;
        currentState = nextState;
    }

    private UIContext GetContext(UIState state)
    {
        switch (state)
        {
            case UIState.IDLE:
            case UIState.APP_LOBBY:
                return UIContext.App;
            
            case UIState.None:
                Debug.LogWarning($"[UIManager] UIState.None has no context");
                return UIContext.App;
            
            default:
                return UIContext.Game;
        }
    }
}
