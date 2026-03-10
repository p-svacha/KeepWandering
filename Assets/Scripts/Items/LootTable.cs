using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A loot table contains chances for different items and can be resolved to get random ones based on those chances.
/// Can also reference other LootTables as entries, so an entire category of items competes as a single weighted entry.
/// </summary>
public class LootTable : IEnumerable<KeyValuePair<ItemDef, float>>
{
    private const bool DEBUG = true;

    private Dictionary<ItemDef, float> Items { get; init; }
    private Dictionary<LootTable, float> SubTables { get; init; }

    public LootTable()
    {
        Items = new Dictionary<ItemDef, float>();
        SubTables = new Dictionary<LootTable, float>();
    }

    public LootTable(Dictionary<ItemDef, float> items)
    {
        Items = items;
        SubTables = new Dictionary<LootTable, float>();
    }

    public LootTable(Dictionary<ItemDef, float> items, Dictionary<LootTable, float> subTables)
    {
        Items = items;
        SubTables = subTables;
    }

    public void Add(ItemDef item, float weight)
    {
        Items.Add(item, weight);
    }

    public void Add(LootTable table, float weight)
    {
        SubTables.Add(table, weight);
    }

    public IEnumerator<KeyValuePair<ItemDef, float>> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns the union of two LootTables as a new LootTable that contains the added up chances of all items and sub-tables.
    /// </summary>
    public LootTable Union(LootTable other)
    {
        Dictionary<ItemDef, float> newItems = new Dictionary<ItemDef, float>(Items);
        foreach(var kvp in other.Items)
        {
            if (newItems.ContainsKey(kvp.Key)) newItems[kvp.Key] += kvp.Value;
            else newItems.Add(kvp.Key, kvp.Value);
        }

        Dictionary<LootTable, float> newSubTables = new Dictionary<LootTable, float>(SubTables);
        foreach(var kvp in other.SubTables)
        {
            if (newSubTables.ContainsKey(kvp.Key)) newSubTables[kvp.Key] += kvp.Value;
            else newSubTables.Add(kvp.Key, kvp.Value);
        }

        return new LootTable(newItems, newSubTables);
    }

    /// <summary>
    /// Returns the intersection of two LootTables as a new LootTable that contains the multiplied chances of all items and sub-tables.
    /// </summary>
    public LootTable Intersect(LootTable other)
    {
        Dictionary<ItemDef, float> newItems = new Dictionary<ItemDef, float>(Items);

        foreach(var kvp in newItems)
        {
            if (!other.Items.ContainsKey(kvp.Key)) newItems.Remove(kvp.Key);
        }

        foreach (var kvp in other.Items)
        {
            if (newItems.ContainsKey(kvp.Key)) newItems[kvp.Key] *= kvp.Value;
        }

        Dictionary<LootTable, float> newSubTables = new Dictionary<LootTable, float>(SubTables);

        foreach(var kvp in newSubTables)
        {
            if (!other.SubTables.ContainsKey(kvp.Key)) newSubTables.Remove(kvp.Key);
        }

        foreach (var kvp in other.SubTables)
        {
            if (newSubTables.ContainsKey(kvp.Key)) newSubTables[kvp.Key] *= kvp.Value;
        }

        return new LootTable(newItems, newSubTables);
    }

    public ItemDef Resolve(bool debug = true)
    {
        if (SubTables.Count == 0)
        {
            ItemDef resolvedItem = Items.GetWeightedRandomElement();
            if (DEBUG && debug) DebugChances(resolvedItem);
            return resolvedItem;
        }

        float totalWeight = 0f;
        foreach (var kvp in Items) totalWeight += kvp.Value;
        foreach (var kvp in SubTables) totalWeight += kvp.Value;

        if (totalWeight == 0f)
            throw new System.Exception("Can't resolve LootTable because all weights are 0.");

        float rng = Random.Range(0f, totalWeight);
        float tmpSum = 0f;

        foreach (var kvp in Items)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum)
            {
                ItemDef resolvedItem = kvp.Key;
                if (DEBUG && debug) DebugChances(resolvedItem);
                return resolvedItem;
            }
        }

        foreach (var kvp in SubTables)
        {
            tmpSum += kvp.Value;
            if (rng < tmpSum)
            {
                ItemDef resolvedItem = kvp.Key.Resolve(debug: false);
                if (DEBUG && debug) DebugChances(resolvedItem);
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

        Dictionary<ItemDef, float> normalizedProbabilities = allProbabilities.ToDictionary(
            x => x.Key, 
            x => x.Value / totalWeight * 100f
        );

        string output = "LootTable Probabilities";
        output += "\n------------------------------";

        foreach (var kvp in normalizedProbabilities.OrderByDescending(x => x.Value))
        {
            bool isPicked = kvp.Key == pickedItem;
            output += "\n" + (isPicked ? "* " : "  ") + kvp.Key.DefName + ": " + kvp.Value.ToString("0.0") + "%";
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
        foreach (var kvp in Items) totalWeight += kvp.Value;
        foreach (var kvp in SubTables) totalWeight += kvp.Value;

        if (totalWeight == 0f) return result;

        // Add direct items
        foreach (var kvp in Items)
        {
            if (result.ContainsKey(kvp.Key))
                result[kvp.Key] += kvp.Value;
            else
                result.Add(kvp.Key, kvp.Value);
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
                float adjustedWeight = subTableKvp.Value * (itemKvp.Value / subTableTotalWeight);

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
    public void AddItemsToInventory(int amount)
    {
        for (int i = 0; i < amount; i++) AddItemToInventory();
    }
}
