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

    public Item GetItem()
    {
        ItemType type = HelperFunctions.GetWeightedRandomElement(Items);
        return Game.Singleton.GetItemInstance(type);
    }
    public List<Item> GetItems(int amount)
    {
        List<Item> items = new List<Item>();
        for (int i = 0; i < amount; i++) items.Add(GetItem());
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
