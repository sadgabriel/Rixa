using System;
using UnityEngine;
using TMPro;

public class JoinGameDialog : Dialog, IConfirmable, ICancelable
{   
    [SerializeField] private TMP_InputField playerNameInputField;

    private Action<string> onConfirmCallback;
    private Action onCancelCallback;
    
    public void SetCallbacks(Action<string> onConfirm, Action onCancel = null)
    {
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
    }

    public void OnConfirm()
    {
        if (onConfirmCallback != null)
        {
            string playerName = playerNameInputField.text;
            onConfirmCallback.Invoke(playerName);
        }
    }

    public void OnCancel()
    {
        if (onCancelCallback != null)
        {
            onCancelCallback.Invoke();
        }
    }
}