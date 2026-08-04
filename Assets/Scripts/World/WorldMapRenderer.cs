using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// MonoBehaviour attached to the Grid GameObject. Responsible for everything regarding rendering a WorldMap.
/// </summary>
public class WorldMapRenderer : MonoBehaviour
{
    public static WorldMapRenderer Instance;

    private Game Game;

    public const string WORLD_MAP_SORTING_LAYER = "WorldMap";
    public const float TILE_COLOR_VARIANCE = 0.04f; // Random variance applied to tile colors to make them look more natural

    [Header("Rendering")]
    public Camera MainCamera;
    public WorldMapCameraHandler RenderCamera;
    public RectTransform RenderTargetRect;

    [Header("Tilemaps")]
    public Grid HexGrid;
    public Tilemap BaseTextureTilemap;
    public Tilemap HighlightTilemap;
    public Tilemap HoverTilemap;

    [Header("Player Position")]
    public GameObject PlayerPositionMarker;

    public const int PLAYER_POSITION_MARKER_SORTING_ORDER = 24500;
    public const float PLAYER_POSITION_MARKER_SIZE = 0.1f;

    private bool IsMarkerMoving;
    private Vector2 MarkerMoveStartPos;
    private Vector2 MarkerMoveTargetPos;
    private float MarkerMoveDuration;
    private float MarkerMoveElapsed;

    public const float MARKER_MOVING_DURATION = GameUI.TRANSITION_FADE_TIME + 0.5f;

    [Header("Path History")]
    public GameObject PathHistoryContainer;

    public const float PATH_HISTORY_LINE_WIDTH = 0.08f;

    public const float PATH_HISTORY_OFFSET_STEP = 0.05f; // perpendicular fan-out distance per repeated edge traversal
    public const float PATH_HISTORY_MIN_OPACITY = 0.15f; // opacity floor for old path history

    private const int PATH_HISTORY_SORTING_ORDER = 20300;
    private const int PATH_HISTORY_RECENT_TILES = 5; // most recent tiles at full opacity
    private const int PATH_HISTORY_FADE_TILES = 15; // opacity reaches the floor by this age

    private int LastRenderedPathHistoryCount = -1;

    [Header("Encounter Sprites")]
    public GameObject EncounterSpriteContainer;
    public GameObject EncounterWorldMapSpritePrefab;

    public const int ENCOUNTER_SPRITE_SORTING_ORDER = 24000;
    public const float ENCOUNTER_SPRITE_SIZE = 0.12f;

    private bool RedrawEncounterSprites = true;
    public void MarkRedrawEncounterSprites() => RedrawEncounterSprites = true;

    [Header("Tile Elements")]
    public GameObject TileElementContainer;
    public const string TREES_PATH = "WorldMap/TileElements/Trees";

    private const int TILE_ELEMENT_ATTEMPTS = 100;
    private const float TILE_ELEMENT_SCATTER_RADIUS = 0.45f; // allows slight bleed into neighboring tiles
    private const int TILE_ELEMENT_SORTING_ORDER = 20;

    public const float TREE_DENSITY_VARIANCE = 0.05f; // Random variance applied to tree density to make them look more natural
    public const float FIELD_DENSITY_VARIANCE = 0.1f; // Random variance applied to field density to make them look more natural

    public const float FIELD_OVERLAP_PADDING = 0f; // extra world-unit gap enforced between field sprites, on top of their content radius
    public const float FIELD_OVERLAP_RADIUS_MULTIPLIER = 0.80f; // shrinks the content-radius check slightly, since sprite bounds may include a bit of soft/feathered edge padding

    [Header("Roads")]
    public GameObject RoadContainer;
    public Color RoadColor;

    private const int ROAD_SORTING_ORDER = 20000;
    private const float ROAD_WIDTH = 0.05f;

    private Color PathVisualizationColor = new Color(0.8f, 0f, 0f, 1f);

    [Header("Area Labels")]
    public GameObject AreaLabelContainer;
    public TextMeshPro AreaLabelPrefab;

