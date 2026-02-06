using System.Collections.Generic;
using Newtonsoft.Json;

public class LobbyGameInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("state")]
    public string State { get; set; } // "waiting" or "playing"
    [JsonProperty("playerCount")]
    public int PlayerCount { get; set; }
}

public class LobbyState
{
    [JsonProperty("games")]
    public List<LobbyGameInfo> Games { get; set; }
}