using UnityEngine;
using System;

public enum UIContext { APP, GAME }

public abstract class Panel : MonoBehaviour
{
    protected StateManager stateManager;
    protected DialogManager dialogManager;
    protected GameClient gameClient;

    protected virtual void Awake()
    {
        stateManager = StateManager.Instance;
        dialogManager = DialogManager.Instance;
        gameClient = GameClient.Instance;
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
public class NonPersistentPanel : Panel
{
    [SerializeField] private UIState panelState;
    
    public UIState PanelState => panelState;
}

public class PersistentPanel : Panel
{
    [SerializeField] private UIContext panelContext;
    
    public UIContext PanelContext => panelContext;
}