    private const int AREA_LABEL_SORTING_ORDER = 20500;
    public static float HIDE_AREA_LABELS_BELOW_CAMERA_SIZE = 3.5f;

    private static Dictionary<Area, TextMeshPro> AreaLabels = new Dictionary<Area, TextMeshPro>();


    [Header("Overlays")]
    public Tilemap DangerOverlayTilemap;

    // Special tiles
    private WorldMapTile HoveredTile;
    private List<WorldMapTile> HighlightedTiles = new List<WorldMapTile>();

    #region Base

    /// <summary>
    /// Called once.
    /// </summary>
    public void Init(Game game)
    {
        Instance = this;
        Game = game;
        LastRenderedPathHistoryCount = -1;
        AreaLabels = new Dictionary<Area, TextMeshPro>();
        IsMarkerMoving = false;
    }

    private void OnEnable()
    {
        if (Game == null) return; // not yet initialized on first activation

        // Reset player position marker to current position, in case it was mid-animation when the world map was closed
        IsMarkerMoving = false;
        PlayerPositionMarker.transform.position = Game.CurrentPosition.WorldPosition;
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
        UpdateAreaLabels();
        UpdateEncounterSprites();
    }

    /// <summary>
    /// Handles which tile is currently hovered with cursor.
    /// </summary>
    private void UpdateHoveredTile()
    {
        // Get world position of cursor within map camera
        Vector3 cursorWorldPosition = GetCursorWorldPosition();

        // Remove selection marker from previously hovered tile
        if (HoveredTile != null) SetTile(HoverTilemap, HoveredTile.Coordinates, null);

        // Identify new hovered tile
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RenderTargetRect, Input.mousePosition, MainCamera, out Vector2 localPoint);
        Vector2 normalizedPointInRect = new Vector2((localPoint.x / RenderTargetRect.rect.width) + 0.5f, (localPoint.y / RenderTargetRect.rect.height) + 0.5f);

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
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!GameUI.Instance.WorldMapMenu.gameObject.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Game.GetNextPositionTiles().Contains(HoveredTile))
            {
                Game.SelectTileOnMap(HoveredTile);
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
            SetTile(HighlightTilemap, oldTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed_Thick"));
        }

        // If new tile is a selectable tile, highlight
        if (Game.WorldMap.CanSelectDestination && Game.GetNextPositionTiles().Contains(newTile))
        {
            SetTile(HighlightTilemap, newTile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed_Thick_Outline"));
        }
    }

    /// <summary>
    /// Updates visibility of area labels based on camera zoom level.
    /// </summary>
    private void UpdateAreaLabels()
    {
        bool showAreaLabels = RenderCamera.Camera.orthographicSize >= HIDE_AREA_LABELS_BELOW_CAMERA_SIZE;
        SetAreaLabelsVisible(showAreaLabels);

        // Scale label font size based on camera zoom level
        if (showAreaLabels)
        {
            foreach (var kvp in AreaLabels)
            {
                Area area = kvp.Key;
                TextMeshPro label = kvp.Value;
                float scaleFactor = RenderCamera.Camera.orthographicSize / WorldMapCameraHandler.DEFAULT_CAMERA_SIZE;
                label.fontSize = area.Type.LabelFontSize * scaleFactor;
            }
        }
    }

    /// <summary>
    /// Updates the sprites that represent discovered encounters on the world map.
    /// </summary>
    private void UpdateEncounterSprites()
    {
        if (RedrawEncounterSprites)
        {
            RedrawEncounterSprites = false;
            RebuildEncounterSprites();
        }

        // Scale according to camera zoom level
        float targetScale = WorldMapCameraHandler.Instance.Camera.orthographicSize * ENCOUNTER_SPRITE_SIZE;
        foreach (Transform child in EncounterSpriteContainer.transform)
        {
            child.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    }

    private void RebuildEncounterSprites()
    {
        HelperFunctions.DestroyAllChildredImmediately(EncounterSpriteContainer);

        foreach (WorldMapTile tile in Game.WorldMap.Tiles.Values)
        {
            if (tile.Encounter != null && tile.Encounter.IsVisible)
            {
                GameObject obj = GameObject.Instantiate(EncounterWorldMapSpritePrefab, EncounterSpriteContainer.transform);
                obj.transform.position = new Vector3(tile.WorldPosition.x, tile.WorldPosition.y, 0f);

                SpriteRenderer frameRenderer = obj.GetComponent<SpriteRenderer>();
                frameRenderer.sortingLayerName = WORLD_MAP_SORTING_LAYER;
                frameRenderer.sortingOrder = ENCOUNTER_SPRITE_SORTING_ORDER;

                SpriteRenderer innerRenderer = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
                innerRenderer.sprite = tile.Encounter.GetWorldMapSprite();
                innerRenderer.sortingLayerName = WORLD_MAP_SORTING_LAYER;
                innerRenderer.sortingOrder = ENCOUNTER_SPRITE_SORTING_ORDER + 1;
            }
        }
    }

    #endregion

    #region Player Position

    /// <summary>
    /// Updates position marker of player.
    /// </summary>
    private void UpdatePlayerPosition()
    {
        Vector2 targetPos;

        if (IsMarkerMoving)
        {
            MarkerMoveElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(MarkerMoveElapsed / MarkerMoveDuration);
            targetPos = Vector2.Lerp(MarkerMoveStartPos, MarkerMoveTargetPos, t);

            if (t >= 1f) IsMarkerMoving = false;
        }
        else
        {
            targetPos = Game.CurrentPosition.WorldPosition;
        }

        PlayerPositionMarker.transform.position = targetPos;
        float targetScale = WorldMapCameraHandler.Instance.Camera.orthographicSize * PLAYER_POSITION_MARKER_SIZE;
        PlayerPositionMarker.transform.localScale = new Vector3(targetScale, targetScale, 1f);

        SpriteRenderer markerRenderer = PlayerPositionMarker.GetComponent<SpriteRenderer>();
        markerRenderer.sortingLayerName = WORLD_MAP_SORTING_LAYER;
        markerRenderer.sortingOrder = PLAYER_POSITION_MARKER_SORTING_ORDER;
    }

    /// <summary>
    /// Animates the player position marker moving toward the given tile over the given duration, instead of
    /// snapping instantly. Used so the marker visibly travels while the screen fades to black on tile selection.
    /// </summary>
    public void StartMovingPlayerMarkerTo(WorldMapTile target)
    {
        float duration = MARKER_MOVING_DURATION;
        IsMarkerMoving = true;
        MarkerMoveStartPos = PlayerPositionMarker.transform.position;
        MarkerMoveTargetPos = target.WorldPosition;
        MarkerMoveDuration = Mathf.Max(duration, 0.0001f);
        MarkerMoveElapsed = 0f;
    }

    #endregion

    #region Path History

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
        return sign * magnitudeSteps * PATH_HISTORY_OFFSET_STEP;
    }

    /// <summary>
    /// Opacity for a path segment based on how many tiles old it is (0 = most recent move).
    /// Full opacity for the most recent PATH_HISTORY_RECENT_TILES tiles, fading linearly down to
    /// PATH_HISTORY_MIN_OPACITY by PATH_HISTORY_FADE_TILES tiles old, then holding at that floor.
    /// </summary>
    private float GetPathHistoryAlpha(int age)
    {
        if (age < PATH_HISTORY_RECENT_TILES) return 1f;
        if (age >= PATH_HISTORY_FADE_TILES) return PATH_HISTORY_MIN_OPACITY;

        float t = (age - PATH_HISTORY_RECENT_TILES) / (float)(PATH_HISTORY_FADE_TILES - PATH_HISTORY_RECENT_TILES);
        return Mathf.Lerp(1f, PATH_HISTORY_MIN_OPACITY, t);
    }

    private void DrawPathHistorySegment(Vector2 from, Vector2 to, float alpha)
    {
        GameObject obj = new GameObject("PathHistorySegment");
        obj.transform.SetParent(PathHistoryContainer.transform);
        obj.layer = PathHistoryContainer.layer;

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.material = ResourceManager.LoadMaterial("WorldMap/PathHistoryMaterial");
        line.startWidth = PATH_HISTORY_LINE_WIDTH;
        line.endWidth = PATH_HISTORY_LINE_WIDTH;
        line.numCornerVertices = 2;
        line.textureMode = LineTextureMode.Tile;
        line.textureScale = new Vector2(2.5f, 1f);
        line.sortingLayerName = WORLD_MAP_SORTING_LAYER;
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
        SetTile(HighlightTilemap, tile.Coordinates, ResourceManager.LoadTile("WorldMap/Tilemaps/TileFrame_Dashed_Thick"));
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
                SetTileColor(DangerOverlayTilemap, tile.Coordinates, tile.BaseDangerLevel.Color);
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

    public void UpdateMapBounds(WorldMap worldMap)
    {
        RenderCamera.SetBounds(worldMap.MinWorldX, worldMap.MinWorldY, worldMap.MaxWorldX, worldMap.MaxWorldY);
    }

    #endregion

    #region Scattered Tile Sprites

    private struct ScatteredElement
    {
        public Vector2 Position;
        public Sprite Sprite;
        public float RotationDegrees;
        public float Scale;
        public bool FlipX;
        public Color Color;
    }

    private const int MAX_QUADS_PER_MESH = 16000; // stays under the 65535-vertex limit of a 16-bit-indexed mesh (16000*4 = 64000)
    private Dictionary<Texture2D, Material> ScatteredElementMaterials = new Dictionary<Texture2D, Material>();

    /// <summary>
    /// Rolls TILE_ELEMENT_ATTEMPTS times, each with 'density' chance to add one randomized scattered element
    /// around the given tile's center. Does not spawn any GameObjects - elements are collected into 'results'
    /// so many tiles' worth can later be merged into a small number of combined meshes.
    /// <br/>If avoidOverlap is true, a candidate is rejected (and the attempt simply skipped) if it would land
    /// too close to any element already in 'results', where "too close" is based on each sprite's actual
    /// content radius (from its trimmed sprite bounds, not the full texture square) times its instance scale -
    /// approximating each sprite as a circle rather than using a fixed bounding box.
    /// </summary>
    private void CollectScatteredElements(WorldMapTile tile, string spriteFolderPath, float density, float densityVariance, bool randomizeRotation, bool randomizeColor, float minScale, float maxScale, bool randomFlipX, System.Func<Color> randomColorGenerator, List<ScatteredElement> results, bool avoidOverlap = false, float overlapPadding = 0f)
    {
        if (density <= 0f) return;

        density += Random.Range(-densityVariance, densityVariance);

        for (int i = 0; i < TILE_ELEMENT_ATTEMPTS; i++)
        {
            if (Random.value > density) continue;

            Vector2 offset = Random.insideUnitCircle * TILE_ELEMENT_SCATTER_RADIUS;
            Vector2 position = tile.WorldPosition + offset;

            Sprite sprite = ResourceManager.LoadRandomSprite(spriteFolderPath);
            float scale = Random.Range(minScale, maxScale);

            if (avoidOverlap && HasOverlap(position, GetSpriteContentRadius(sprite, scale) + overlapPadding, results))
            {
                continue; // reject this attempt; don't count against the remaining rolls otherwise
            }

            results.Add(new ScatteredElement
            {
                Position = position,
                Sprite = sprite,
                RotationDegrees = randomizeRotation ? Random.Range(0f, 360f) : 0f,
                Scale = scale,
                FlipX = randomFlipX && Random.value > 0.5f,
                Color = randomizeColor && randomColorGenerator != null ? randomColorGenerator() : Color.white
            });
        }
    }

    /// <summary>
    /// Approximates a sprite instance's actual painted footprint as a circle, using its trimmed sprite
    /// bounds (requires Mesh Type = Tight on import) times the instance's scale.
    /// </summary>
    private float GetSpriteContentRadius(Sprite sprite, float scale)
    {
        Bounds bounds = sprite.bounds;
        float avgExtent = (bounds.extents.x + bounds.extents.y) / 2f;
        return avgExtent * scale * FIELD_OVERLAP_RADIUS_MULTIPLIER;
    }

    private bool HasOverlap(Vector2 position, float radius, List<ScatteredElement> existing)
    {
        foreach (ScatteredElement e in existing)
        {
            float existingRadius = GetSpriteContentRadius(e.Sprite, e.Scale);
            float minDist = radius + existingRadius;
            if ((e.Position - position).sqrMagnitude < minDist * minDist) return true;
        }
        return false;
    }

    /// <summary>
    /// Populates every tile on the world map with its biome's background elements (trees, city buildings),
    /// merging each element type into a small, fixed number of combined meshes rather than one GameObject
    /// per element - avoiding one draw call per scattered sprite (~10k+ individually otherwise).
    /// Called once after world generation; elements are not regenerated afterward.
    /// </summary>
    public void PopulateTileElements(WorldMap worldMap)
    {
        List<ScatteredElement> trees = new List<ScatteredElement>();
        List<ScatteredElement> buildings = new List<ScatteredElement>();
        List<ScatteredElement> fields = new List<ScatteredElement>();

        foreach (WorldMapTile tile in worldMap.Tiles.Values)
        {
            CollectScatteredElements(tile, TREES_PATH, tile.Biome.TreeDensity, TREE_DENSITY_VARIANCE,
                randomizeRotation: true, randomizeColor: true, minScale: 0.12f, maxScale: 0.15f,
                randomFlipX: true, randomColorGenerator: GetRandomGreenShade, results: trees);

            CollectScatteredElements(tile, "WorldMap/TileElements/CityBuildings", tile.Biome.CityBuildingDensity, 0f,
                randomizeRotation: false, randomizeColor: true, minScale: 0.20f, maxScale: 0.25f,
                randomFlipX: true, randomColorGenerator: GetRandomCityBuildingColor, results: buildings);

            CollectScatteredElements(tile, "WorldMap/TileElements/Fields", tile.Biome.FieldDensity, FIELD_DENSITY_VARIANCE,
                randomizeRotation: false, randomizeColor: true, minScale: 0.5f, maxScale: 0.55f,
                randomFlipX: true, randomColorGenerator: GetRandomFieldColor, results: fields,
                avoidOverlap: true, overlapPadding: FIELD_OVERLAP_PADDING);
        }

        // Trees: flat sorting order, submission order within the mesh doesn't matter for correctness
        BuildScatteredElementMeshes(trees, TileElementContainer, "Trees", TILE_ELEMENT_SORTING_ORDER);

        // Buildings: sort by Y (descending) so triangles submit back-to-front within the merged mesh -
        // this bakes the old per-instance Y-sort behavior into triangle order instead of sortingOrder,
        // since a single merged Renderer can only have one sortingOrder.
        buildings.Sort((a, b) => b.Position.y.CompareTo(a.Position.y));
        BuildScatteredElementMeshes(buildings, TileElementContainer, "CityBuildings", TILE_ELEMENT_SORTING_ORDER);

        // Fields: sort by Y (descending) so triangles submit back-to-front within the merged mesh -
        // this bakes the old per-instance Y-sort behavior into triangle order instead of sortingOrder,
        // since a single merged Renderer can only have one sortingOrder.
        fields.Sort((a, b) => b.Position.y.CompareTo(a.Position.y));
        BuildScatteredElementMeshes(fields, TileElementContainer, "Fields", TILE_ELEMENT_SORTING_ORDER);
    }

    /// <summary>
    /// Builds one or more combined meshes (chunked to stay under Unity's per-mesh vertex limit) from a list of
    /// scattered elements, all sharing the source sprite sheet's texture as a single material - collapsing what
    /// would otherwise be one draw call per element into a small, fixed number of draw calls regardless of count.
    /// </summary>
    private void BuildScatteredElementMeshes(List<ScatteredElement> elements, GameObject container, string batchName, int sortingOrder)
    {
        if (elements.Count == 0) return;

        Texture2D texture = elements[0].Sprite.texture;
        Material material = GetOrCreateScatteredElementMaterial(texture);

        for (int chunkStart = 0; chunkStart < elements.Count; chunkStart += MAX_QUADS_PER_MESH)
        {
            int chunkCount = Mathf.Min(MAX_QUADS_PER_MESH, elements.Count - chunkStart);

            Vector3[] vertices = new Vector3[chunkCount * 4];
            Vector2[] uvs = new Vector2[chunkCount * 4];
            Color[] colors = new Color[chunkCount * 4];
            int[] triangles = new int[chunkCount * 6];

            for (int i = 0; i < chunkCount; i++)
            {
                AppendQuad(elements[chunkStart + i], vertices, uvs, colors, triangles, i);
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            GameObject obj = new GameObject($"{batchName}Batch{chunkStart / MAX_QUADS_PER_MESH}");
            obj.transform.SetParent(container.transform);
            obj.layer = container.layer;

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = WORLD_MAP_SORTING_LAYER;
            renderer.sortingOrder = sortingOrder;
        }
    }

    private Material GetOrCreateScatteredElementMaterial(Texture2D texture)
    {
        if (ScatteredElementMaterials.TryGetValue(texture, out Material mat)) return mat;

        mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = texture;
        ScatteredElementMaterials.Add(texture, mat);
        return mat;
    }

    /// <summary>
    /// Writes one quad's worth of vertex/uv/color/triangle data into the given arrays at slot 'index'.
    /// Builds a plain rectangular quad from the sprite's pixel rect (for UVs) and bounds (for local corner
    /// positions), independent of the sprite's own internal mesh/Mesh Type import setting.
    /// </summary>
    private void AppendQuad(ScatteredElement element, Vector3[] vertices, Vector2[] uvs, Color[] colors, int[] triangles, int index)
    {
        Sprite sprite = element.Sprite;
        Bounds bounds = sprite.bounds;

        float left = bounds.center.x - bounds.extents.x;
        float right = bounds.center.x + bounds.extents.x;
        float bottom = bounds.center.y - bounds.extents.y;
        float top = bounds.center.y + bounds.extents.y;

        Vector2[] localCorners = new Vector2[4]
        {
        new Vector2(left, bottom),
        new Vector2(right, bottom),
        new Vector2(right, top),
        new Vector2(left, top)
        };

        float rotationRad = element.RotationDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rotationRad);
        float sin = Mathf.Sin(rotationRad);

        int vertBase = index * 4;
        for (int c = 0; c < 4; c++)
        {
            Vector2 corner = localCorners[c] * element.Scale;
            float rotatedX = corner.x * cos - corner.y * sin;
            float rotatedY = corner.x * sin + corner.y * cos;

            vertices[vertBase + c] = new Vector3(element.Position.x + rotatedX, element.Position.y + rotatedY, 0f);
            colors[vertBase + c] = element.Color;
        }

        Rect rect = sprite.rect;
        Texture2D texture = sprite.texture;
        float u0 = rect.x / texture.width;
        float v0 = rect.y / texture.height;
        float u1 = (rect.x + rect.width) / texture.width;
        float v1 = (rect.y + rect.height) / texture.height;

        if (element.FlipX) (u0, u1) = (u1, u0);

        uvs[vertBase + 0] = new Vector2(u0, v0);
        uvs[vertBase + 1] = new Vector2(u1, v0);
        uvs[vertBase + 2] = new Vector2(u1, v1);
        uvs[vertBase + 3] = new Vector2(u0, v1);

        int triBase = index * 6;
        triangles[triBase + 0] = vertBase + 0;
        triangles[triBase + 1] = vertBase + 2;
        triangles[triBase + 2] = vertBase + 1;
        triangles[triBase + 3] = vertBase + 0;
        triangles[triBase + 4] = vertBase + 3;
        triangles[triBase + 5] = vertBase + 2;
    }

    private static Color GetRandomGreenShade()
    {
        float hue = Random.Range(0.28f, 0.36f);
        float saturation = Random.Range(0.1f, 0.35f);
        float value = Random.Range(0.5f, 0.85f);
        return Color.HSVToRGB(hue, saturation, value);
    }

    private static Color GetRandomCityBuildingColor()
    {
        float hue = Random.value;
        float saturation = Random.Range(0f, 0.2f);
        float value = Random.Range(0.85f, 1f);
        return Color.HSVToRGB(hue, saturation, value);
    }

    private static Color GetRandomFieldColor()
    {
        List<Color> baseColors = new List<Color>()
        {
            //new Color(0.90f, 0.80f, 0.38f), // wheat
            new Color(0.53f, 0.46f, 0.25f), // soil
            new Color(0.38f, 0.47f, 0.29f), // green
        };
        float colorVariance = 0.05f;

        Color baseColor = baseColors[Random.Range(0, baseColors.Count)];
        float r = Mathf.Clamp01(baseColor.r + Random.Range(-colorVariance, colorVariance));
        float g = Mathf.Clamp01(baseColor.g + Random.Range(-colorVariance, colorVariance));
        float b = Mathf.Clamp01(baseColor.b + Random.Range(-colorVariance, colorVariance));
        //return new Color(r, g, b);

        // greenish-brownish hue, low saturation, high value
        float hue = Random.Range(0.15f, 0.5f);
        float saturation = Random.Range(0.1f, 0.25f);
        float value = Random.Range(0.80f, 0.95f);
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
        line.sortingLayerName = WORLD_MAP_SORTING_LAYER;
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

    #region Area Labels

    public void SetAreaLabelsVisible(bool visible)
    {
        AreaLabelContainer.SetActive(visible);
    }

    /// <summary>
    /// Generates a label for an area.
    /// </summary>
    public void GenerateLabel(Area area)
    {
        float fontSize = area.Type.LabelFontSize;

        // Calculate rotation based on principal axis of tile positions (PCA)
        float angle = 0f;
        if (area.Tiles.Count >= 2)
        {
            float covXX = 0f, covYY = 0f, covXY = 0f;
            foreach (WorldMapTile tile in area.Tiles)
            {
                float dx = tile.WorldPosition.x - area.Center.x;
                float dy = tile.WorldPosition.y - area.Center.y;
                covXX += dx * dx;
                covYY += dy * dy;
                covXY += dx * dy;
            }
            angle = 0.5f * Mathf.Atan2(2f * covXY, covXX - covYY) * Mathf.Rad2Deg;

            // Scale rotation down linearly so it stays within -30/30 range
            // (maps -90..90 to -30..30 while preserving proportions)
            //angle = angle * (30f / 90f);
        }

        // Instantiate label from prefab
        TextMeshPro label = GameObject.Instantiate(AreaLabelPrefab, AreaLabelContainer.transform);

        label.color = area.Type.LabelColor;
        label.name = area.Name + " Label";
        label.transform.position = new Vector3(area.Center.x, area.Center.y, 0f);
        label.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        label.sortingLayerID = SortingLayer.NameToID(WORLD_MAP_SORTING_LAYER);
        label.sortingOrder = AREA_LABEL_SORTING_ORDER;

        TextMeshPro tmp = label.GetComponent<TextMeshPro>();
        tmp.text = area.Name;
        tmp.fontSize = fontSize;

        AreaLabels[area] = label;
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

    /// <summary>
    /// Converts the current mouse screen position to a world position on the world map, accounting for the
    /// UI render-texture setup (mouse position is in UI/canvas space, not directly in the render camera's viewport).
    /// </summary>
    public Vector3 GetCursorWorldPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RenderTargetRect, Input.mousePosition, MainCamera, out Vector2 localPoint);
        Vector2 normalizedPointInRect = new Vector2((localPoint.x / RenderTargetRect.rect.width) + 0.5f, (localPoint.y / RenderTargetRect.rect.height) + 0.5f);
        return RenderCamera.Camera.ViewportToWorldPoint(normalizedPointInRect);
    }

    #endregion
}
