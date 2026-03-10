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

    public bool IsPassable { get; init; } = true;
    public LootTable LootTable { get; init; } = new LootTable();
    public List<StatDef> MostImportantStats { get; init; } = new List<StatDef>();

    public override void OnLoadingDefsDone()
    {
        base.OnLoadingDefsDone();

        Visuals = Game.Instance.BiomeBackgroundContainer.transform.Find(DefName).gameObject;
        WorldMapTile = TileFactory.CreateTileFromTexture(ResourceManager.LoadTexture("Biomes/" + DefName));

        if (DefDatabase<EncounterDef>.TryGetNamed($"BiomeEncounter_{DefName}", out var encounter))
        {
            EveningEncounter = encounter;
        }
        else EveningEncounter = EncounterDefOf.EveningFallback;
    }
}
