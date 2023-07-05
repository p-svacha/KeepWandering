using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// The world is the playing board of the whole game.
/// One tile represents very roughly 1km^2.
/// </summary>
public class WorldMap : MonoBehaviour
{
    [Header("Tilemaps")]
    public Grid HexGrid;
    public Tilemap Tilemap;

    public float MinWorldX { get; private set; }
    public float MaxWorldX { get; private set; }
    public float MinWorldY { get; private set; }
    public float MaxWorldY { get; private set; }

    // Tiles are stored in a dictionary, where the key is their coordinates
    private Dictionary<Vector2Int, WorldMapTile> Tiles;

    #region World Generation

    private PerlinNoise ForestNoise;

    public void GenerateWorld(int initalMapSize)
    {
        // Initialize noisemaps
        ForestNoise = new PerlinNoise();

        // Add initial tiles
        Tiles = new Dictionary<Vector2Int, WorldMapTile>();

        AddTile(Vector2Int.zero);

        for(int i = 0; i < initalMapSize - 1; i++)
        {
            ExpandRandomTile();
        }
    }

    /// <summary>
    /// Adds a random tile at the edge of the map.
    /// </summary>
    public void ExpandRandomTile()
    {
        List<Vector2Int> candidateCoordinates = new List<Vector2Int>();
        foreach (WorldMapTile tile in Tiles.Values)
        {
            foreach(Direction dir in HelperFunctions.GetAdjacentHexDirections())
            {
                if(!tile.HasAdjacentTile(dir))
                {
                    Vector2Int candidatePos = HelperFunctions.GetAdjacentHexCoordinates(tile.Coordinates, dir);
                    candidateCoordinates.Add(candidatePos);
                }
            }
        }

        Vector2Int chosenCoordinates = candidateCoordinates[Random.Range(0, candidateCoordinates.Count)];
        AddTile(chosenCoordinates);
    }

    /// <summary>
    /// Adds a tile at the specifies coordinates. Biome is set automatically.
    /// </summary>
    private void AddTile(Vector2Int coordinates)
    {
        // Create Tile
        WorldMapTile newTile = new WorldMapTile(this, coordinates);
        Tiles.Add(coordinates, newTile);

        // Set Biome
        LocationType loc = LocationType.Suburbs;
        if (ForestNoise.GetValue(coordinates) > 0.65f) loc = LocationType.Woods;
        newTile.SetLocation(loc);

        // Fill Tilemaps
        FillTile(newTile);

        UpdateMapBounds();
    }

    /// <summary>
    /// Updates the min and max world bounds values.
    /// </summary>
    private void UpdateMapBounds()
    {
        int minX = Tiles.Min(x => x.Key.x);
        int maxX = Tiles.Max(x => x.Key.x);
        int minY = Tiles.Min(x => x.Key.y);
        int maxY = Tiles.Max(x => x.Key.y);

        MinWorldX = minX * HelperFunctions.HEXAGON_SIDE2SIDE;
        MaxWorldX = maxX * HelperFunctions.HEXAGON_SIDE2SIDE;
        MinWorldY = minY * 0.75f;
        MaxWorldY = maxY * 0.75f;
    }

    /// <summary>
    /// Fills a tile in all tilemaps
    /// </summary>
    private void FillTile(WorldMapTile tile)
    {
        SetTile(Tilemap, tile.Coordinates, ResourceManager.Singleton.GetLocationTile(tile.Location));
    }

    #endregion

    #region Setters

    public void SetTile(Tilemap tilemap, Vector2Int coordinates, TileBase tile)
    {
        Vector3Int pos = new Vector3Int(coordinates.x, coordinates.y, 0);
        tilemap.SetTile(pos, tile);
    }

    public void SetTileColor(Tilemap tilemap, Vector2Int coordinates, Color c)
    {
        Vector3Int pos = new Vector3Int(coordinates.x, coordinates.y, 0);
        tilemap.SetTileFlags(pos, TileFlags.None);
        tilemap.SetColor(pos, c);
    }

    #endregion

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
    public WorldMapTile GetTile(Vector3 worldPosition)
    {
        Vector3Int tileCoords = HexGrid.LocalToCell(worldPosition);
        return GetTile(tileCoords.x, tileCoords.y);
    }

    #endregion
}


