using System;
using UnityEngine;
using TMPro;

public class NotificationDialog : Dialog,ICancelable
{
    [SerializeField] private TextMeshProUGUI messageText;

    private Action onCancelCallback;
    
    
    public void SetCallbacks(Action onCancel = null)
    {
        onCancelCallback = onCancel;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public void OnCancel()
    {
        audioManager.PlayButtonClick();
        if (onCancelCallback != null)
        {
            onCancelCallback.Invoke();
        }
        else
        {
            dialogManager.CloseTopDialog();
        }
    }
}