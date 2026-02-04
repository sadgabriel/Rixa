using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private GameObject modalBackground;
    [SerializeField] private Transform dialogContainer;

    [SerializeField] private GameObject createGameDialogPrefab;
    [SerializeField] private GameObject joinGameDialogPrefab;

    private Stack<GameObject> dialogStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        modalBackground.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTopDialog();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowCreateGameDialog(Action<string, string> onConfirm, Action onCancel = null)
    {
        GameObject dialog = ShowDialog(createGameDialogPrefab);
        CreateGameDialog createGameDialog = dialog.GetComponent<CreateGameDialog>();
        createGameDialog?.SetCallbacks(onConfirm, onCancel);
    }

    public void ShowJoinGameDialog(Action<string> onConfirm, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(joinGameDialogPrefab);
        JoinGameDialog joinGameDialog = dialogGO.GetComponent<JoinGameDialog>();
        joinGameDialog?.SetCallbacks(onConfirm, onCancel);
    }

    private GameObject ShowDialog(GameObject dialogPrefab)
    {
        GameObject dialogGO = Instantiate(dialogPrefab, dialogContainer);
        dialogStack.Push(dialogGO);
        modalBackground.SetActive(true);

        modalBackground.transform.SetAsLastSibling();
        dialogGO.transform.SetAsLastSibling();

        return dialogGO;
    }

    public void CloseTopDialog()
    {
        if (dialogStack.Count > 0)
        {
            GameObject topDialog = dialogStack.Pop();
            Destroy(topDialog);
            if (dialogStack.Count == 0)
            {
                modalBackground.SetActive(false);
            }
        }
    }
}