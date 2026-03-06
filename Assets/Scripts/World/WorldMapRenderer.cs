using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// MonoBehaviour attached to the Grid GameObject. Responsible for everything regarding rendering a WorldMap.
/// </summary>
public class WorldMapRenderer : MonoBehaviour
{
    public static WorldMapRenderer Instance;

    private Game Game;

    [Header("Rendering")]
    public Camera MainCamera;
    public WorldMapCameraHandler RenderCamera;
    public RectTransform RenderTargetRect;
    public const float DEFAULT_ZOOM = 4f;

    [Header("Tilemaps")]
    public Grid HexGrid;
    public Tilemap BaseTextureTilemap;
    public Tilemap MarkerTilemap;
    public Tilemap HighlightTilemap;
    public Tilemap HoverTilemap;

    [Header("Player Position")]
    public LineRenderer PathHistoryRenderer;
    public GameObject PlayerPositionMarker;

    [Header("Area Labels")]
    public static float MIN_AREA_LABEL_SIZE = 6;
    public static float MAX_AREA_LABEL_SIZE = 7;
    public GameObject AreaLabelContainer;
    public TextMeshPro AreaLabelPrefab;

    
    private Color PathVisualizationColor = new Color(0.8f, 0f, 0f, 1f);
    private float PathVisualizationWidth = 0.2f;

    // Special tiles
    private WorldMapTile HoveredTile;
    private List<WorldMapTile> GreenHighlightedTiles = new List<WorldMapTile>();
    private List<WorldMapTile> BlueHighlightedTiles = new List<WorldMapTile>();
    private List<WorldMapTile> RedHighlightedTiles = new List<WorldMapTile>();
    private WorldMapTile ContextMenuTile;

    // Tile Cache
    public Dictionary<EncounterDef, Tile> EncounterMarkerCache;

    /// <summary>
    /// Called once.
    /// </summary>
    public void Init(Game game)
    {
        Instance = this;
        Game = game;

        // Cache
        EncounterMarkerCache = new Dictionary<EncounterDef, Tile>();
        foreach (EncounterDef def in DefDatabase<EncounterDef>.AllDefs)
        {
            if (def.Type == EncounterType.Morning) continue;
            if (def.Type == EncounterType.Biome) continue;
            if (def.Type == EncounterType.Night) continue;

            // Only interested in location encounters
            Tile markerTile = TileFactory.CreateTileFromSprite(def.WorldMapMarker);
            EncounterMarkerCache.Add(def, markerTile);
        }
    }

    public void ResetCamera()
    {
        RenderCamera.SetZoom(DEFAULT_ZOOM);
        RenderCamera.SetPosition(new Vector3(Game.CurrentPosition.WorldPosition.x, Game.CurrentPosition.WorldPosition.y, -10));
    }

    public void FocusTile(WorldMapTile tile)
    {
        RenderCamera.SetZoom(DEFAULT_ZOOM);
        RenderCamera.SetPosition(new Vector3(tile.WorldPosition.x, tile.WorldPosition.y, -10));
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
        WorldMapTile newHoveredTile;
        if (normalizedPointInRect.x < 0 || normalizedPointInRect.x > 1 || normalizedPointInRect.y < 0 || normalizedPointInRect.y > 1) newHoveredTile = null;
        else newHoveredTile = GetTileAtWorldPosition(cursorWorldPosition);
        if (newHoveredTile != HoveredTile) OnHoveredTileChanged(HoveredTile, newHoveredTile);
        HoveredTile = newHoveredTile;

        // Add selection marker to new hovered tile
        if (HoveredTile != null)
        {
            SetTile(HoverTilemap, HoveredTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileMarkerTransparentWhite"));
        }

        // Hide context menu
        if (HoveredTile != ContextMenuTile && !EventSystem.current.IsPointerOverGameObject()) Game.UI.ContextMenu.Hide();

        // Update tile info text
        Game.UI.WorldMapMenu.TileInfoText.text = HoveredTile == null ? "" : HoveredTile.ToString();
    }

    /// <summary>
    /// Handles clicking on a tile to select destination for the day.
    /// </summary>
    private void UpdateTileSelection()
    {
        if (!Game.WorldMap.CanSelectDestination) return;
        if (HoveredTile == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Game.GetNextPositionTiles().Contains(HoveredTile))
            {
                ContextMenuTile = HoveredTile;
                Game.SelectTileOnMap(ContextMenuTile);
            }
        }
    }

