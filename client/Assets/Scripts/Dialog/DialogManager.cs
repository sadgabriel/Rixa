using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    private static DialogManager instance;
    public static DialogManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DialogManager>();
            }
            return instance;
        }
    }

    [SerializeField] private GameObject modalBackground;
    [SerializeField] private Transform dialogContainer;

    [SerializeField] private GameObject notificationDialogPrefab;
    [SerializeField] private GameObject confirmationDialogPrefab;
    [SerializeField] private GameObject createGameDialogPrefab;
    [SerializeField] private GameObject joinGameDialogPrefab;
    [SerializeField] private GameObject factionDialogPrefab;
    [SerializeField] private GameObject factionNameInputDialogPrefab;

    private Stack<GameObject> dialogStack = new Stack<GameObject>();

    public bool IsDialogOpen => dialogStack.Count > 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        modalBackground.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (IsDialogOpen)
                {
                    CloseTopDialog();
                } else
                {
                    ShowConfirmationDialog("게임을 종료하시겠습니까?", () =>
                    {
                        Application.Quit();
                        CloseTopDialog();
                    });
                }
            } else if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (IsDialogOpen)
                {
                    GameObject topDialog = dialogStack.Peek();
                    IConfirmable confirmable = topDialog.GetComponent<IConfirmable>();
                    if (confirmable != null)
                    {
                        confirmable.OnConfirm();
                    }
                }
            }
            
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void ShowNotificationDialog(string message, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(notificationDialogPrefab);
        NotificationDialog notificationDialog = dialogGO.GetComponent<NotificationDialog>();
        notificationDialog?.SetMessage(message);
        notificationDialog?.SetCallbacks(onCancel);
    }

    public void ShowConfirmationDialog(string message, Action onConfirm, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(confirmationDialogPrefab);
        ConfirmationDialog confirmationDialog = dialogGO.GetComponent<ConfirmationDialog>();
        confirmationDialog?.SetMessage(message);
        confirmationDialog?.SetCallbacks(onConfirm, onCancel);
    }

    public void ShowCreateGameDialog(Action<string, string> onConfirm, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(createGameDialogPrefab);
        CreateGameDialog createGameDialog = dialogGO.GetComponent<CreateGameDialog>();
        createGameDialog?.SetCallbacks(onConfirm, onCancel);
    }

    public void ShowJoinGameDialog(Action<string> onConfirm, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(joinGameDialogPrefab);
        JoinGameDialog joinGameDialog = dialogGO.GetComponent<JoinGameDialog>();
        joinGameDialog?.SetCallbacks(onConfirm, onCancel);
    }

    public void ShowFactionDialog(string factionName, string factionDescription, string resourceName1, int resourceValue1, string resourceDescription1, string resourceName2, int resourceValue2, string resourceDescription2, string resourceName3, int resourceValue3, string resourceDescription3, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(factionDialogPrefab);
        FactionDialog factionDialog = dialogGO.GetComponent<FactionDialog>();
        factionDialog?.SetFactionInfo(factionName, factionDescription);
        factionDialog?.SetResourceInfo(resourceName1, resourceValue1, resourceDescription1, resourceName2, resourceValue2, resourceDescription2, resourceName3, resourceValue3, resourceDescription3);
        factionDialog?.SetCallback(onCancel);
    }

    public void ShowFactionNameInputDialog(Action<string> onConfirm, Action onCancel = null)
    {
        GameObject dialogGO = ShowDialog(factionNameInputDialogPrefab);
        FactionNameInputDialog factionNameInputDialog = dialogGO.GetComponent<FactionNameInputDialog>();
        factionNameInputDialog?.SetCallbacks(onConfirm, onCancel);
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
        if (IsDialogOpen)
        {
            GameObject topDialog = dialogStack.Pop();
            Destroy(topDialog);
            if (dialogStack.Count == 0)
            {
                modalBackground.SetActive(false);
            }
            else
            {
                modalBackground.transform.SetAsLastSibling();
                dialogStack.Peek().transform.SetAsLastSibling();
            }
        }
    }
}