using System;
using UnityEngine;
using TMPro;

public class FactionDialog : Dialog, ICancelable
{
    [SerializeField] private TextMeshProUGUI factionNameText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;

    private Action onCancelCallback;

    public void SetFactionInfo(string factionName, string factionDescription)
    {
        factionNameText.text = factionName;
        factionDescriptionText.text = factionDescription;
    }

    public void SetCallback(Action onCancel)
    {
        onCancelCallback = onCancel;
    }

    public void OnCancel()
    {
        if (onCancelCallback != null)
        {
            onCancelCallback.Invoke();
        }
    }
}