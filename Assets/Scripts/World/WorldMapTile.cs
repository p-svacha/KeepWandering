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
    public Vector2 WorldPosition { get; private set; }
    public Vector2 RoadPosition { get; private set; } // World position with some random offset for road placement
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
    public Area City => Areas.FirstOrDefault(a => a.Type == AreaTypeDefOf.City);
    public Area Forest => Areas.FirstOrDefault(a => a.Type == AreaTypeDefOf.Forest);
    public Area Lake => Areas.FirstOrDefault(a => a.Type == AreaTypeDefOf.Lake); 


    public WorldMapTile(Dictionary<Vector2Int, WorldMapTile> allTiles, Vector2Int coordinates)
    {
        AllTiles = allTiles;
        Coordinates = coordinates;
        WorldPosition = WorldMapRenderer.Instance.GetWorldPosition(coordinates);
        RoadPosition = WorldPosition + new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
        Areas = new List<Area>();
        DangerLevel = DangerLevelDefOf.Safe;
        NumVisits = 0;

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

    /// <summary>
    /// Returns the straight-line hex distance (in tiles) to the given tile, ignoring passability and roads.
    /// </summary>
    public int GetHexDistance(WorldMapTile other) => HelperFunctions.GetHexDistance(Coordinates, other.Coordinates);

    /// <summary>
    /// Returns the minimum number of in-game days required for the player to travel from the given source
    /// tile to this tile, accounting for passability and the road movement bonus (2 tiles per day when start,
    /// mid, and end tiles all have roads - mirrors Game.GetNextPositionTiles()).
    /// <br/>Returns -1 if this tile or the source tile is impassable, or if this tile is unreachable from the source.
    /// </summary>
    public int GetShortestPath(WorldMapTile source)
    {
        if (!IsPassable() || !source.IsPassable()) return -1;
        if (source == this) return 0;

        Queue<WorldMapTile> frontier = new Queue<WorldMapTile>();
        HashSet<WorldMapTile> visited = new HashSet<WorldMapTile>() { source };
        frontier.Enqueue(source);
        int days = 0;

        while (frontier.Count > 0)
        {
            int tilesAtCurrentDay = frontier.Count;
            days++;

            for (int i = 0; i < tilesAtCurrentDay; i++)
            {
                WorldMapTile current = frontier.Dequeue();

                foreach (WorldMapTile next in GetNextDayReachableTiles(current))
                {
                    if (visited.Contains(next)) continue;
                    if (next == this) return days;

                    visited.Add(next);
                    frontier.Enqueue(next);
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns all tiles reachable in a single day's move from the given tile - adjacent passable tiles,
    /// plus any 2-tile road-bonus destinations. Mirrors Game.GetNextPositionTiles().
    /// </summary>
    private static List<WorldMapTile> GetNextDayReachableTiles(WorldMapTile from)
    {
        List<WorldMapTile> tiles = new List<WorldMapTile>();

        foreach (WorldMapTile adj in from.GetAdjacentTiles())
        {
            if (adj.IsPassable()) tiles.Add(adj);
        }

        if (from.HasRoad)
        {
            foreach (WorldMapTile mid in tiles.Where(t => t.HasRoad).ToList())
            {
                foreach (WorldMapTile end in mid.GetAdjacentTiles())
                {
                    if (end == from) continue;
                    if (!end.HasRoad || !end.IsPassable()) continue;
                    if (!tiles.Contains(end)) tiles.Add(end);
                }
            }
        }

        return tiles;
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

    public int DistanceFromStart => GetHexDistance(WorldMap.Instance.StartTile);
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

    #endregion
}
