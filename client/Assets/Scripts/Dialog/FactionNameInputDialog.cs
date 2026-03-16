using System;
using UnityEngine;
using TMPro;

public class FactionNameInputDialog : Dialog, IConfirmable, ICancelable
{   
    [SerializeField] private TMP_InputField factionNameInputField;

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
            string factionName = factionNameInputField.text;
            onConfirmCallback.Invoke(factionName);
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