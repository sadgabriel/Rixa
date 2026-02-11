using UnityEngine;
using System;

public enum UIContext { App, Game }

public class Panel : MonoBehaviour
{
    [SerializeField] private UIState panelState;
    [SerializeField] private UIContext panelContext;
    [SerializeField] private bool isPersistent = false;

    protected StateManager stateManager;
    protected DialogManager dialogManager;
    protected GameClient gameClient;

    protected virtual void Awake()
    {
        stateManager = StateManager.Instance;
        dialogManager = DialogManager.Instance;
        gameClient = GameClient.Instance;
    }

    public UIState PanelState
    {
        get { return panelState; }
    }

    public UIContext PanelContext
    {
        get { return panelContext; }
    }

    public bool IsPersistent
    {
        get { return isPersistent; }
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}