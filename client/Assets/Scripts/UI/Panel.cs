using UnityEngine;
using System;

public class Panel : MonoBehaviour
{
    [SerializeField] private UIState panelState;
    private StateManager stateManager;

    protected virtual void Start()
    {
        stateManager = StateManager.Instance;
    }

    public UIState PanelState
    {
        get { return panelState; }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}