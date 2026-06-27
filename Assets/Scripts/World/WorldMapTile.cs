using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An instance of this class contains all information about a single hex tile on the world map.
/// </summary>
public class WorldMapTile
{
    // World
    private Dictionary<Vector2Int, WorldMapTile> AllTiles;
    public Vector2Int Coordinates { get; private set; }
    public int DistanceFromStart {  get; private set; }
    public Vector2 WorldPosition { get; private set; }
    public Dictionary<Direction, Vector2> CornerWorldPositions { get; private set; }
    public Dictionary<Direction, Vector2> SideMidpointWorldPositions { get; private set; }
    public BiomeDef Biome { get; private set; }
    public bool HasRoad { get; private set; }
    public LocationEncounter Encounter { get; private set; }
    public Quest Mission { get; private set; }
    public DangerLevelDef DangerLevel { get; private set; }
    public int NumVisits { get; private set; }
    public bool HasBeenVisited => NumVisits > 0;

    public List<Area> Areas { get; private set; }
    public Area City => Areas.FirstOrDefault(a => a.Type == AreaType.City);
    public Area Forest => Areas.FirstOrDefault(a => a.Type == AreaType.Forest);
    public Area Lake => Areas.FirstOrDefault(a => a.Type == AreaType.Lake); 


    public WorldMapTile(Dictionary<Vector2Int, WorldMapTile> allTiles, Vector2Int coordinates)
    {
        AllTiles = allTiles;
        Coordinates = coordinates;
        WorldPosition = WorldMapRenderer.Instance.GetWorldPosition(coordinates);
        Areas = new List<Area>();
        DangerLevel = DangerLevelDefOf.Safe;
        NumVisits = 0;

        DistanceFromStart = GetDistanceFromTile(Vector2Int.zero);

        CornerWorldPositions = new Dictionary<Direction, Vector2>()
        {
            { Direction.NE, WorldPosition + new Vector2(0.25f, HelperFunctions.HEXAGON_SIDE2SIDE / 2f) },
            { Direction.E, WorldPosition + new Vector2(0.5f, 0f) },
            { Direction.SE,  WorldPosition + new Vector2(0.25f, -HelperFunctions.HEXAGON_SIDE2SIDE / 2f) },
            { Direction.SW, WorldPosition + new Vector2(-0.25f, -HelperFunctions.HEXAGON_SIDE2SIDE / 2f) },
            { Direction.W, WorldPosition + new Vector2(-0.5f, 0f) },
            { Direction.NW, WorldPosition + new Vector2(-0.25f, HelperFunctions.HEXAGON_SIDE2SIDE / 2f) },
        };

        SideMidpointWorldPositions = new Dictionary<Direction, Vector2>()
        {
            { Direction.N, WorldPosition + new Vector2(0f, HelperFunctions.HEXAGON_SIDE2SIDE / 2f) },
            { Direction.NE, WorldPosition + new Vector2(0.25f, HelperFunctions.HEXAGON_SIDE2SIDE / 4f) },
            { Direction.SE, WorldPosition + new Vector2(0.25f, -HelperFunctions.HEXAGON_SIDE2SIDE / 4f) },
            { Direction.S, WorldPosition + new Vector2(0f, -HelperFunctions.HEXAGON_SIDE2SIDE / 4f * 2f) },
            { Direction.SW, WorldPosition + new Vector2(-0.25f, -HelperFunctions.HEXAGON_SIDE2SIDE / 4f) },
            { Direction.NW, WorldPosition + new Vector2(-0.25f, HelperFunctions.HEXAGON_SIDE2SIDE / 4f) },
        };
    }

    public void AddVisit() => NumVisits++;

    public void ModifyDangerLevel(int amount)
    {
        // Calculate target level
        int targetDangerLevel = (int)DangerLevel.DangerLevel + amount;
        if (targetDangerLevel < 0) targetDangerLevel = 0;
        int maxDangerLevel = DefDatabase<DangerLevelDef>.AllDefs.Max(dl => (int)dl.DangerLevel);
        if (targetDangerLevel > maxDangerLevel) targetDangerLevel = maxDangerLevel;

        // Set new level
        DangerLevel = DefDatabase<DangerLevelDef>.AllDefs.First(dl => (int)dl.DangerLevel == targetDangerLevel);
    }

    public int GetDistanceFromTile(Vector2Int coordinates)
    {
        // Convert to cube coordinates (odd-q offset, flat-top)
        int col1 = Coordinates.x;
        int row1 = Coordinates.y;
        int q1 = col1;
        int r1 = row1 - (col1 - (col1 & 1)) / 2;
        int s1 = -q1 - r1;

        int col2 = coordinates.x;
        int row2 = coordinates.y;
        int q2 = col2;
        int r2 = row2 - (col2 - (col2 & 1)) / 2;
        int s2 = -q2 - r2;

        return (Mathf.Abs(q1 - q2) + Mathf.Abs(r1 - r2) + Mathf.Abs(s1 - s2)) / 2;
    }

