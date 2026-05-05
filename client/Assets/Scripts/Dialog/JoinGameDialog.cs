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
        audioManager.PlayButtonClick();
        if (onConfirmCallback != null)
        {
            string playerName = playerNameInputField.text;

            if (string.IsNullOrEmpty(playerName))
            {
                dialogManager.ShowNotificationDialog("플레이어 이름을 입력해주세요.");
                return;
            }

            if (playerName.Length > 8){
            
                dialogManager.ShowNotificationDialog("플레이어 이름은 8자 이하로 입력해주세요.");
                return;
            }

            onConfirmCallback.Invoke(playerName);
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