using System.Data.Common;

public class LobbyGameInfo
{
    [JsonProperty("id")]
    public string Id;
    [JsonProperty("name")]
    public string Name;
    [JsonProperty("state")]
    public string State;
    [JsonProperty("playerCount")]
    public int PlayerCount;
}

public class LobbyState
{
    [JsonProperty("games")]
    public List<LobbyGameInfo> Games;
}