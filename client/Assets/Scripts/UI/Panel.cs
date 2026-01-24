using UnityEngine;
using System;

public class Panel : MonoBehaviour
{
    [SerializeField] protected StateManager stateManager;
    [SerializeField] private UIState panelState;
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