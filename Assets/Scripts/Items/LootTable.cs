using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A loot table contains chances for different items and can be resolved to get random ones based on those chances.
/// </summary>
public class LootTable
{
    private Dictionary<ItemType, float> Items;

    public LootTable(params KeyValuePair<ItemType, float>[] items)
    {
        Items = new Dictionary<ItemType, float>();
        foreach (var kvp in items) Items.Add(kvp.Key, kvp.Value);
    }

    public LootTable(Dictionary<ItemType, float> items)
    {
        Items = items;
    }

    /// <summary>
    /// Returns the union of two LootTables as a new LootTable that contains the added up chances of of all items.
    /// </summary>
    public LootTable Union(LootTable other)
    {
        Dictionary<ItemType, float> newChances = new Dictionary<ItemType, float>(Items);
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
        Dictionary<ItemType, float> newChances = new Dictionary<ItemType, float>(Items);

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

    public Item GetItem(bool hide = false)
    {
        ItemType type = HelperFunctions.GetWeightedRandomElement(Items);
        Item item = Game.Singleton.GetItemInstance(type);
        if (hide)
        {
            item.Hide();
            item.transform.position = new Vector3(-200, -200, 0f);
        }
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
        ItemType type = HelperFunctions.GetWeightedRandomElement(Items);
        Item item = Game.Singleton.GetItemInstance(type);
        Game.Singleton.AddItemToInventory(item);
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
