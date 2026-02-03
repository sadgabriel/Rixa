using System;
using UnityEngine;

public class CreateGameDialog : Dialog
{
    private Action<string, string> onConfirmCallback;
    private Action onCancelCallback;
    
    public void SetCallbacks(Action<string, string> onConfirm, Action onCancel = null)
    {
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
    }

    public void OnConfirmButtonClicked()
    {
        if (onConfirmCallback != null)
        {
            string gameName = "";
            string playerName = "";
            onConfirmCallback.Invoke(gameName, playerName);
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