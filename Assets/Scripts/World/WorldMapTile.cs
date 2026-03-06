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
    public BiomeDef Biome { get; private set; }
    public LocationEncounter Encounter { get; private set; }
    public Mission Mission { get; private set; }

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

        // Calculate hex tile distance from start (0,0) using cube coordinates (odd-r offset)
        int col = coordinates.x;
        int row = coordinates.y;
        int q = col - (row - (row & 1)) / 2;
        int r = row;
        int s = -q - r;
        DistanceFromStart = (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2;
    }

    public void SetBiome(BiomeDef biome)
    {
        Biome = biome;
        WorldMapRenderer.Instance.FillTile(this);
    }

    public void SetEncounter(LocationEncounter encounter)
    {
        Encounter = encounter;
        WorldMapRenderer.Instance.SetMarkerTile(this, encounter.Def);
    }

    public void SetMission(Mission mission)
    {
        Mission = mission;
        WorldMapRenderer.Instance.SetMissionMarker(this, mission);
    }

    #region Getters

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

    public Vector2 North => WorldPosition + new Vector2(0f, 0.5f);
    public Vector2 NorthEast => WorldPosition + new Vector2(HelperFunctions.HEXAGON_SIDE2SIDE / 2f, 0.25f);
    public Vector2 SouthEast => WorldPosition + new Vector2(HelperFunctions.HEXAGON_SIDE2SIDE / 2f, -0.25f);
    public Vector2 South => WorldPosition + new Vector2(0f, -0.5f);
    public Vector2 SouthWest => WorldPosition + new Vector2(-HelperFunctions.HEXAGON_SIDE2SIDE / 2f, -0.25f);
    public Vector2 NorthWest => WorldPosition + new Vector2(-HelperFunctions.HEXAGON_SIDE2SIDE / 2f, 0.25f);

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

    public override string ToString()
    {
        string info = Biome.ToString();
        if (Encounter != null) info += ", " + Encounter.Label;
        if (Mission != null) info += ", Mission marker for \"" + Mission.Text + "\"";
        // info += "\nDistance from start: " + DistanceFromStart; // debug
        return info;
    }


    #endregion
}
