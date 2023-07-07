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

    #region Random

    public static T GetWeightedRandomElement<T>(Dictionary<T, float> weightDictionary)
    {
        if (weightDictionary.Any(x => x.Value < 0)) throw new System.Exception("Negative probability found for " + weightDictionary.First(x => x.Value < 0).Key.ToString());
        float probabilitySum = weightDictionary.Sum(x => x.Value);
        float rng = Random.Range(0, probabilitySum);
        float tmpSum = 0;
        T chosenValue = default(T);
        bool resultFound = false;
        foreach (KeyValuePair<T, float> kvp in weightDictionary)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum)
            {
                chosenValue = kvp.Key;
                resultFound = true;
                break;
            }
        }

        if (Game.DEBUG_RANDOM_CHOICES)
        {
            Dictionary<T, float> normalizedProbabilites = weightDictionary.ToDictionary(x => x.Key, x => x.Value / probabilitySum * 100f);
            string probabilites = "Probabilites for " + typeof(T).FullName;
            probabilites += "\n------------------------------";
            foreach (KeyValuePair<T, float> kvp in normalizedProbabilites.Where(x => x.Value > 0).OrderByDescending(x => x.Value)) probabilites += "\n" + (kvp.Key.Equals(chosenValue) ? "* " : "") + kvp.Key.ToString() + ": " + kvp.Value.ToString("0.0") + "%";
            probabilites += "\n------------------------------";
            Debug.Log(probabilites);
        }

        if (resultFound) return chosenValue;

        if (probabilitySum == 0) throw new System.Exception("Can't return anything of " + typeof(T).FullName + " because all probabilities are 0");
        throw new System.Exception();
    }

    #endregion

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

    public static string GetEnumDescription<T>(this T source)
    {
        FieldInfo fi = source.GetType().GetField(source.ToString());

        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
            typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0) return attributes[0].Description;
        else return source.ToString();
    }

    public static string GetItemListAsString(List<Item> items)
    {
        string s = "";
        foreach (Item item in items) s += " " + item.Name + ",";
        s = s.TrimStart(' ');
        s = s.TrimEnd(',');
        return s;
    }

    #endregion

    #region UI

    /// <summary>
    /// Destroys all children of a GameObject immediately.
    /// </summary>
    public static void DestroyAllChildredImmediately(GameObject obj)
    {
        int numChildren = obj.transform.childCount;
        for (int i = 0; i < numChildren; i++) GameObject.DestroyImmediate(obj.transform.GetChild(0).gameObject);
    }

    #endregion
}
