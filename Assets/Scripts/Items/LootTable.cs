using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A loot table contains chances for different items and can be resolved to get random ones based on those chances.
/// Can also reference other LootTables as entries, so an entire category of items competes as a single weighted entry.
/// </summary>
public class LootTable : IEnumerable<KeyValuePair<ItemDef, Rarity>>
{
    private Dictionary<ItemDef, Rarity> Items { get; init; }
    private Dictionary<LootTable, Rarity> SubTables { get; init; }

    public LootTable()
    {
        Items = new Dictionary<ItemDef, Rarity>();
        SubTables = new Dictionary<LootTable, Rarity>();
    }

    public LootTable(Dictionary<ItemDef, Rarity> items)
    {
        Items = items;
        SubTables = new Dictionary<LootTable, Rarity>();
    }

    public LootTable(Dictionary<ItemDef, Rarity> items, Dictionary<LootTable, Rarity> subTables)
    {
        Items = items;
        SubTables = subTables;
    }

    public void Add(ItemDef item, Rarity rarity)
    {
        Items.Add(item, rarity);
    }

    public void Add(LootTable table, Rarity rarity)
    {
        SubTables.Add(table, rarity);
    }

    public IEnumerator<KeyValuePair<ItemDef, Rarity>> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns the union of two LootTables as a new LootTable that contains the averaged out rarities of the two tables. Rarity is always rounded to the nearest rarity value, and rounded up in case of a tie.
    /// </summary>
    public LootTable Union(LootTable other)
    {
        Dictionary<ItemDef, Rarity> newItems = new Dictionary<ItemDef, Rarity>(Items);
        foreach (var kvp in other.Items)
        {
            if (newItems.TryGetValue(kvp.Key, out Rarity existing))
                newItems[kvp.Key] = AverageRarity(existing, kvp.Value);
            else
                newItems.Add(kvp.Key, kvp.Value);
        }

        Dictionary<LootTable, Rarity> newSubTables = new Dictionary<LootTable, Rarity>(SubTables);
        foreach (var kvp in other.SubTables)
        {
            if (newSubTables.TryGetValue(kvp.Key, out Rarity existing))
                newSubTables[kvp.Key] = AverageRarity(existing, kvp.Value);
            else
                newSubTables.Add(kvp.Key, kvp.Value);
        }

        return new LootTable(newItems, newSubTables);
    }

    /// <summary>
    /// Averages two Rarity values and snaps the result to the nearest defined Rarity, rounding up on a tie.
    /// </summary>
    private static Rarity AverageRarity(Rarity a, Rarity b)
    {
        float avg = ((int)a + (int)b) / 2f;

        Rarity closest = Rarity.ExtremelyRare;
        float smallestDiff = float.MaxValue;

        foreach (Rarity candidate in System.Enum.GetValues(typeof(Rarity)).Cast<Rarity>().OrderBy(r => (int)r))
        {
            float diff = Mathf.Abs((int)candidate - avg);
            if (diff < smallestDiff || (diff == smallestDiff && (int)candidate > (int)closest))
            {
                smallestDiff = diff;
                closest = candidate;
            }
        }

        return closest;
    }

    public List<ItemDef> ResolveMultiple(int amount, bool debug = true)
    {
        List<ItemDef> resolved = new List<ItemDef>();
        for (int i = 0; i < amount; i++) resolved.Add(Resolve(debug));
        return resolved;
    }

    public ItemDef Resolve(bool debug = true)
    {
        if (SubTables.Count == 0)
        {
            ItemDef resolvedItem = Items.GetWeightedRandomElement(debug: false);
            if (Game.DEBUG_RANDOM_CHOICES && debug) DebugChances(resolvedItem);
            return resolvedItem;
        }

        float totalWeight = 0f;
        foreach (var kvp in Items) totalWeight += (int)kvp.Value;
        foreach (var kvp in SubTables) totalWeight += (int)kvp.Value;

        if (totalWeight == 0f)
            throw new System.Exception("Can't resolve LootTable because all weights are 0.");

        float rng = Random.Range(0f, totalWeight);
        float tmpSum = 0f;

        foreach (var kvp in Items)
        {
            tmpSum += (int)kvp.Value;
            if (rng < tmpSum)
            {
                ItemDef resolvedItem = kvp.Key;
                if (Game.DEBUG_RANDOM_CHOICES && debug) DebugChances(resolvedItem);
                return resolvedItem;
            }
        }

        foreach (var kvp in SubTables)
        {
            tmpSum += (int)kvp.Value;
            if (rng < tmpSum)
            {
                ItemDef resolvedItem = kvp.Key.Resolve(debug: false);
                if (Game.DEBUG_RANDOM_CHOICES && debug) DebugChances(resolvedItem);
                return resolvedItem;
            }
        }

        throw new System.Exception("Failed to resolve LootTable.");
    }

