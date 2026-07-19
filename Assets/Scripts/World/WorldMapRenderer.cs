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
    public GameObject PathHistoryContainer;
    public GameObject PlayerPositionMarker;
    public float PathHistoryOffsetStep = 0.05f; // perpendicular fan-out distance per repeated edge traversal
    public float PathHistoryMinOpacity = 0.15f; // opacity floor for old path history

    private const int PATH_HISTORY_RECENT_TILES = 5; // most recent tiles at full opacity
    private const int PATH_HISTORY_FADE_TILES = 15; // opacity reaches the floor by this age
    private const string PATH_HISTORY_SORTING_LAYER = "WorldMap";
    private const int PATH_HISTORY_SORTING_ORDER = 17000; // adjust to sit correctly relative to roads/fence/markers

    private int LastRenderedPathHistoryCount = -1;

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
    private const int Y_SORT_PRECISION_MULTIPLIER = 10; // scales fractional Y differences into distinct integer sorting orders

    [Header("Roads")]
    public GameObject RoadContainer;
    public Color RoadColor;

    private const string ROAD_SORTING_LAYER = "WorldMap";
    private const int ROAD_SORTING_ORDER = 20000;
    private const float ROAD_WIDTH = 0.05f;


    private Color PathVisualizationColor = new Color(0.8f, 0f, 0f, 1f);
    public const float PATH_VISUALIZATION_WIDTH = 0.06f;

    [Header("Overlays")]
    public Tilemap DangerOverlayTilemap;

    // Special tiles
    private WorldMapTile HoveredTile;
    private List<WorldMapTile> HighlightedTiles = new List<WorldMapTile>();
    private WorldMapTile ContextMenuTile;

    // Tile Cache
    public Dictionary<EncounterDef, Tile> EncounterMarkerCache { get; private set; }

    /// <summary>
    /// Called once.
    /// </summary>
    public void Init(Game game)
    {
        Instance = this;
        Game = game;
        LastRenderedPathHistoryCount = -1;

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
        Game.UI.WorldMapMenu.ShowTileInfo(HoveredTile);
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
        float targetScale = WorldMapCameraHandler.Instance.Camera.orthographicSize * 0.1f;
        PlayerPositionMarker.transform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    private void UpdatePathHistory()
    {
        if (Game.PathHistory.Count == LastRenderedPathHistoryCount) return;
        LastRenderedPathHistoryCount = Game.PathHistory.Count;

        // Clear previous segments
        for (int i = PathHistoryContainer.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(PathHistoryContainer.transform.GetChild(i).gameObject);
        }

        if (Game.PathHistory.Count < 2) return;

        // Tracks how many times each undirected edge (tileA-tileB regardless of direction) has been
        // walked before, so repeat traversals can fan out to alternating sides instead of overlapping.
        Dictionary<(WorldMapTile, WorldMapTile), int> edgeOccurrences = new Dictionary<(WorldMapTile, WorldMapTile), int>();
        int lastIndex = Game.PathHistory.Count - 1;

        for (int i = 0; i < lastIndex; i++)
        {
            WorldMapTile tileA = Game.PathHistory[i];
            WorldMapTile tileB = Game.PathHistory[i + 1];

            // Canonical edge ordering, so the same edge walked in either direction maps to the same key
            // and fans out consistently rather than potentially cancelling itself out.
            bool aIsFirst = CompareTilesForEdgeKey(tileA, tileB) <= 0;
            WorldMapTile first = aIsFirst ? tileA : tileB;
            WorldMapTile second = aIsFirst ? tileB : tileA;
            var edgeKey = (first, second);

            edgeOccurrences.TryGetValue(edgeKey, out int occurrence);
            edgeOccurrences[edgeKey] = occurrence + 1;

            Vector2 dir = (second.WorldPosition - first.WorldPosition).normalized;
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            Vector2 offset = perpendicular * GetEdgeOffsetMagnitude(occurrence);

            // Age: how many tiles back the more recent end of this segment was visited, relative to now
            int age = lastIndex - (i + 1);
            float alpha = GetPathHistoryAlpha(age);

            DrawPathHistorySegment(tileA.WorldPosition + offset, tileB.WorldPosition + offset, alpha);
        }
    }

    private int CompareTilesForEdgeKey(WorldMapTile a, WorldMapTile b)
    {
        if (a.Coordinates.x != b.Coordinates.x) return a.Coordinates.x.CompareTo(b.Coordinates.x);
        return a.Coordinates.y.CompareTo(b.Coordinates.y);
    }

    /// <summary>
    /// Returns the perpendicular offset magnitude for the Nth traversal of an edge (0-indexed).
    /// 0 = centered (first traversal), then fans out alternately: +1, -1, +2, -2, ...
    /// </summary>
    private float GetEdgeOffsetMagnitude(int occurrenceIndex)
    {
        if (occurrenceIndex == 0) return 0f;
        int magnitudeSteps = (occurrenceIndex + 1) / 2;
        float sign = (occurrenceIndex % 2 == 1) ? 1f : -1f;
        return sign * magnitudeSteps * PathHistoryOffsetStep;
    }

    /// <summary>
    /// Opacity for a path segment based on how many tiles old it is (0 = most recent move).
    /// Full opacity for the most recent PATH_HISTORY_RECENT_TILES tiles, fading linearly down to
    /// PathHistoryMinOpacity by PATH_HISTORY_FADE_TILES tiles old, then holding at that floor.
    /// </summary>
    private float GetPathHistoryAlpha(int age)
    {
        if (age < PATH_HISTORY_RECENT_TILES) return 1f;
        if (age >= PATH_HISTORY_FADE_TILES) return PathHistoryMinOpacity;

        float t = (age - PATH_HISTORY_RECENT_TILES) / (float)(PATH_HISTORY_FADE_TILES - PATH_HISTORY_RECENT_TILES);
        return Mathf.Lerp(1f, PathHistoryMinOpacity, t);
    }

    private void DrawPathHistorySegment(Vector2 from, Vector2 to, float alpha)
    {
        GameObject obj = new GameObject("PathHistorySegment");
        obj.transform.SetParent(PathHistoryContainer.transform);
        obj.layer = PathHistoryContainer.layer;

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.material = ResourceManager.LoadMaterial("WorldMap/PathHistoryMaterial");
        line.startWidth = PATH_VISUALIZATION_WIDTH;
        line.endWidth = PATH_VISUALIZATION_WIDTH;
        line.numCornerVertices = 2;
        line.textureMode = LineTextureMode.Tile;
        line.textureScale = new Vector2(2.5f, 1f);
        line.sortingLayerName = PATH_HISTORY_SORTING_LAYER;
        line.sortingOrder = PATH_HISTORY_SORTING_ORDER;

        Color color = PathVisualizationColor;
        color.a *= alpha;
        line.startColor = color;
        line.endColor = color;

        line.positionCount = 2;
        line.SetPosition(0, new Vector3(from.x, from.y, 0f));
        line.SetPosition(1, new Vector3(to.x, to.y, 0f));
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

    #region Overlays

    public void SetDangerOverlayVisible(bool visible)
    {
        if (!visible)
        {
            DangerOverlayTilemap.ClearAllTiles();
            return;
        }

        else
        {
            foreach (WorldMapTile tile in WorldMap.Instance.QuarantineZone.Tiles)
            {
                SetTile(DangerOverlayTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/HexTileBase"));
                SetTileColor(DangerOverlayTilemap, tile.Coordinates, tile.DangerLevel.Color);
            }
        }
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
    public void SpawnScatteredElements(WorldMapTile tile, string spriteFolderPath, float density, float densityVariance, bool randomizeRotation, bool randomizeColor, float minScale, float maxScale, bool randomFlipX = false, bool sortByYPosition = false, System.Func<Color> randomColorGenerator = null)
    {
        if (density <= 0f) return;

        density += Random.Range(-densityVariance, densityVariance);

        for (int i = 0; i < TILE_ELEMENT_ATTEMPTS; i++)
        {
            if (Random.value > density) continue;

            Vector2 offset = Random.insideUnitCircle * TILE_ELEMENT_SCATTER_RADIUS;
            Vector2 position = tile.WorldPosition + offset;

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

            // Lower Y should draw in front (closer to "camera" in a 2D side-view sense), so sorting order
            // is inverted relative to Y - higher Y (further back) gets a lower order, lower Y gets a higher order.
            renderer.sortingOrder = sortByYPosition
                ? TILE_ELEMENT_SORTING_ORDER - Mathf.RoundToInt(position.y * Y_SORT_PRECISION_MULTIPLIER)
                : TILE_ELEMENT_SORTING_ORDER;

            if (randomizeColor && randomColorGenerator != null)
            {
                renderer.color = randomColorGenerator();
            }
            if (randomFlipX)
            {
                renderer.flipX = Random.value > 0.5f;
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
            // Trees
            SpawnScatteredElements(
                tile: tile,
                spriteFolderPath: TREES_PATH,
                density: tile.Biome.TreeDensity,
                densityVariance: TREE_DENSITY_VARIANCE,
                randomizeRotation: true,
                randomizeColor: true,
                minScale: 0.12f,
                maxScale: 0.15f,
                randomFlipX: true,
                sortByYPosition: false,
                randomColorGenerator: GetRandomGreenShade
            );

            // City buildings
            SpawnScatteredElements(
                tile: tile,
                spriteFolderPath: "WorldMap/TileElements/CityBuildings",
                density: tile.Biome.CityBuildingDensity,
                densityVariance: 0f,
                randomizeRotation: false,
                randomizeColor: true,
                minScale: 0.25f,
                maxScale: 0.30f,
                randomFlipX: true,
                sortByYPosition: true,
                randomColorGenerator: GetRandomCityBuildingColor
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

    private static Color GetRandomCityBuildingColor()
    {
        float hue = Random.value;
        float saturation = Random.Range(0f, 0.2f); // low saturation for grayish colors
        float value = Random.Range(0.85f, 1f); // bright colors as the base images alredy have some darker base colors
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