    public void SetBiome(BiomeDef biome)
    {
        Biome = biome;
        WorldMapRenderer.Instance.FillTile(this);
    }

    public void AddRoad() => HasRoad = true;

    public void SetEncounter(LocationEncounter encounter)
    {
        Encounter = encounter;
    }

    #region Getters

    public bool HasEncounter => Encounter != null;

    /// <summary>
    /// Returns the adjacent tile in a specified direction
    /// </summary>
    public WorldMapTile GetAdjacentTile(Direction dir)
    {
        Vector2Int adjCoord = HelperFunctions.GetAdjacentHexCoordinates(Coordinates, dir);
        AllTiles.TryGetValue(adjCoord, out WorldMapTile tile);
        return tile;
    }

    /// <summary>
    /// Returns all existing adjacent tiles of this tile.
    /// </summary>
    public List<WorldMapTile> GetAdjacentTiles()
    {
        List<WorldMapTile> adjacentTiles = new List<WorldMapTile>();
        foreach (Direction dir in HelperFunctions.GetAdjacentHexDirections())
        {
            WorldMapTile adjacentTile = GetAdjacentTile(dir);
            if (adjacentTile != null) adjacentTiles.Add(adjacentTile);
        }

        return adjacentTiles;
    }

    /// <summary>
    /// Returns if this tile has an adjacent tile in the specifies direction.
    /// </summary>
    public bool HasAdjacentTile(Direction dir)
    {
        return GetAdjacentTile(dir) != null;
    }

    public bool IsPassable()
    {
        return Biome.IsPassable;
    }

    /// <summary>
    /// Returns the city, forest or lake that is closest to this tile.
    /// </summary>
    /// <returns></returns>
    public Area GetClosestArea()
    {
        // Check if tile is within a city, forest or lake area
        if (City != null) return City;
        if (Forest != null) return Forest;
        if (Lake != null) return Lake;

        // Go outwards in rings of adjacent tiles until we find a city, forest or lake
        int ring = 1;
        while (true)
        {
            List<WorldMapTile> ringTiles = GetTilesInHexRing(ring).GetShuffledList();
            foreach (WorldMapTile tile in ringTiles)
            {
                if (tile.City != null) return tile.City;
                if (tile.Forest != null) return tile.Forest;
                if (tile.Lake != null) return tile.Lake;
            }
            ring++;
        }
    }

    public List<WorldMapTile> GetTilesInHexRing(int ring)
    {
        List<WorldMapTile> checkedTiles = new List<WorldMapTile>();
        List<WorldMapTile> prevRingTiles = new List<WorldMapTile>() { this };
        List<WorldMapTile> nextRingTiles = new List<WorldMapTile>();

        for (int i = 0; i < ring; i++)
        {
            nextRingTiles = new List<WorldMapTile>();
            foreach (WorldMapTile tile in prevRingTiles)
            {
                foreach (WorldMapTile adjacent in tile.GetAdjacentTiles())
                {
                    if (!checkedTiles.Contains(adjacent))
                    {
                        checkedTiles.Add(adjacent);
                        nextRingTiles.Add(adjacent);
                    }
                }
            }
            prevRingTiles = nextRingTiles;
        }

        return prevRingTiles;
    }

    public List<WorldMapTile> GetTilesInHexRadius(int radius, bool includeSelf = false)
    {
        List<WorldMapTile> tilesInRadius = new List<WorldMapTile>();
        if (includeSelf) tilesInRadius.Add(this);
        for (int r = 0; r <= radius; r++)
        {
            tilesInRadius.AddRange(GetTilesInHexRing(r));
        }
        return tilesInRadius;
    }

    public override string ToString() => $"{Coordinates} {Biome.LabelCapWord}";

    public string GetWorldMapInfo()
    {
        string info = $"{Coordinates} {Biome.LabelCapWord}";
        if (Encounter != null && !Encounter.IsHidden) info += ", " + Encounter.Label;
        if (Mission != null) info += ", Mission marker for \"" + Mission.Text + "\"";
        // info += "\nDistance from start: " + DistanceFromStart; // debug
        info += $", Distance: {GetDistanceFromTile(Game.Instance.CurrentPosition.Coordinates)}";
        if (HasBeenVisited) info += $", {DangerLevel.Label}";
        return info;
    }


    #endregion
}
