using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

public class WsClient : MonoBehaviour
{
    private static WsClient instance;
    public static WsClient Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<WsClient>();
            }
            return instance;
        }
    }

    private string serverUrl = "ws://211.220.5.29:7363";
    private NativeWebSocket.WebSocket ws;

    public event Action<string, JToken> OnMessage;
    public event Action OnOpen;
    public event Action<string> OnClose;
    public event Action<string> OnError;

    public bool IsConnected => ws != null && ws.State == NativeWebSocket.WebSocketState.Open;

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

        LoadConfig();
        Initialize();
    }

    private async void Start()
    {
        try
        {
            await ws.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection failed: {e.Message}");
            OnError?.Invoke($"Connection failed: {e.Message}");
        }
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
    }

    private void OnDestroy()
    {
        if (IsConnected)
        {
            ws.Close();
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    public async void Disconnect()
    {
        if (ws != null)
        {
            await ws.Close();
        }
    }

    public async void Send(string type, object data)
    {
        if (!IsConnected) return;

        var message = new
        {
            type,
            data
        };

        string text = JsonConvert.SerializeObject(message);
        await ws.SendText(text);
    }

    private void Initialize()
    {
        ws = new NativeWebSocket.WebSocket(serverUrl);

        ws.OnOpen += () =>
        {
            OnOpen?.Invoke();
        };

        ws.OnError += (e) =>
        {
            OnError?.Invoke(e);
        };

        ws.OnClose += (e) =>
        {
            OnClose?.Invoke(e.ToString());
        };

        ws.OnMessage += (bytes) =>
        {
            string text = Encoding.UTF8.GetString(bytes);

            try
            {
                JObject obj = JObject.Parse(text);

                string type = (string)obj["type"];
                JToken data = obj["data"];
                OnMessage?.Invoke(type, data);
            }
            catch (Exception e)
            { 
                OnError?.Invoke($"Bad incoming JSON: {e.Message}");
            }
        };
    }

    private void LoadConfig()
    {
        try
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "config.json");
            string json = System.IO.File.ReadAllText(path);
            var config = JsonUtility.FromJson<WsConfig>(json);
            if (!string.IsNullOrEmpty(config.serverUrl))
            {
                serverUrl = config.serverUrl;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"config.json 로드 실패, 기본값 사용: {e.Message}");
        }
    }

    [Serializable]
    private class WsConfig
    {
        public string serverUrl;
    }
}

