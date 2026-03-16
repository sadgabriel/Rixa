using System;
using UnityEngine;
using TMPro;

public class CreateGameDialog : Dialog, IConfirmable, ICancelable
{
    [SerializeField] private TMP_InputField gameNameInputField;
    [SerializeField] private TMP_InputField playerNameInputField;

    private Action<string, string> onConfirmCallback;
    private Action onCancelCallback;
    
    
    public void SetCallbacks(Action<string, string> onConfirm, Action onCancel = null)
    {
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
    }

    public void OnConfirm()
    {
        if (onConfirmCallback != null)
        {
            string gameName = gameNameInputField.text;
            string playerName = playerNameInputField.text;
            onConfirmCallback.Invoke(gameName, playerName);
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