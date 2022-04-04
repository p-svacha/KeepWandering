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

    public static T GetWeightedRandomElement<T>(Dictionary<T, float> weightDictionary, bool debug = false)
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

        if (debug)
        {
            Dictionary<T, float> normalizedProbabilites = weightDictionary.ToDictionary(x => x.Key, x => x.Value / probabilitySum * 100f);
            string probabilites = "Probabilites for " + typeof(T).FullName;
            foreach (KeyValuePair<T, float> kvp in normalizedProbabilites.OrderByDescending(x => x.Value)) probabilites += "\n" + (kvp.Key.Equals(chosenValue) ? "* " : "") + kvp.Key.ToString() + ": " + kvp.Value + "%";
            Debug.Log(probabilites);
        }

        if (resultFound) return chosenValue;

        if (probabilitySum == 0) throw new System.Exception("Can't return anything of " + typeof(T).FullName + " because all probabilities are 0");
        throw new System.Exception();
    }

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
}
