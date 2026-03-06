using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The world is the playing board of the whole game. Holds the state of the world map and all functionality to alter the state.
/// </summary>
public class WorldMap
{
    public static WorldMap Instance { get; private set; }

    public float MinWorldX { get; private set; }
    public float MaxWorldX { get; private set; }
    public float MinWorldY { get; private set; }
    public float MaxWorldY { get; private set; }

    /// <summary>
    /// Dictionary containing all world tiles with their coordinates as the key.
    /// </summary>
    private Dictionary<Vector2Int, WorldMapTile> Tiles;

    public bool CanSelectDestination;

    // Areas
    public Area QuarantineZone { get; private set; }
    public List<Area> Cities { get; private set; }
    public List<Area> Forests { get; private set; }
    public List<Area> Lakes { get; private set; }

    public WorldMap(Dictionary<Vector2Int, WorldMapTile> tiles, Area quarantineZone, List<Area> cities, List<Area> forests, List<Area> lakes)
    {
        Instance = this;
        Tiles = tiles;
        QuarantineZone = quarantineZone;
        Cities = cities;
        Forests = forests;
        Lakes = lakes;
        UpdateMapBounds();
    }

    /// <summary>
    /// Updates the min and max world bounds values.
    /// </summary>
    public void UpdateMapBounds()
    {
        if (Tiles == null || Tiles.Count == 0) return;

        int minX = Tiles.Min(x => x.Key.x);
        int maxX = Tiles.Max(x => x.Key.x);
        int minY = Tiles.Min(x => x.Key.y);
        int maxY = Tiles.Max(x => x.Key.y);

        MinWorldX = minX * HelperFunctions.HEXAGON_SIDE2SIDE;
        MaxWorldX = maxX * HelperFunctions.HEXAGON_SIDE2SIDE;
        MinWorldY = minY * 0.75f;
        MaxWorldY = maxY * 0.75f;
    }

    #region Getters

    public WorldMapTile GetTile(Vector2Int coordinates)
    {
        WorldMapTile tile;
        Tiles.TryGetValue(coordinates, out tile);
        return tile;
    }
    public WorldMapTile GetTile(int x, int y)
    {
        return GetTile(new Vector2Int(x, y));
    }

    public WorldMapTile GetRandomTile(BiomeDef biome = null, bool empty = true, bool mustBorderFence = false)
    {
        List<WorldMapTile> candidateTiles = new List<WorldMapTile>(QuarantineZone.Tiles);

        if (biome != null)
        {
            candidateTiles = candidateTiles.Where(t => t.Biome == biome).ToList();
        }
        if (empty)
        {
            candidateTiles = candidateTiles.Where(t => t.Encounter == null).ToList();
        }
        if (mustBorderFence)
        {
            candidateTiles = candidateTiles.Where(t => QuarantineZone.IsOnPerimeter(t)).ToList();
        }

        return candidateTiles.RandomElement();
    }

    /// <summary>
    /// Returns the number of appearances of a specific encounter on the world map.
    /// </summary>
    public int GetNumAppearances(EncounterDef def)
    {
        int numAppearances = 0;
        foreach (WorldMapTile tile in Tiles.Values)
        {
            if (tile.Encounter != null && tile.Encounter.Def == def) numAppearances++;
        }
        return numAppearances;
    }

    #endregion
}
