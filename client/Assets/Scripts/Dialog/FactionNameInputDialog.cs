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
        audioManager.PlayButtonClick();
        if (onConfirmCallback != null)
        {
            string factionName = factionNameInputField.text;

            if (string.IsNullOrEmpty(factionName))
            {
                dialogManager.ShowNotificationDialog("팩션 이름을 입력해주세요.");
                return;
            }

            if (factionName.Length > 8)
            {
                dialogManager.ShowNotificationDialog("팩션 이름은 8자 이하로 입력해주세요.");
                return;
            }

            onConfirmCallback.Invoke(factionName);
        }
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