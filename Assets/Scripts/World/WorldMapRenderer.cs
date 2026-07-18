using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/// <summary>
/// MonoBehaviour attached to the Grid GameObject. Responsible for everything regarding rendering a WorldMap.
/// </summary>
public class WorldMapRenderer : MonoBehaviour
{
    public static WorldMapRenderer Instance;

    public const float TILE_COLOR_VARIANCE = 0.04f; // Random variance applied to tile colors to make them look more natural
    public const float TREE_DENSITY_VARIANCE = 0.05f; // Random variance applied to tree density to make them look more natural

    private Game Game;

    [Header("Rendering")]
    public Camera MainCamera;
    public WorldMapCameraHandler RenderCamera;
    public RectTransform RenderTargetRect;

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
    public GameObject AreaLabelContainer;
    public TextMeshPro AreaLabelPrefab;

    [Header("Tile Elements")]
    public GameObject TileElementContainer;
    public const string TREES_PATH = "WorldMap/TileElements/Trees";

    private const int TILE_ELEMENT_ATTEMPTS = 100;
    private const float TILE_ELEMENT_SCATTER_RADIUS = 0.45f; // allows slight bleed into neighboring tiles
    private const string TILE_ELEMENT_SORTING_LAYER = "WorldMap";
    private const int TILE_ELEMENT_SORTING_ORDER = 20;

    [Header("Roads")]
    public GameObject RoadContainer;
    public Color RoadColor;

    private const string ROAD_SORTING_LAYER = "WorldMap";
    private const int ROAD_SORTING_ORDER = 30;
    private const float ROAD_WIDTH = 0.05f;


    private Color PathVisualizationColor = new Color(0.8f, 0f, 0f, 1f);
    public const float PATH_VISUALIZATION_WIDTH = 0.04f;

