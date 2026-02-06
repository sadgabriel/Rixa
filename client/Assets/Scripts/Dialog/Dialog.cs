using UnityEngine;

public class Dialog : MonoBehaviour
{
    protected DialogManager dialogManager;

    protected virtual void Awake()
    {
        dialogManager = DialogManager.Instance;
    }
}