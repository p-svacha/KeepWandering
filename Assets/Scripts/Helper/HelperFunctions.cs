using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class HelperFunctions
{
    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    #region Hex-Map

    public const float HEXAGON_SIDE2SIDE = 0.866f;
    public static List<Direction> GetAdjacentHexDirections()
    {
        return new List<Direction>() { Direction.NE, Direction.E, Direction.SE, Direction.SW, Direction.W, Direction.NW };
    }
    public static List<Direction> GetAdjacentSquareDirections()
    {
        return new List<Direction>() { Direction.N, Direction.E, Direction.S, Direction.W };
    }

    public static Direction GetNextHexDirectionClockwise(Direction dir)
    {
        return dir switch
        {
            Direction.NW => Direction.NE,
            Direction.NE => Direction.E,
            Direction.E => Direction.SE,
            Direction.SE => Direction.SW,
            Direction.SW => Direction.W,
            Direction.W => Direction.NW,
            _ => throw new System.Exception("Invalid hex direction")
        };
    }

    public static Vector2Int GetAdjacentHexCoordinates(Vector2Int source, Direction dir)
    {
        if (dir == Direction.E) return new Vector2Int(source.x + 1, source.y);
        if (dir == Direction.W) return new Vector2Int(source.x - 1, source.y);
        if (source.y % 2 == 0)
        {
            if (dir == Direction.NW) return new Vector2Int(source.x - 1, source.y + 1);
            if (dir == Direction.NE) return new Vector2Int(source.x, source.y + 1);
            if (dir == Direction.SW) return new Vector2Int(source.x - 1, source.y - 1);
            if (dir == Direction.SE) return new Vector2Int(source.x, source.y - 1);
        }
        else
        {
            if (dir == Direction.NW) return new Vector2Int(source.x, source.y + 1);
            if (dir == Direction.NE) return new Vector2Int(source.x + 1, source.y + 1);
            if (dir == Direction.SW) return new Vector2Int(source.x, source.y - 1);
            if (dir == Direction.SE) return new Vector2Int(source.x + 1, source.y - 1);
        }

        throw new System.Exception("Invalid direction adjacency for hex tiles!");
    }

    #endregion

    #region String

    public static string GetItemListAsString(List<Item> items)
    {
        string s = "";
        foreach (Item item in items) s += " " + item.LabelCapWord + ",";
        s = s.TrimStart(' ');
        s = s.TrimEnd(',');
        return s;
    }

    #endregion

    #region UI

    /// <summary>
    /// Destroys all children of a GameObject immediately.
    /// </summary>
    public static void DestroyAllChildredImmediately(GameObject obj, int skipElements = 0)
    {
        int numChildren = obj.transform.childCount;
        for (int i = skipElements; i < numChildren; i++) GameObject.DestroyImmediate(obj.transform.GetChild(skipElements).gameObject);
    }

    #endregion
}
