using System;
using UnityEngine;

public class JoinGameDialog : Dialog
{   
    private Action<string> onConfirmCallback;
    private Action onCancelCallback;
    
    public void SetCallbacks(Action<string> onConfirm, Action onCancel = null)
    {
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
    }

    public void OnConfirmButtonClicked()
    {
        if (onConfirmCallback != null)
        {
            string playerName = "";
            onConfirmCallback.Invoke(playerName);
        }
    }

    public void OnCancelButtonClicked()
    {
        if (onCancelCallback != null)
        {
            onCancelCallback.Invoke();
        }
    }
}