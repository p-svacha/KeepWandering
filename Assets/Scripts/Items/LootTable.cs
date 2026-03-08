using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A loot table contains chances for different items and can be resolved to get random ones based on those chances.
/// </summary>
public class LootTable : IEnumerable<KeyValuePair<ItemDef, float>>
{
    private Dictionary<ItemDef, float> Items { get; init; }

    public LootTable()
    {
        Items = new Dictionary<ItemDef, float>();
    }

    public LootTable(Dictionary<ItemDef, float> items)
    {
        Items = items;
    }

    public void Add(ItemDef item, float weight)
    {
        Items.Add(item, weight);
    }

    public IEnumerator<KeyValuePair<ItemDef, float>> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns the union of two LootTables as a new LootTable that contains the added up chances of of all items.
    /// </summary>
    public LootTable Union(LootTable other)
    {
        Dictionary<ItemDef, float> newChances = new Dictionary<ItemDef, float>(Items);
        foreach(var kvp in other.Items)
        {
            if (newChances.ContainsKey(kvp.Key)) newChances[kvp.Key] += kvp.Value;
            else newChances.Add(kvp.Key, kvp.Value);
        }
        return new LootTable(newChances);
    }

    /// <summary>
    /// Returns the intersection of two LootTables as a new LootTable that contains the multiplied chances of all items.
    /// </summary>
    public LootTable Intersect(LootTable other)
    {
        Dictionary<ItemDef, float> newChances = new Dictionary<ItemDef, float>(Items);

        foreach(var kvp in newChances)
        {
            if (!other.Items.ContainsKey(kvp.Key)) newChances.Remove(kvp.Key);
        }

        foreach (var kvp in other.Items)
        {
            if (newChances.ContainsKey(kvp.Key)) newChances[kvp.Key] *= kvp.Value;
        }

        return new LootTable(newChances);
    }

    public ItemDef Resolve()
    {
        return HelperFunctions.GetWeightedRandomElement(Items);
    }
    public Item GetItem(bool hidden = false, bool frozen = true)
    {
        ItemDef type = HelperFunctions.GetWeightedRandomElement(Items);
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

    public Item AddItemToInventory()
    {
        ItemDef type = HelperFunctions.GetWeightedRandomElement(Items);
        Item item = Game.Instance.CreateItem(type);
        Game.Instance.AddExistingItemToInventory(item);
        return item;
    }
    public List<Item> AddItemsToInventory(int amount)
    {
        List<Item> items = new List<Item>();
        for (int i = 0; i < amount; i++) items.Add(AddItemToInventory());
        return items;
    }
    public List<Item> AddItemsToInventory(int minAmount, int maxAmount)
    {
        int amount = Random.Range(minAmount, maxAmount + 1);
        return AddItemsToInventory(amount);
    }
}
