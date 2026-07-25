using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeDef : Def
{
    public override string DefTypeLabel => "Biome";
    public Sprite BackgroundSprite => ResourceManager.LoadSprite($"Backgrounds/{DefName}");
    public GameObject Visuals { get; private set; } // Container for background or particle stuff

    /// <summary>
    /// If the player can move on this tile.
    /// </summary>
    public bool IsPassable { get; init; } = true;

    /// <summary>
    /// General loot table for encounters that happen in this biome. This is used to add biome-specific items to the loot tables of encounters that happen in this biome, without having to modify the encounter defs themselves. It is also the table used when scavenging loot in the evening in this biome.
    /// </summary>
    public LootTable LootTable { get; init; } = null;

    /// <summary>
    /// Chance that placed evening traps trigger on wildlife during the night.
    /// </summary>
    public float TrapTriggerChance { get; init; } = 0f;

    /// <summary>
    /// Visual only. The base color of the biome.
    /// </summary>
    public Color BaseColor { get; init; }


    // Scattered sprite densities: Chance (0–1) for a sprite to be spawned on a tile of this biome, rolled independently.  E.g. 0.5 averages ~50 sprites per tile.
    public float TreeDensity { get; init; } = 0f;
    public float CityBuildingDensity { get; init; } = 0f;
    public float FieldDensity { get; init; } = 0f;


    public BiomeDef(string defName) : base(defName) { }

    public override void OnLoadingDefsDone()
    {
        Visuals = Game.Instance.BiomeBackgroundContainer.transform.Find(DefName).gameObject;
    }
}

public static class BiomeDefs
{
    public static List<BiomeDef> Defs => new List<BiomeDef>()
    {
        new BiomeDef("Woods")
        {
            Label = "woods",
            Description = "A place of many wild animals and plants.",
            LootTable = new LootTable
            {
                { ItemDefOf.Wood, 12 },
                { LootTables.Plants, 12 },
                { LootTables.Food, 10 },
                { LootTables.Drinks, 9 },
                { LootTables.Tools, 5 },
                { ItemDefOf.Rope, 5 },
                { LootTables.Medical, 1 },
            },
            TrapTriggerChance = 0.5f,
            // Rendering
            BaseColor = new Color(0.36f, 0.56f, 0.40f),
            TreeDensity = 0.95f,
        },

        new BiomeDef("Outskirts")
        {
            Label = "outskirts",
            Description = "A sparsely populated area that covers big areas around and between cities.",
            LootTable = new LootTable
            {
                { LootTables.Trash, 10 },
                { LootTables.Food, 7 },
                { LootTables.Drinks, 7 },
                { LootTables.Medical, 5 },
                { LootTables.Tools, 5 },
                { LootTables.Plants, 5 },
                { ItemDefOf.Wood, 3 },
            },
            TrapTriggerChance = 0.3f,
            // Rendering
            BaseColor = new Color(0.73f, 0.89f, 0.78f),
            TreeDensity = 0.12f,
            FieldDensity = 0.03f,
        },

        new BiomeDef("City")
        {
            Label = "city",
            Description = "A bustling urban area with many buildings and people.",
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
            TrapTriggerChance = 0.1f,
            // Rendering
            BaseColor = new Color(0.91f, 0.92f, 0.93f),
            TreeDensity = 0f,
            CityBuildingDensity = 0.25f,
        },

        new BiomeDef("Lake")
        {
            Label = "lake",
            Description = "A serene body of water surrounded by nature.",
            IsPassable = false,
            // Rendering
            BaseColor = new Color(0.61f, 0.75f, 0.98f),
            TreeDensity = 0.0f,
        }
    };
}

[DefOf]
public static class BiomeDefOf
{
    public static BiomeDef City;
    public static BiomeDef Lake;
    public static BiomeDef Outskirts;
    public static BiomeDef Woods;
}