    // Special tiles
    private WorldMapTile HoveredTile;
    private List<WorldMapTile> HighlightedTiles = new List<WorldMapTile>();
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
        RenderCamera.SetZoom(WorldMapCameraHandler.DEFAULT_CAMERA_SIZE);
        RenderCamera.SetPosition(new Vector3(Game.CurrentPosition.WorldPosition.x, Game.CurrentPosition.WorldPosition.y, -10));
    }

    public void FocusTile(WorldMapTile tile)
    {
        RenderCamera.SetZoom(WorldMapCameraHandler.DEFAULT_CAMERA_SIZE);
        RenderCamera.SetPosition(new Vector3(tile.WorldPosition.x, tile.WorldPosition.y, -10));
    }
    public void FocusArea(Area area)
    {
        RenderCamera.SetZoom(WorldMapCameraHandler.DEFAULT_CAMERA_SIZE);
        RenderCamera.SetPosition(new Vector3(area.Center.x, area.Center.y, -10));
    }

    private void Update()
    {
        if (Program.Instance.State != ProgramState.Game) return;

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
            if (!(Game.WorldMap.CanSelectDestination && Game.GetNextPositionTiles().Contains(HoveredTile)))
            {
                SetTile(HoverTilemap, HoveredTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileMarker_Hover"));
            }
        }

        // Hide context menu
        if (HoveredTile != ContextMenuTile && !EventSystem.current.IsPointerOverGameObject()) UI_ContextMenu.Instance.Hide();

        // Update tile info text
        Game.UI.WorldMapMenu.TileInfoText.text = HoveredTile == null ? "" : HoveredTile.GetWorldMapInfo();
    }

    /// <summary>
    /// Handles clicking on a tile to select destination for the day.
    /// </summary>
    private void UpdateTileSelection()
    {
        if (!Game.WorldMap.CanSelectDestination) return;
        if (HoveredTile == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!GameUI.Instance.WorldMapMenu.gameObject.activeSelf) return;

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
    private void OnHoveredTileChanged(WorldMapTile oldTile, WorldMapTile newTile)
    {
        // If old tile was a selectable tile, reset to default
        if (Game.WorldMap.CanSelectDestination && Game.GetNextPositionTiles().Contains(oldTile))
        {
            SetTile(HighlightTilemap, oldTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed"));
        }

        // If new tile is a selectable tile, highlight
        if (Game.WorldMap.CanSelectDestination && Game.GetNextPositionTiles().Contains(newTile))
        {
            SetTile(HighlightTilemap, newTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed_Outline"));
        }
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
            PathHistoryRenderer.material = ResourceManager.LoadMaterial("WorldMap/PathHistoryMaterial");
            PathHistoryRenderer.startWidth = PATH_VISUALIZATION_WIDTH;
            PathHistoryRenderer.endWidth = PATH_VISUALIZATION_WIDTH;
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

    public void HighlightTile(WorldMapTile tile)
    {
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed"));
        HighlightedTiles.Add(tile);
    }

    public void UnhighlightAllTiles()
    {
        foreach (WorldMapTile tile in HighlightedTiles) SetTile(HighlightTilemap, tile.Coordinates, null);
        HighlightedTiles.Clear();
    }

    #endregion

    #region Tile Rendering

    /// <summary>
    /// Fills a tile in all tilemaps.
    /// </summary>
    public void FillTile(WorldMapTile tile)
    {
        SetTile(BaseTextureTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/HexTileBase"));
        Color targetColor = tile.Biome.BaseColor;
        targetColor.r += Random.Range(-TILE_COLOR_VARIANCE, TILE_COLOR_VARIANCE);
        targetColor.g += Random.Range(-TILE_COLOR_VARIANCE, TILE_COLOR_VARIANCE);
        targetColor.b += Random.Range(-TILE_COLOR_VARIANCE, TILE_COLOR_VARIANCE);
        SetTileColor(BaseTextureTilemap, tile.Coordinates, targetColor);
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

    public void UpdateMapBounds(WorldMap worldMap)
    {
        RenderCamera.SetBounds(worldMap.MinWorldX, worldMap.MinWorldY, worldMap.MaxWorldX, worldMap.MaxWorldY);
    }

    /// <summary>
    /// Scatters randomized sprites from a folder around a given center point. Rolls TILE_ELEMENT_ATTEMPTS times,
    /// each with 'density' chance to spawn one sprite at a random offset within the scatter radius. Sprites may
    /// overlap each other and may spill slightly into neighboring tiles.
    /// </summary>
    public void SpawnScatteredElements(Vector2 center, string spriteFolderPath, float density, float densityVariance, bool randomizeRotation, bool randomizeColor, float minScale, float maxScale, System.Func<Color> randomColorGenerator = null)
    {
        if (density <= 0f) return;

        density += Random.Range(-densityVariance, densityVariance);

        for (int i = 0; i < TILE_ELEMENT_ATTEMPTS; i++)
        {
            if (Random.value > density) continue;

            Vector2 offset = Random.insideUnitCircle * TILE_ELEMENT_SCATTER_RADIUS;
            Vector2 position = center + offset;

            Sprite sprite = ResourceManager.LoadRandomSprite(spriteFolderPath);

            GameObject obj = new GameObject("TileElement");
            obj.transform.SetParent(TileElementContainer.transform);
            obj.layer = TileElementContainer.layer;
            obj.transform.position = new Vector3(position.x, position.y, 0f);
            obj.transform.rotation = randomizeRotation ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) : Quaternion.identity;
            float scale = Random.Range(minScale, maxScale);
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = TILE_ELEMENT_SORTING_LAYER;
            renderer.sortingOrder = TILE_ELEMENT_SORTING_ORDER;

            if (randomizeColor && randomColorGenerator != null)
            {
                renderer.color = randomColorGenerator();
            }
        }
    }

    /// <summary>
    /// Populates every tile on the world map with its biome's background elements (trees, etc).
    /// Called once after world generation; elements are not regenerated afterward.
    /// </summary>
    public void PopulateTileElements(WorldMap worldMap)
    {
        foreach (WorldMapTile tile in worldMap.Tiles.Values)
        {
            SpawnScatteredElements(
                center: tile.WorldPosition,
                spriteFolderPath: TREES_PATH,
                density: tile.Biome.TreeDensity,
                densityVariance: TREE_DENSITY_VARIANCE,
                randomizeRotation: true,
                randomizeColor: true,
                minScale: 0.12f,
                maxScale: 0.15f,
                randomColorGenerator: GetRandomGreenShade
            );
        }
    }

    private static Color GetRandomGreenShade()
    {
        float hue = Random.Range(0.28f, 0.36f);
        float saturation = Random.Range(0.4f, 0.75f);
        float value = Random.Range(0.5f, 0.85f);
        return Color.HSVToRGB(hue, saturation, value);
    }

    #endregion

    #region Roads

    /// <summary>
    /// Renders the road network as a set of continuous LineRenderers. Each contiguous chain of road tiles
    /// between two "joints" (endpoints or branch points) becomes a single LineRenderer, so lines stay smooth
    /// through corners instead of being segmented per tile-pair. Handles branching road networks correctly,
    /// as well as pure closed loops (rare, but possible if roads happen to form a ring).
    /// </summary>
    public void RenderRoads(WorldMap worldMap)
    {
        HashSet<(WorldMapTile, WorldMapTile)> visitedEdges = new HashSet<(WorldMapTile, WorldMapTile)>();
        List<WorldMapTile> roadTiles = worldMap.Tiles.Values.Where(t => t.HasRoad).ToList();

        // Pass 1: draw every chain starting from a joint (endpoint or branch point, i.e. degree != 2)
        foreach (WorldMapTile tile in roadTiles)
        {
            List<WorldMapTile> roadNeighbors = GetRoadNeighbors(tile);
            if (roadNeighbors.Count == 2) continue; // mid-chain tile, reached by walking from a joint instead

            foreach (WorldMapTile neighbor in roadNeighbors)
            {
                if (IsEdgeVisited(visitedEdges, tile, neighbor)) continue;
                DrawRoadLine(WalkRoadChain(tile, neighbor, visitedEdges));
            }
        }

        // Pass 2: catch any remaining closed loops (every tile degree 2, no joints at all)
        foreach (WorldMapTile tile in roadTiles)
        {
            foreach (WorldMapTile neighbor in GetRoadNeighbors(tile))
            {
                if (IsEdgeVisited(visitedEdges, tile, neighbor)) continue;
                DrawRoadLine(WalkRoadChain(tile, neighbor, visitedEdges));
            }
        }
    }

    private List<WorldMapTile> GetRoadNeighbors(WorldMapTile tile) => tile.GetAdjacentTiles().Where(t => t.HasRoad).ToList();

    private bool IsEdgeVisited(HashSet<(WorldMapTile, WorldMapTile)> visited, WorldMapTile a, WorldMapTile b)
        => visited.Contains((a, b)) || visited.Contains((b, a));

    /// <summary>
    /// Walks from 'from' through 'firstStep' and continues straight through degree-2 tiles until hitting
    /// a joint or looping back to the start. Marks every traversed edge as visited.
    /// </summary>
    private List<WorldMapTile> WalkRoadChain(WorldMapTile from, WorldMapTile firstStep, HashSet<(WorldMapTile, WorldMapTile)> visitedEdges)
    {
        List<WorldMapTile> chain = new List<WorldMapTile>() { from };

        WorldMapTile previous = from;
        WorldMapTile current = firstStep;
        visitedEdges.Add((previous, current));

        while (true)
        {
            chain.Add(current);

            List<WorldMapTile> neighbors = GetRoadNeighbors(current);
            if (neighbors.Count != 2 || current == from) break; // joint reached, or loop closed

            WorldMapTile next = neighbors.First(t => t != previous);
            if (IsEdgeVisited(visitedEdges, current, next)) break; // safety net against malformed data

            visitedEdges.Add((current, next));
            previous = current;
            current = next;
        }

        return chain;
    }

    private void DrawRoadLine(List<WorldMapTile> chain)
    {
        if (chain.Count < 2) return;

        GameObject obj = new GameObject("Road");
        obj.transform.SetParent(RoadContainer.transform);
        obj.layer = RoadContainer.layer;

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.material = ResourceManager.LoadMaterial("WorldMap/Road");
        line.startWidth = ROAD_WIDTH;
        line.endWidth = ROAD_WIDTH;
        line.numCornerVertices = 4;
        line.textureMode = LineTextureMode.Tile;
        line.sortingLayerName = ROAD_SORTING_LAYER;
        line.sortingOrder = ROAD_SORTING_ORDER;
        line.startColor = RoadColor;
        line.endColor = RoadColor;

        line.positionCount = chain.Count;
        for (int i = 0; i < chain.Count; i++)
        {
            Vector2 pos = chain[i].RoadPosition;
            line.SetPosition(i, new Vector3(pos.x, pos.y, 0f));
        }
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
