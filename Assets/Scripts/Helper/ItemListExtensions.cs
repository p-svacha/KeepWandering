using System.Collections.Generic;
using System.Linq;

public static class ItemListExtensions
{
    /// <summary>
    /// Returns a natural language listing of the items, grouping identical items with their count.
    /// E.g. [wood, knife, knife, crowbar] -> "two knives, a piece of wood and a crowbar".
    /// Groups are ordered by count (descending), ties keep the order of first appearance.
    /// </summary>
    public static string ToNaturalLanguage(this List<ItemDef> itemDefs)
    {
        return itemDefs
            .GroupBy(def => def)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key.GetCountedLabel(group.Count()))
            .ToList()
            .ToNaturalLanguage();
    }

    public static string ToNaturalLanguage(this List<Item> items) => items.Select(item => item.Def).ToList().ToNaturalLanguage();
}