    /// <summary>
    /// Shows the chances of each item in the loot table in the console, with one line per item, ordered by probability. The picked item is marked with an asterisk.
    /// </summary>
    private void DebugChances(ItemDef pickedItem)
    {
        Dictionary<ItemDef, float> allProbabilities = GetAllItemProbabilities();

        float totalWeight = allProbabilities.Sum(x => x.Value);
        if (totalWeight == 0f) return;

        // Aggregate by DefName to merge entries that reference the same logical item from different sources (e.g. direct items and subtables)
        Dictionary<string, float> aggregated = new Dictionary<string, float>();
        foreach (var kvp in allProbabilities)
        {
            string name = kvp.Key.DefName;
            if (aggregated.ContainsKey(name))
                aggregated[name] += kvp.Value;
            else
                aggregated[name] = kvp.Value;
        }

        string pickedName = pickedItem.DefName;

        string output = "LootTable Probabilities";
        output += "\n------------------------------";

        foreach (var kvp in aggregated.OrderByDescending(x => x.Value))
        {
            float pct = kvp.Value / totalWeight * 100f;
            bool isPicked = kvp.Key == pickedName;
            output += "\n" + (isPicked ? "* " : "  ") + kvp.Key + ": " + pct.ToString("0.0") + "%";
        }

        output += "\n------------------------------";
        Debug.Log(output);
    }

    /// <summary>
    /// Recursively collects all items from this table and its sub-tables with their weighted probabilities.
    /// </summary>
    private Dictionary<ItemDef, float> GetAllItemProbabilities()
    {
        Dictionary<ItemDef, float> result = new Dictionary<ItemDef, float>();

        float totalWeight = 0f;
        foreach (var kvp in Items) totalWeight += (int)kvp.Value;
        foreach (var kvp in SubTables) totalWeight += (int)kvp.Value;

        if (totalWeight == 0f) return result;

        // Add direct items
        foreach (var kvp in Items)
        {
            if (result.ContainsKey(kvp.Key))
                result[kvp.Key] += (int)kvp.Value;
            else
                result.Add(kvp.Key, (int)kvp.Value);
        }

        // Add items from sub-tables with adjusted weights
        foreach (var subTableKvp in SubTables)
        {
            Dictionary<ItemDef, float> subTableProbabilities = subTableKvp.Key.GetAllItemProbabilities();
            float subTableTotalWeight = subTableProbabilities.Sum(x => x.Value);

            if (subTableTotalWeight == 0f) continue;

            foreach (var itemKvp in subTableProbabilities)
            {
                // Weight in this table = (sub-table weight) * (item's proportion in sub-table)
                float adjustedWeight = (int)subTableKvp.Value * (itemKvp.Value / subTableTotalWeight);

                if (result.ContainsKey(itemKvp.Key))
                    result[itemKvp.Key] += adjustedWeight;
                else
                    result.Add(itemKvp.Key, adjustedWeight);
            }
        }

        return result;
    }

    public Item GetItem(bool hidden = false, bool frozen = true)
    {
        ItemDef type = Resolve();
        Item item = Game.Instance.CreateItem(type, hidden);
        if (!frozen) item.Renderer.Unfreeze();
        return item;
    }
    public List<Item> GetItems(int amount, bool hide = false)
    {
        List<Item> items = new List<Item>();
        for (int i = 0; i < amount; i++) items.Add(GetItem(hide));
        return items;
    }
    public List<Item> GetItems(int minAmount, int maxAmount)
    {
        int amount = Random.Range(minAmount, maxAmount + 1);
        return GetItems(amount);
    }

    public void AddItemToInventory()
    {
        ItemDef def = Resolve();
        Game.Instance.AddNewItemToInventory(def);
    }

    public void AddItemsToInventory(int min, int max)
    {
        int amount = Random.Range(min, max + 1);
        AddItemsToInventory(amount);
    }
    public void AddItemsToInventory(int amount)
    {
        for (int i = 0; i < amount; i++) AddItemToInventory();
    }
}
