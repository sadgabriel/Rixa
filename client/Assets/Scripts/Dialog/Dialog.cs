using UnityEngine;

public class Dialog : MonoBehaviour
{
    protected DialogManager dialogManager;

    protected virtual void Start()
    {
        dialogManager = DialogManager.Instance;
    }
}