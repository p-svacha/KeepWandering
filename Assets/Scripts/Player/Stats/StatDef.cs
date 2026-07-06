using System.Collections.Generic;
using UnityEngine;

public class StatDef : Def
{
    public override string DefTypeLabel => "Stat";

    public StatDef(string defName) : base(defName) { }

    public string Abbreviation { get; init; }
}

public static class StatDefs
{
    public static List<StatDef> Defs = new List<StatDef>()
    {
        new StatDef("Morale")
        {
            Label = "morale",
            Abbreviation = "MOR",
            Description = "Your general mental state and overall emotional well-being.\n\nActs as a small offset to all skill checks.",
        },

        new StatDef("Strength")
        {
            Label = "strength",
            Abbreviation = "STR",
            Description = "Your raw physical force.\n\nUseful for activities like fighting, forcing things open, breaking things, moving heavy objects, climbing.",
        },

        new StatDef("Dexterity")
        {
            Label = "dexterity",
            Abbreviation = "DXT",
            Description = "Your bodily coordination and control.\n\nUseful for activities like lockpicking, sneaking, disarming traps, bypassing obstacles or interacting with small objects.",
        },

        new StatDef("Survival")
        {
            Label = "survival",
            Abbreviation = "SRV",
            Description = "Your general field knowledge and awareness.\n\nUseful for activities like scavenging, medical actions, scouting, camping, and navigating the environment.",
        },

        new StatDef("Social")
        {
            Label = "social",
            Abbreviation = "SCL",
            Description = "Your people skills.\n\nUseful for activities like persuading others, trading, pleading, intimidating, and any interaction where how you speak matters more than what you do.",
        },
    };
}

[DefOf]
public class StatDefOf
{
    public static StatDef Morale;

    public static StatDef Strength;
    public static StatDef Dexterity;
    public static StatDef Survival;
    public static StatDef Social;
}
