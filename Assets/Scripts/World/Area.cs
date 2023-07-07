using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collection of adjacent tiles within the world map that share some common properties.
/// </summary>
public class Area
{
    private WorldMap World;
    public string Name;
    public List<WorldMapTile> Tiles;

    // Visual
    private const float FENCE_WIDTH = 0.03f;
    private Color FENCE_COLOR = Color.red;
    public GameObject FenceObject;

    public Area(WorldMap world, string name, List<WorldMapTile> tiles)
    {
        World = world;
        Name = name;
        Tiles = tiles;
    }

    public void DrawPerimeterFence()
    {
        if (FenceObject != null) GameObject.Destroy(FenceObject.gameObject);

        FenceObject = new GameObject(Name + " Fence");
        FenceObject.transform.SetParent(World.transform);
        FenceObject.layer = World.gameObject.layer;

        LineRenderer line = FenceObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startWidth = FENCE_WIDTH;
        line.endWidth = FENCE_WIDTH;
        line.startColor = FENCE_COLOR;
        line.endColor = FENCE_COLOR;

        line.sortingLayerName = "WorldMap";
        line.sortingOrder = 100;

        List<Vector2> fence = GetPerimeterPoints();
        line.positionCount = fence.Count;
        for (int i = 0; i < fence.Count; i++) line.SetPosition(i, fence[i]);
    }

    /// <summary>
    /// Returns the outside edge (fencing) points of the area,
    /// </summary>
    public List<Vector2> GetPerimeterPoints()
    {
        List<Vector2> perimeterPoints = new List<Vector2>();

        // Take a starting point
        List<WorldMapTile> perimeterTiles = GetPerimeterTiles();
        WorldMapTile currentTile = perimeterTiles[0];
        Direction nextStartDir = Direction.NW;
        bool perimeterDone = false;

        // Follow the outside perimeter until reaching the first point again
        while(!perimeterDone)
        {
            Debug.Log("nextstartdir is " + nextStartDir.ToString());
            Direction currentDir = nextStartDir;
            nextStartDir = Direction.None;

            while(!perimeterDone && nextStartDir == Direction.None)
            {
                WorldMapTile adjTile = currentTile.GetAdjacentTile(currentDir);
                if (!Tiles.Contains(adjTile))
                {
                    perimeterPoints.Add(GetPerimeterPointForDirection(currentTile, currentDir));
                    if (perimeterPoints.Count > 1 && perimeterPoints.First() == perimeterPoints.Last()) perimeterDone = true;
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                }
                else if(perimeterTiles.Contains(adjTile))
                {
                    currentTile = adjTile;
                    nextStartDir = HelperFunctions.GetNextHexDirectionClockwise(
                        HelperFunctions.GetNextHexDirectionClockwise(
                            HelperFunctions.GetNextHexDirectionClockwise(
                                HelperFunctions.GetNextHexDirectionClockwise(currentDir))));
                }
                else
                {
                    currentDir = HelperFunctions.GetNextHexDirectionClockwise(currentDir);
                }
            }
        }

        return perimeterPoints;
    }

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

    /// <summary>
    /// Returns a list of all tiles bordering the outside of the area.
    /// </summary>
    public List<WorldMapTile> GetPerimeterTiles()
    {
        List<WorldMapTile> perimeterTiles = new List<WorldMapTile>();

        foreach(WorldMapTile tile in Tiles)
        {
            foreach(WorldMapTile adjTile in tile.GetAdjacentTiles())
            {
                if (!Tiles.Contains(adjTile) && !perimeterTiles.Contains(tile)) perimeterTiles.Add(tile);
            }
        }

        Debug.Log(perimeterTiles.Count);

        return perimeterTiles;
    }

}
