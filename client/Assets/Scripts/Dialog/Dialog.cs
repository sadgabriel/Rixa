using UnityEngine;

public class Dialog : MonoBehaviour
{
    protected DialogManager dialogManager;

    protected virtual void Awake()
    {
        dialogManager = DialogManager.Instance;
    }
}

public interface IConfirmable
{
    void OnConfirm();
}

public interface ICancelable
{
    void OnCancel();
}