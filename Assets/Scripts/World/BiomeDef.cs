using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeDef : Def
{
    public override string DefTypeLabel => "Biome";
    public Sprite BackgroundSprite => ResourceManager.LoadSprite($"Backgrounds/{DefName}");
    public GameObject Visuals { get; private set; } // Container for background or particle stuff
    public Tile WorldMapTile { get; private set; }
    public EncounterDef EveningEncounter { get; private set; }

    /// <summary>
    /// If the player can move on this tile.
    /// </summary>
    public bool IsPassable { get; init; } = true;

    /// <summary>
    /// General loot table for encounters that happen in this biome. This is used to add biome-specific items to the loot tables of encounters that happen in this biome, without having to modify the encounter defs themselves.
    /// </summary>
    public LootTable LootTable { get; init; } = null;

    /// <summary>
    /// Set of most important stats. Not directly used for anything at the moment, but can be used by encounters or the UI to determine which stats to show or emphasize for this biome.
    /// </summary>
    public List<StatDef> MostImportantStats { get; init; } = new List<StatDef>();

    /// <summary>
    /// Chance that placed evening traps trigger on wildlife during the night.
    /// </summary>
    public float TrapTriggerChance { get; init; } = 0f;


    public BiomeDef(string defName) : base(defName) { }

    public override void OnLoadingDefsDone()
    {
        Visuals = Game.Instance.BiomeBackgroundContainer.transform.Find(DefName).gameObject;
        WorldMapTile = TileFactory.CreateTileFromTexture(ResourceManager.LoadTexture("Biomes/" + DefName));

        if (DefDatabase<EncounterDef>.TryGetNamed($"BiomeEncounter_{DefName}", out var encounter))
        {
            EveningEncounter = encounter;
        }
        else EveningEncounter = EncounterDefOf.EveningFallback;
    }
}
