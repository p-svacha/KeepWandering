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
    public AreaType Type { get; private set; }
    public List<WorldMapTile> Tiles;
    private List<WorldMapTile> PerimeterTiles;

    // Visual
    private Color FENCE_COLOR = Color.white;
    public GameObject FenceObject;
    public GameObject LabelObject;

    public int TileCount => Tiles.Count;

    public Area(string name, AreaType type, List<WorldMapTile> tiles)
    {
        Name = name;
        Type = type;
        Tiles = tiles;
        PerimeterTiles = GetPerimeterTiles();
        foreach(WorldMapTile tile in Tiles) tile.Areas.Add(this);
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

        List<Vector2> fence = GetPerimeterPoints();
        line.positionCount = fence.Count;
        for (int i = 0; i < fence.Count; i++) line.SetPosition(i, fence[i]);
    }

    /// <summary>
    /// Shows a label for this area on the world map. Position is centered on the area and rotation aligns with the principal axis of the tile positions.
    /// </summary>
    public void ShowLabel(float fontSize)
    {
        HideLabel();

        // Calculate center position (centroid of all tiles)
        Vector2 center = Vector2.zero;
        foreach (WorldMapTile tile in Tiles) center += tile.WorldPosition;
        center /= Tiles.Count;

        // Calculate rotation based on principal axis of tile positions (PCA)
        float angle = 0f;
        if (Tiles.Count >= 2)
        {
            float covXX = 0f, covYY = 0f, covXY = 0f;
            foreach (WorldMapTile tile in Tiles)
            {
                float dx = tile.WorldPosition.x - center.x;
                float dy = tile.WorldPosition.y - center.y;
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
        LabelObject.transform.position = new Vector3(center.x, center.y, 0f);
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

    public bool IsInArea(WorldMapTile tile) => Tiles.Contains(tile);
    public bool IsOnPerimeter(WorldMapTile tile) => PerimeterTiles.Contains(tile);

    /// <summary>
    /// Returns the outside edge (fencing) points of the area,
    /// </summary>
    public List<Vector2> GetPerimeterPoints()
    {
        List<Vector2> perimeterPoints = new List<Vector2>();

        // Take a starting point
        WorldMapTile currentTile = PerimeterTiles[0];
        Direction nextStartDir = Direction.NW;
        bool perimeterDone = false;

        // Follow the outside perimeter until reaching the first point again
        while (!perimeterDone)
        {
            Direction currentDir = nextStartDir;
            nextStartDir = Direction.None;
            bool fenceDrawn = false;

            while(!perimeterDone && nextStartDir == Direction.None)
            {
                WorldMapTile adjTile = currentTile.GetAdjacentTile(currentDir);

                // Adjacent tile is not within area => draw fence + go to next direction
                if (!Tiles.Contains(adjTile))
                {
                    perimeterPoints.Add(GetPerimeterPointForDirection(currentTile, currentDir));
                    if (perimeterPoints.Count >= 1000 || (perimeterPoints.Count > 1 && perimeterPoints.First() == perimeterPoints.Last())) perimeterDone = true;
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                    fenceDrawn = true;
                }

                // Adjacent tile on perimeter and fence has been drawn already => continue on that tile
                else if (PerimeterTiles.Contains(adjTile) && fenceDrawn)
                {
                    currentTile = adjTile;
                    nextStartDir = HelperFunctions.GetNextHexDirectionClockwise(
                        HelperFunctions.GetNextHexDirectionClockwise(
                            HelperFunctions.GetNextHexDirectionClockwise(
                                HelperFunctions.GetNextHexDirectionClockwise(currentDir))));
                }

                // Adjacent tile is in area but on on perimeter => go to next direction
                else
                {
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                }
            }
        }

        return perimeterPoints;
    }

    /// <summary>
    /// Returns a list of all tiles bordering the outside of the area.
    /// </summary>
    private List<WorldMapTile> GetPerimeterTiles()
    {
        List<WorldMapTile> perimeterTiles = new List<WorldMapTile>();

        foreach(WorldMapTile tile in Tiles)
        {
            foreach(WorldMapTile adjTile in tile.GetAdjacentTiles())
            {
                if (!Tiles.Contains(adjTile) && !perimeterTiles.Contains(tile)) perimeterTiles.Add(tile);
            }
        }

        return perimeterTiles;
    }

    /// <summary>
    /// Returns the the edge point of a tile that corresponds to the where the a fence needs to be drawn.
    /// </summary>
    private Vector3 GetPerimeterPointForDirection(WorldMapTile tile, Direction dir)
    {
        return dir switch
        {
            Direction.NW => tile.North,
            Direction.NE => tile.NorthEast,
            Direction.E => tile.SouthEast,
            Direction.SE => tile.South,
            Direction.SW => tile.SouthWest,
            Direction.W => tile.NorthWest,
            _ => throw new System.Exception("Invalid hex direction")
        };
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
