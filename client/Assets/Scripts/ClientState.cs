using System.Collections.Generic;
using Newtonsoft.Json;

public class ClientState
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("playerId")]
    public string PlayerId { get; set; }
    [JsonProperty("gameId")]
    public string GameId { get; set; }
}