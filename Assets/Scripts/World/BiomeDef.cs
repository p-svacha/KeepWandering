using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeDef : Def
{
    public override string DefTypeLabel => "Biome";
    public GameObject Visuals { get; private set; } // Container for background or particle stuff
    public Tile WorldMapTile { get; private set; }

    public bool IsPassable { get; init; } = true;
    public LootTable LootTable { get; init; } = new LootTable();
    

    public override void OnLoadingDefsDone()
    {
        base.OnLoadingDefsDone();

        Visuals = Game.Instance.BiomeBackgroundContainer.transform.Find(DefName).gameObject;
        WorldMapTile = TileGenerator.CreateTileFromTexture(ResourceManager.LoadTexture("Biomes/" + DefName));
    }
}
