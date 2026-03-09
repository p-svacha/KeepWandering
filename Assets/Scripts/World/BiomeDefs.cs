using System.Collections.Generic;
using UnityEngine;

public static class BiomeDefs
{
    public static List<BiomeDef> Defs => new List<BiomeDef>()
    {
        new BiomeDef()
        {
            DefName = "Woods",
            Label = "woods",
            Description = "A place of many wild animals and plants.",
            MostImportantStats = new List<StatDef>() { StatDefOf.Intelligence, StatDefOf.Perception, StatDefOf.Dexterity },
        },

        new BiomeDef()
        {
            DefName = "Outskirts",
            Label = "outskirts",
            Description = "A sparsely populated area that covers big areas around and between cities.",
            MostImportantStats = new List<StatDef>() { StatDefOf.Charisma, StatDefOf.Strength, StatDefOf.Dexterity },
            LootTable = new LootTable
            {
                { LootTables.Trash, 10 },
                { LootTables.Food, 7 },
                { LootTables.Drinks, 7 },
                { LootTables.Tools, 5 },
            },
        },

        new BiomeDef()
        {
            DefName = "City",
            Label = "city",
            Description = "A bustling urban area with many buildings and people.",
            MostImportantStats = new List<StatDef>() { StatDefOf.Combat, StatDefOf.Charisma, StatDefOf.Perception },
        },

        new BiomeDef()
        {
            DefName = "Lake",
            Label = "lake",
            Description = "A serene body of water surrounded by nature.",
            IsPassable = false,
        }
    };
}
