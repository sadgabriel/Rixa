using UnityEngine;
using System;

public class Panel : MonoBehaviour
{
    [SerializeField] private UIState panelState;
    protected StateManager stateManager;
    protected DialogManager dialogManager;
    protected GameClient gameClient;

    protected virtual void Start()
    {
        stateManager = StateManager.Instance;
        dialogManager = DialogManager.Instance;
        gameClient = GameClient.Instance;
    }

    public UIState PanelState
    {
        get { return panelState; }
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