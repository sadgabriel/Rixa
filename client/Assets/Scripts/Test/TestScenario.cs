using System;
using System.Collections.Generic;

[Serializable]
public class TestScenario
{
    public string name;
    public List<FactionTestData> factions;
}

[Serializable]
public class FactionTestData
{
    public string name;
    public string context;
    public string concept;
    public string conceptName;
    public string flaw;
    public string attack;
    public string defense;
}