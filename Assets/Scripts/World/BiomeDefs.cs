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
            LootTable = new LootTable
            {
                { LootTables.Food, 10 },
                { LootTables.Plants, 12 },
                { LootTables.Drinks, 9 },
                { LootTables.Tools, 5 },
                { ItemDefOf.Rope, 5 },
                { LootTables.Medical, 1 },
            },
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
                { LootTables.Medical, 5 },
                { LootTables.Tools, 5 },
                { LootTables.Plants, 5 },
            },
        },

        new BiomeDef()
        {
            DefName = "City",
            Label = "city",
            Description = "A bustling urban area with many buildings and people.",
            MostImportantStats = new List<StatDef>() { StatDefOf.Combat, StatDefOf.Charisma, StatDefOf.Perception },
            LootTable = new LootTable
            {
                { LootTables.Trash, 18 },
                { LootTables.Food, 15 },
                { LootTables.Drinks, 15 },
                { LootTables.Tools, 10 },
                { LootTables.Weapons, 12 },
                { LootTables.Medical, 20 },
                { ItemDefOf.Coin, 10 },
            },
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
