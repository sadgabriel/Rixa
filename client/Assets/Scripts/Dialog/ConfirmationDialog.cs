using System;
using UnityEngine;
using TMPro;

public class ConfirmationDialog : Dialog, IConfirmable, ICancelable
{
    [SerializeField] private TextMeshProUGUI messageText;

    private Action onConfirmCallback;
    private Action onCancelCallback;
    
    
    public void SetCallbacks(Action onConfirm, Action onCancel = null)
    {
        onConfirmCallback = onConfirm;
        onCancelCallback = onCancel;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public void OnConfirm()
    {
        if (onConfirmCallback != null)
        {
            onConfirmCallback.Invoke();
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