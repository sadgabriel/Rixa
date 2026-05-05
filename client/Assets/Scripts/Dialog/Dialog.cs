using UnityEngine;

public class Dialog : MonoBehaviour
{
    protected DialogManager dialogManager;
    protected AudioManager audioManager;

    protected virtual void Awake()
    {
        dialogManager = DialogManager.Instance;
        audioManager = AudioManager.Instance;
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