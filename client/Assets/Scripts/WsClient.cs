using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class WsClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://localhost:7363";
    private NativeWebSocket.WebSocket ws;

    public event Action<string, JToken> OnMessage;
    public event Action OnOpen;
    public event Action<string> OnClose;
    public event Action<string> OnError;

    public bool IsConnected => ws != null && ws.State == NativeWebSocket.WebSocketState.Open;

    private void Awake()
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

    private async void Start()
    {
        await ws.Connect();
    }

    private void Update()
    {
        ws?.DispatchMessageQueue();
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

    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }
}
