using System.Collections.Generic;
using UnityEngine;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}