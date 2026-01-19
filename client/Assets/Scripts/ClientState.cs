using System.Collections.Generic;
using Newtonsoft.Json;

public class ClientState
{
    [JsonProperty("id")]
    public string Id;
    [JsonProperty("playerId")]
    public string PlayerId;
    [JsonProperty("gameId")]
    public string GameId;
}