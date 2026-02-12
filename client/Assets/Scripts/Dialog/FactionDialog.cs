using System;
using UnityEngine;
using TMPro;

public class FactionDialog : Dialog
{
    [SerializeField] private TextMeshProUGUI factionNameText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;

    private Action onCloseButtonClickedCallback;

    public void SetFactionInfo(string factionName, string factionDescription)
    {
        factionNameText.text = factionName;
        factionDescriptionText.text = factionDescription;
    }

    public void SetOnCloseButtonClickedCallback(Action callback)
    {
        onCloseButtonClickedCallback = callback;
    }

    public void OnCloseButtonClicked()
    {
        if (onCloseButtonClickedCallback != null)
        {
            onCloseButtonClickedCallback.Invoke();
        }
    }
}