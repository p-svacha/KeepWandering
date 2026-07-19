using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Collection of adjacent tiles within the world map that share some common properties.
/// </summary>
public class Area
{
    private WorldMapRenderer Renderer => WorldMapRenderer.Instance;
    public string Name { get; private set; }
    public Vector2 Center { get; private set; }
    public AreaType Type { get; private set; }
    public List<WorldMapTile> Tiles { get; private set; }

    // Visual
    private Color FENCE_COLOR = Color.white;
    public GameObject FenceObject;
    public GameObject LabelObject;

    // Label
    public static int SIZE_FOR_MIN_LABEL_SIZE = 4;
    public static int SIZE_MAX_MIN_LABEL_SIZE = 30;
    public static float MIN_LABEL_SIZE = 4;
    public static float MAX_LABEL_SIZE = 8;

    public int TileCount => Tiles.Count;

    public Area(string name, AreaType type, List<WorldMapTile> tiles)
    {
        Name = name;
        Type = type;
        Tiles = tiles;
        foreach (WorldMapTile tile in Tiles) tile.Areas.Add(this);

        // Center
        Vector2 center = Vector2.zero;
        foreach (WorldMapTile tile in Tiles) center += tile.WorldPosition;
        Center = center / Tiles.Count;
    }

    public void DrawPerimeterFence(Material material, float width = 0.03f)
    {
        if (FenceObject != null) GameObject.Destroy(FenceObject.gameObject);

        FenceObject = new GameObject(Name + " Fence");
        FenceObject.transform.SetParent(Renderer.transform);
        FenceObject.layer = Renderer.gameObject.layer;

        LineRenderer line = FenceObject.AddComponent<LineRenderer>();
        line.material = material;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = FENCE_COLOR;
        line.endColor = FENCE_COLOR;
        line.textureMode = LineTextureMode.RepeatPerSegment;
        line.loop = true;

        line.sortingLayerName = "WorldMap";
        line.sortingOrder = 100;

        List<Vector2> fencePositions = new List<Vector2>();
        foreach (WorldMapTile tile in GetOrderedPerimeterTiles()) fencePositions.Add(tile.RoadPosition);
        line.positionCount = fencePositions.Count;
        for (int i = 0; i < fencePositions.Count; i++) line.SetPosition(i, fencePositions[i]);
    }

    /// <summary>
    /// Shows a label for this area on the world map. Position is centered on the area and rotation aligns with the principal axis of the tile positions.
    /// </summary>
    public void ShowLabel()
    {
        HideLabel();

        float t = Mathf.InverseLerp(SIZE_FOR_MIN_LABEL_SIZE, SIZE_MAX_MIN_LABEL_SIZE, TileCount);
        float fontSize = Mathf.Lerp(MIN_LABEL_SIZE, MAX_LABEL_SIZE, t);

        // Calculate rotation based on principal axis of tile positions (PCA)
        float angle = 0f;
        if (Tiles.Count >= 2)
        {
            float covXX = 0f, covYY = 0f, covXY = 0f;
            foreach (WorldMapTile tile in Tiles)
            {
                float dx = tile.WorldPosition.x - Center.x;
                float dy = tile.WorldPosition.y - Center.y;
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
        LabelObject = GameObject.Instantiate(Renderer.AreaLabelPrefab.gameObject, Renderer.AreaLabelContainer.transform);
        LabelObject.name = Name + " Label";
        LabelObject.transform.position = new Vector3(Center.x, Center.y, 0f);
        LabelObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        TextMeshPro tmp = LabelObject.GetComponent<TextMeshPro>();
        tmp.text = Name;
        tmp.fontSize = fontSize;
    }

    /// <summary>
    /// Hides the label for this area.
    /// </summary>
    public void HideLabel()
    {
        if (LabelObject != null)
        {
            GameObject.Destroy(LabelObject);
            LabelObject = null;
        }
    }

    #region Getters

    public bool ContainsTile(WorldMapTile tile) => Tiles.Contains(tile);
    public bool IsOnPerimeter(WorldMapTile tile) => GetOrderedPerimeterTiles().Contains(tile);

    /// <summary>
    /// Walks the outside boundary of the area tile-by-tile (each consecutive tile adjacent to the previous),
    /// returning the perimeter tiles in that walk order.
    /// </summary>
    public List<WorldMapTile> GetOrderedPerimeterTiles()
    {
        // Find an unordered set of perimeter tiles first, to know which tiles qualify and to pick a start tile.
        // Checked via each tile's neighboring coordinates directly (not via WorldMapTile.GetAdjacentTiles(), which
        // silently omits neighbors that don't have a WorldMapTile object yet - which would wrongly hide perimeter
        // tiles if this Area is constructed before its outside neighbors exist, e.g. before the outer ring is generated).
        HashSet<Vector2Int> areaCoordinates = new HashSet<Vector2Int>(Tiles.Select(t => t.Coordinates));
        List<WorldMapTile> unorderedPerimeterTiles = new List<WorldMapTile>();

        foreach (WorldMapTile tile in Tiles)
        {
            foreach (Direction dir in HelperFunctions.GetAdjacentHexDirections())
            {
                Vector2Int adjCoord = HelperFunctions.GetAdjacentHexCoordinates(tile.Coordinates, dir);
                if (!areaCoordinates.Contains(adjCoord))
                {
                    unorderedPerimeterTiles.Add(tile);
                    break;
                }
            }
        }

        List<WorldMapTile> orderedTiles = new List<WorldMapTile>();
        if (unorderedPerimeterTiles.Count == 0) return orderedTiles;

        WorldMapTile currentTile = unorderedPerimeterTiles[0];
        Direction nextStartDir = Direction.N;
        bool perimeterDone = false;

        orderedTiles.Add(currentTile);

        while (!perimeterDone)
        {
            Direction currentDir = nextStartDir;
            nextStartDir = Direction.None;

            while (!perimeterDone && nextStartDir == Direction.None)
            {
                WorldMapTile adjTile = currentTile.GetAdjacentTile(currentDir);

                if (!Tiles.Contains(adjTile))
                {
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                }
                else if (unorderedPerimeterTiles.Contains(adjTile))
                {
                    currentTile = adjTile;

                    if (currentTile == orderedTiles[0])
                    {
                        perimeterDone = true;
                        break;
                    }

                    orderedTiles.Add(currentTile);

                    nextStartDir = HelperFunctions.GetNextHexDirectionClockwise(
                        HelperFunctions.GetNextHexDirectionClockwise(
                            HelperFunctions.GetNextHexDirectionClockwise(
                                HelperFunctions.GetNextHexDirectionClockwise(currentDir))));
                }
                else
                {
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                }

                if (orderedTiles.Count > Tiles.Count + 10) break;
            }
        }

        return orderedTiles;
    }

    public WorldMapTile GetRandomTile()
    {
        return Tiles[Random.Range(0, Tiles.Count)];
    }

    public WorldMapTile GetRandomPassableTile()
    {
        List<WorldMapTile> candidates = Tiles.Where(x => x.IsPassable()).ToList();
        return candidates[Random.Range(0, candidates.Count)];
    }

    #endregion
}

public enum AreaType
{
    QuarantineZone,
    City,
    Forest,
    Lake,
}