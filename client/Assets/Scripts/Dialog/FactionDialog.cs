using System;
using UnityEngine;
using TMPro;

public class FactionDialog : Dialog, ICancelable
{
    [SerializeField] private TextMeshProUGUI factionNameText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;
    [SerializeField] private TextMeshProUGUI resourceText1;
    [SerializeField] private TextMeshProUGUI resourceText2;
    [SerializeField] private TextMeshProUGUI resourceText3;
    [SerializeField] private TextMeshProUGUI resourceDescriptionText1;
    [SerializeField] private TextMeshProUGUI resourceDescriptionText2;
    [SerializeField] private TextMeshProUGUI resourceDescriptionText3;

    private Action onCancelCallback;

    public void SetFactionInfo(string factionName, string factionDescription)
    {
        factionNameText.text = factionName;
        factionDescriptionText.text = factionDescription;
    }

    public void SetResourceInfo(string resourceName1, int resourceValue1, string resourceDescription1, string resourceName2, int resourceValue2, string resourceDescription2, string resourceName3, int resourceValue3, string resourceDescription3)
    {
        resourceText1.text = $"{resourceName1}: {resourceValue1}";
        resourceText2.text = $"{resourceName2}: {resourceValue2}";
        resourceText3.text = $"{resourceName3}: {resourceValue3}";
        resourceDescriptionText1.text = resourceDescription1;
        resourceDescriptionText2.text = resourceDescription2;
        resourceDescriptionText3.text = resourceDescription3;
    }

    public void SetCallback(Action onCancel)
    {
        onCancelCallback = onCancel;
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