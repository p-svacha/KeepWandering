using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

/// <summary>
/// The world is the playing board of the whole game.
/// One tile represents very roughly 1km^2.
/// </summary>
public class WorldMap : MonoBehaviour
{
    private Game Game;

    [Header("Rendering")]
    public Camera MainCamera;
    public CameraHandler RenderCamera;
    public RectTransform RenderTargetRect;
    public const float DEFAULT_ZOOM = 5f;

    [Header("Tilemaps")]
    public Grid HexGrid;
    public Tilemap BaseTextureTilemap;
    public Tilemap HighlightTilemap;
    public Tilemap HoverTilemap;

    [Header("Player Position")]
    public GameObject PlayerPositionMarker;

    public LineRenderer PathHistoryRenderer;
    private Color PathVisualizationColor = new Color(0.8f, 0f, 0f, 1f);
    private float PathVisualizationWidth = 0.2f;

    public float MinWorldX { get; private set; }
    public float MaxWorldX { get; private set; }
    public float MinWorldY { get; private set; }
    public float MaxWorldY { get; private set; }

    /// <summary>
    /// Dictionary containing all world tiles with their coordinates as the key.
    /// </summary>
    private Dictionary<Vector2Int, WorldMapTile> Tiles;

    /// <summary>
    /// Dictionary containing the unique instances of each biome.
    /// </summary>
    private Dictionary<LocationType, Location> Locations;

    // Special tiles
    private WorldMapTile HoveredTile;
    private List<WorldMapTile> GreenHighlightedTiles = new List<WorldMapTile>();
    private List<WorldMapTile> BlueHighlightedTiles = new List<WorldMapTile>();
    public bool CanSelectDestination;

    public void Init(Game game)
    {
        Game = game;

        Locations = new Dictionary<LocationType, Location>();
        Locations.Add(LocationType.City, new Loc_City());
        Locations.Add(LocationType.Woods, new Loc_Woods());
        Locations.Add(LocationType.MainRoad, new Loc_MainRoad());
    }

    public void ResetCamera()
    {
        RenderCamera.SetZoom(DEFAULT_ZOOM);
        RenderCamera.SetPosition(new Vector3(Game.CurrentPosition.WorldPosition.x, Game.CurrentPosition.WorldPosition.y, -10));
    }

    private void Update()
    {
        UpdatePlayerPosition();
        UpdatePathHistory();
        UpdateHoveredTile();
        UpdateTileSelection();
    }

    /// <summary>
    /// Handles which tile is currently hovered with cursor.
    /// </summary>
    private void UpdateHoveredTile()
    {
        // Get local position of cursor within our render rect
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RenderTargetRect, Input.mousePosition, MainCamera, out Vector2 localPoint);

        // Calcuate normalized cursor position within our render rect (0-1)
        Vector2 normalizedPointInRect = new Vector2((localPoint.x / RenderTargetRect.rect.width) + 0.5f, (localPoint.y / RenderTargetRect.rect.height) + 0.5f);

        // Get world position of cursor within map camera
        Vector3 cursorWorldPosition = RenderCamera.Camera.ViewportToWorldPoint(normalizedPointInRect);

        // Remove selection marker from previously hovered tile
        if (HoveredTile != null) SetTile(HoverTilemap, HoveredTile.Coordinates, null);

        // Identify new hovered tile
        if (normalizedPointInRect.x < 0 || normalizedPointInRect.x > 1 || normalizedPointInRect.y < 0 || normalizedPointInRect.y > 1) HoveredTile = null;
        else HoveredTile = GetTile(cursorWorldPosition);

        // Add selection marker to new hovered tile
        if (HoveredTile != null)
            SetTile(HoverTilemap, HoveredTile.Coordinates, ResourceManager.Singleton.TileMarkerTransparentWhite);

        // Update tile info text
        Game.UI.WorldMapMenu.TileInfoText.text = HoveredTile == null ? "" : HoveredTile.ToString();
    }

    /// <summary>
    /// Handles clicking on a tile to select destination for the day.
    /// </summary>
    private void UpdateTileSelection()
    {
        if (!CanSelectDestination) return;
        if (HoveredTile == null) return;
        if (!GreenHighlightedTiles.Contains(HoveredTile)) return;

        if(Input.GetMouseButtonDown(0)) Game.SetNextDayPosition(HoveredTile);
    }

    #region Player Position

    /// <summary>
    /// Updates position marker of player.
    /// </summary>
    private void UpdatePlayerPosition()
    {
        PlayerPositionMarker.transform.position = Game.CurrentPosition.WorldPosition;
    }

    private void UpdatePathHistory()
    {
        if (Game.PathHistory.Count >= 2)
        {
            PathHistoryRenderer.material = ResourceManager.Singleton.PathHistoryMaterial;
            PathHistoryRenderer.startWidth = PathVisualizationWidth;
            PathHistoryRenderer.endWidth = PathVisualizationWidth;
            PathHistoryRenderer.startColor = PathVisualizationColor;
            PathHistoryRenderer.endColor = PathVisualizationColor;
            PathHistoryRenderer.positionCount = Game.PathHistory.Count;
            for (int i = 0; i < Game.PathHistory.Count; i++)
            {
                PathHistoryRenderer.SetPosition(i, Game.PathHistory[i].WorldPosition);
            }
        }
    }

    #endregion

    #region Highlight

    public void HighlightTileGreen(WorldMapTile tile)
    {
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.Singleton.TileMarkerGreen);
        GreenHighlightedTiles.Add(tile);
    }

    public void UnhighlightAllGreenTiles()
    {
        foreach (WorldMapTile tile in GreenHighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        GreenHighlightedTiles.Clear();
    }

    public void HighlightTileBlue(WorldMapTile tile)
    {
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.Singleton.TileMarkerBlue);
        BlueHighlightedTiles.Add(tile);
    }

    public void UnhighlightAllBlueTiles()
    {
        foreach (WorldMapTile tile in BlueHighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        BlueHighlightedTiles.Clear();
    }

    #endregion

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
        LocationType locType = LocationType.MainRoad;
        if (ForestNoise.GetValue(coordinates) > 0.65f) locType = LocationType.Woods;
        newTile.SetLocation(Locations[locType]);

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

        RenderCamera.SetBounds(MinWorldX, MinWorldY, MaxWorldX, MaxWorldY);
    }

    /// <summary>
    /// Fills a tile in all tilemaps
    /// </summary>
    private void FillTile(WorldMapTile tile)
    {
        SetTile(BaseTextureTilemap, tile.Coordinates, tile.Location.BaseTextureTile);
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


