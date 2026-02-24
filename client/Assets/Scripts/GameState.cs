using System.Collections.Generic;
using Newtonsoft.Json;

public class Context
{
    [JsonProperty("rawContextDescription")]
    public string RawContextDescription { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("eventLog")]
    public List<string> EventLog { get; set; }
}

public class Player
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("factionId")]
    public string FactionId { get; set; }

    [JsonProperty("ready")]
    public bool Ready { get; set; }
}

public class Faction
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("rawConcept")]
    public string RawConcept { get; set; }

    [JsonProperty("rawFlaw")]
    public string RawFlaw { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("resources")]
    public List<Resource> Resources { get; set; }

    [JsonProperty("score")]
    public int Score { get; set; }
}

public class Resource
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }
}

public class Match
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("attackerId")]
    public string AttackerId { get; set; }

    [JsonProperty("defenderId")]
    public string DefenderId { get; set; }

    [JsonProperty("attackDescription")]
    public string AttackDescription { get; set; }

    [JsonProperty("defenseDescription")]
    public string DefenseDescription { get; set; }

    [JsonProperty("attackerTags")]
    public List<string> AttackerTags { get; set; }

    [JsonProperty("defenderTags")]
    public List<string> DefenderTags { get; set; }

    [JsonProperty("targetedResources")]
    public List<string> TargetedResources { get; set; }

    [JsonProperty("protectedResources")]
    public List<string> ProtectedResources { get; set; }

    [JsonProperty("winnerId")]
    public string WinnerId { get; set; }

    [JsonProperty("lostResource")]
    public string LostResource { get; set; }

    [JsonProperty("displayNarrative")]
    public string DisplayNarrative { get; set; }
}

public class GameState
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("phase")]
    public string Phase { get; set; }

    [JsonProperty("round")]
    public int Round { get; set; }

    [JsonProperty("context")]
    public Context Context { get; set; }

    [JsonProperty("players")]
    public List<Player> Players { get; set; }

    [JsonProperty("leadPlayerId")]
    public string LeadPlayerId { get; set; }

    [JsonProperty("factions")]
    public List<Faction> Factions { get; set; }

    [JsonProperty("matches")]
    public List<Match> Matches { get; set; }

    [JsonProperty("setupOffset")]
    public int SetupOffset { get; set; }
}
