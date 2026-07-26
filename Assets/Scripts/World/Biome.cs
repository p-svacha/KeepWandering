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
                { ItemDefOf.Wood, Rarity.VeryCommon },
                { ItemDefOf.Berries, Rarity.Common },
                { ItemDefOf.MedicinalHerbs, Rarity.Common },
                { ItemDefOf.Charcoal, Rarity.Occasional },
                { ItemDefOf.Bone, Rarity.Occasional },
                { ItemDefOf.Rope, Rarity.Occasional },
                { ItemDefOf.MeatRaw, Rarity.Rare },
                { ItemDefOf.NutSnack, Rarity.Rare },
                { ItemDefOf.Knife, Rarity.Rare },
                { ItemDefOf.Matchbox, Rarity.Rare },
                { ItemDefOf.Antidote, Rarity.VeryRare },
                { ItemDefOf.WaterBottle, Rarity.VeryRare },
                { ItemDefOf.Beans, Rarity.VeryRare },
                { ItemDefOf.Bandage, Rarity.VeryRare },
                { ItemDefOf.Shovel, Rarity.VeryRare },
                { ItemDefOf.Lighter, Rarity.VeryRare },
                { ItemDefOf.Trap, Rarity.VeryRare },
                { ItemDefOf.Crowbar, Rarity.ExtremelyRare },
                { ItemDefOf.MedicalKit, Rarity.ExtremelyRare },
                { ItemDefOf.Antibiotics, Rarity.ExtremelyRare },
                { ItemDefOf.OilLamp, Rarity.ExtremelyRare },
                { ItemDefOf.Lockpick, Rarity.ExtremelyRare },
                { ItemDefOf.Coin, Rarity.ExtremelyRare },
                { ItemDefOf.Tent, Rarity.ExtremelyRare },
                { ItemDefOf.Bedroll, Rarity.ExtremelyRare },
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
                { ItemDefOf.Beans, Rarity.Common },
                { ItemDefOf.WaterBottle, Rarity.Common },
                { ItemDefOf.NutSnack, Rarity.Occasional },
                { ItemDefOf.Berries, Rarity.Occasional },
                { ItemDefOf.Bandage, Rarity.Occasional },
                { ItemDefOf.MedicinalHerbs, Rarity.Occasional },
                { ItemDefOf.Rope, Rarity.Occasional },
                { ItemDefOf.Shovel, Rarity.Occasional },
                { ItemDefOf.Matchbox, Rarity.Occasional },
                { ItemDefOf.Coin, Rarity.Rare },
                { ItemDefOf.Knife, Rarity.Rare },
                { ItemDefOf.MedicalKit, Rarity.Rare },
                { ItemDefOf.Antibiotics, Rarity.Rare },
                { ItemDefOf.MeatRaw, Rarity.Rare },
                { ItemDefOf.Bone, Rarity.Rare },
                { ItemDefOf.Wood, Rarity.Rare },
                { ItemDefOf.Lighter, Rarity.Rare },
                { ItemDefOf.OilLamp, Rarity.Rare },
                { ItemDefOf.Crowbar, Rarity.VeryRare },
                { ItemDefOf.Antidote, Rarity.VeryRare },
                { ItemDefOf.Charcoal, Rarity.VeryRare },
                { ItemDefOf.Lockpick, Rarity.VeryRare },
                { ItemDefOf.Trap, Rarity.VeryRare },
                { ItemDefOf.Tent, Rarity.VeryRare },
                { ItemDefOf.Bedroll, Rarity.VeryRare },
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
                { ItemDefOf.Coin, Rarity.VeryCommon },
                { ItemDefOf.Bandage, Rarity.Common },
                { ItemDefOf.WaterBottle, Rarity.Common },
                { ItemDefOf.Beans, Rarity.Common },
                { ItemDefOf.Knife, Rarity.Occasional },
                { ItemDefOf.Antibiotics, Rarity.Occasional },
                { ItemDefOf.Lockpick, Rarity.Occasional },
                { ItemDefOf.Crowbar, Rarity.Occasional },
                { ItemDefOf.NutSnack, Rarity.Occasional },
                { ItemDefOf.Lighter, Rarity.Occasional },
                { ItemDefOf.Matchbox, Rarity.Occasional },
                { ItemDefOf.MedicalKit, Rarity.Rare },
                { ItemDefOf.Antidote, Rarity.Rare },
                { ItemDefOf.Rope, Rarity.Rare },
                { ItemDefOf.OilLamp, Rarity.Rare },
                { ItemDefOf.Charcoal, Rarity.Rare },
                { ItemDefOf.Bone, Rarity.VeryRare },
                { ItemDefOf.Wood, Rarity.VeryRare },
                { ItemDefOf.Shovel, Rarity.VeryRare },
                { ItemDefOf.Bedroll, Rarity.VeryRare },
                { ItemDefOf.MeatRaw, Rarity.ExtremelyRare },
                { ItemDefOf.Berries, Rarity.ExtremelyRare },
                { ItemDefOf.MedicinalHerbs, Rarity.ExtremelyRare },
                { ItemDefOf.Trap, Rarity.ExtremelyRare },
                { ItemDefOf.Tent, Rarity.ExtremelyRare },
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