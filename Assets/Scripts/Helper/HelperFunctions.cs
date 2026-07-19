using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class HelperFunctions
{
    /// <summary>
    /// Modulo that handles negative values in a logical way.
    /// </summary>
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    #region Hex-Map (flat-top)

    public const float HEXAGON_SIDE2SIDE = 0.866f;
    public static List<Direction> GetAdjacentHexDirections()
    {
        return new List<Direction>() { Direction.N, Direction.NE, Direction.SE, Direction.S, Direction.SW, Direction.NW };
    }
    public static List<Direction> GetAdjacentSquareDirections()
    {
        return new List<Direction>() { Direction.N, Direction.E, Direction.S, Direction.W };
    }

    public static Direction GetNextHexDirectionClockwise(Direction dir)
    {
        return dir switch
        {
            Direction.N => Direction.NE,
            Direction.NE => Direction.SE,
            Direction.SE => Direction.S,
            Direction.S => Direction.SW,
            Direction.SW => Direction.NW,
            Direction.NW => Direction.N,
            _ => throw new System.Exception("Invalid hex direction")
        };
    }

    public static Direction GetOppositeHexDirection(Direction dir)
    {
        return dir switch
        {
            Direction.N => Direction.S,
            Direction.NE => Direction.SW,
            Direction.SE => Direction.NW,
            Direction.S => Direction.N,
            Direction.SW => Direction.NE,
            Direction.NW => Direction.SE,
            _ => throw new System.Exception("Invalid hex direction")
        };
    }

    public static Direction GetNextHexDirectionCounterClockwise(Direction dir)
    {
        return dir switch
        {
            Direction.N => Direction.NW,
            Direction.NW => Direction.SW,
            Direction.SW => Direction.S,
            Direction.S => Direction.SE,
            Direction.SE => Direction.NE,
            Direction.NE => Direction.N,
            _ => throw new System.Exception("Invalid hex direction")
        };
    }

    public static Vector2Int GetAdjacentHexCoordinates(Vector2Int source, Direction dir)
    {
        // N/S move along the row axis (cell-x) regardless of column parity
        if (dir == Direction.N) return new Vector2Int(source.x + 1, source.y);
        if (dir == Direction.S) return new Vector2Int(source.x - 1, source.y);

        // Diagonal offsets depend on column parity (cell-y is the column axis)
        bool isEvenColumn = Mod(source.y, 2) == 0;
        if (isEvenColumn)
        {
            if (dir == Direction.NE) return new Vector2Int(source.x, source.y + 1);
            if (dir == Direction.SE) return new Vector2Int(source.x - 1, source.y + 1);
            if (dir == Direction.SW) return new Vector2Int(source.x - 1, source.y - 1);
            if (dir == Direction.NW) return new Vector2Int(source.x, source.y - 1);
        }
        else
        {
            if (dir == Direction.NE) return new Vector2Int(source.x + 1, source.y + 1);
            if (dir == Direction.SE) return new Vector2Int(source.x, source.y + 1);
            if (dir == Direction.SW) return new Vector2Int(source.x, source.y - 1);
            if (dir == Direction.NW) return new Vector2Int(source.x + 1, source.y - 1);
        }

        throw new System.Exception("Invalid direction adjacency for hex tiles!");
    }

    /// <summary>
    /// Converts this hex grid's offset coordinates to cube coordinates. Derived from the adjacency
    /// rules in GetAdjacentHexCoordinates (column parity keyed off the y-coordinate).
    /// </summary>
    public static Vector3Int GetCubeCoordinates(Vector2Int offsetCoord)
    {
        int q = offsetCoord.y;
        int r = (offsetCoord.y - Mod(offsetCoord.y, 2)) / 2 - offsetCoord.x;
        int s = -q - r;
        return new Vector3Int(q, r, s);
    }

    /// <summary>
    /// Returns the straight-line hex distance (in tiles) between two offset coordinates on this hex grid.
    /// </summary>
    public static int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        Vector3Int cubeA = GetCubeCoordinates(a);
        Vector3Int cubeB = GetCubeCoordinates(b);
        return (Mathf.Abs(cubeA.x - cubeB.x) + Mathf.Abs(cubeA.y - cubeB.y) + Mathf.Abs(cubeA.z - cubeB.z)) / 2;
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