    /// <summary>
    /// Gets called when the hovered tile changed.
    /// </summary>
    private void OnHoveredTileChanged(WorldMapTile oldTile, WorldMapTile newTile) { }

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
            PathHistoryRenderer.material = ResourceManager.LoadMaterial("WorldMap/PathHistoryMaterial");
            PathHistoryRenderer.startWidth = PathVisualizationWidth;
            PathHistoryRenderer.endWidth = PathVisualizationWidth;
            PathHistoryRenderer.startColor = PathVisualizationColor;
            PathHistoryRenderer.endColor = PathVisualizationColor;
            PathHistoryRenderer.positionCount = Game.PathHistory.Count;
            PathHistoryRenderer.numCornerVertices = 2;
            PathHistoryRenderer.textureMode = LineTextureMode.Tile;
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
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileMarkerGreen"));
        GreenHighlightedTiles.Add(tile);
    }

    public void UnhighlightAllGreenTiles()
    {
        foreach (WorldMapTile tile in GreenHighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        GreenHighlightedTiles.Clear();
    }

    public void HighlightTileBlue(WorldMapTile tile)
    {
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileMarkerBlue"));
        BlueHighlightedTiles.Add(tile);
    }

    public void UnhighlightAllBlueTiles()
    {
        foreach (WorldMapTile tile in BlueHighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        BlueHighlightedTiles.Clear();
    }

    public void HighlightTileRed(WorldMapTile tile)
    {
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileMarkerRed"));
        RedHighlightedTiles.Add(tile);
    }

    public void UnhighlightAllRedTiles()
    {
        foreach (WorldMapTile tile in RedHighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        RedHighlightedTiles.Clear();
    }

    #endregion

    #region Tile Rendering

    /// <summary>
    /// Fills a tile in all tilemaps.
    /// </summary>
    public void FillTile(WorldMapTile tile)
    {
        SetTile(BaseTextureTilemap, tile.Coordinates, tile.Biome.WorldMapTile);
    }

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

    public void SetMarkerTile(WorldMapTile tile, EncounterDef encounter)
    {
        if (encounter == null)
        {
            SetTile(MarkerTilemap, tile.Coordinates, null);
        }
        else
        {
            Tile markerTile = EncounterMarkerCache[encounter];
            SetTile(MarkerTilemap, tile.Coordinates, markerTile);
        }
    }

    public void SetMissionMarker(WorldMapTile tile, Mission mission)
    {
        if (mission == null) SetTile(MarkerTilemap, tile.Coordinates, null);
        else SetTile(MarkerTilemap, tile.Coordinates, mission.MapMarker);
    }

    public void UpdateMapBounds(WorldMap worldMap)
    {
        RenderCamera.SetBounds(worldMap.MinWorldX, worldMap.MinWorldY, worldMap.MaxWorldX, worldMap.MaxWorldY);
    }

    #endregion

    #region Getters

    /// <summary>
    /// Converts a world position to hex coordinates and returns the corresponding tile.
    /// </summary>
    public WorldMapTile GetTileAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int tileCoords = HexGrid.LocalToCell(worldPosition);
        return Game.WorldMap.GetTile(tileCoords.x, tileCoords.y);
    }

    /// <summary>
    /// Converts hex coordinates to a world position.
    /// </summary>
    public Vector2 GetWorldPosition(Vector2Int coordinates)
    {
        Vector3 worldPos = HexGrid.CellToWorld(new Vector3Int(coordinates.x, coordinates.y, 0));
        return new Vector2(worldPos.x, worldPos.y);
    }

    #endregion
}
