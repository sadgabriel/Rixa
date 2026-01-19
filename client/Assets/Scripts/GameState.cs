using System.Collections.Generic;
using Newtonsoft.Json;

public class Context
{
    [JsonProperty("rawContextDescription")]
    public string RawContextDescription;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("eventLog")]
    public List<string> EventLog;
}

public class Player
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("factionId")]
    public string FactionId;

    [JsonProperty("ready")]
    public bool Ready;
}

public class Faction
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("rawConcept")]
    public string RawConcept;

    [JsonProperty("rawFlaw")]
    public string RawFlaw;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("resources")]
    public List<Resource> Resources;

    [JsonProperty("score")]
    public int Score;
}

public class Resource
{
    [JsonProperty("name")]
    public string Name;

    [JsonProperty("description")]
    public string Description;

    [JsonProperty("count")]
    public int Count;
}

public class Match
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("attackerId")]
    public string AttackerId;

    [JsonProperty("defenderId")]
    public string DefenderId;

    [JsonProperty("attackDescription")]
    public string AttackDescription;

    [JsonProperty("defenseDescription")]
    public string DefenseDescription;

    [JsonProperty("attackerTags")]
    public List<string> AttackerTags;

    [JsonProperty("defenderTags")]
    public List<string> DefenderTags;

    [JsonProperty("targetedResources")]
    public List<string> TargetedResources;

    [JsonProperty("protectedResources")]
    public List<string> ProtectedResources;

    [JsonProperty("winnerId")]
    public string WinnerId;

    [JsonProperty("lostResource")]
    public string LostResource;

    [JsonProperty("displayNarrative")]
    public string DisplayNarrative;
}

public class GameState
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("phase")]
    public string Phase;

    [JsonProperty("round")]
    public int Round;

    [JsonProperty("context")]
    public Context Context;

    [JsonProperty("players")]
    public List<Player> Players;

    [JsonProperty("factions")]
    public List<Faction> Factions;

    [JsonProperty("matches")]
    public List<Match> Matches;
}
