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
        audioManager.PlayButtonClick();

        if (onConfirmCallback != null)
        {
            string gameName = gameNameInputField.text;
            string playerName = playerNameInputField.text;

            if (string.IsNullOrEmpty(gameName))
            {
                dialogManager.ShowNotificationDialog("게임 이름을 입력해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                dialogManager.ShowNotificationDialog("플레이어 이름을 입력해주세요.");
                return;
            }

            if (gameName.Length > 15)
            {
                dialogManager.ShowNotificationDialog("게임 이름은 15자 이하로 입력해주세요.");
                return;
            }

            if (playerName.Length > 8)
            {
                dialogManager.ShowNotificationDialog("플레이어 이름은 8자 이하로 입력해주세요.");
                return;
            }

            onConfirmCallback.Invoke(gameName, playerName);
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