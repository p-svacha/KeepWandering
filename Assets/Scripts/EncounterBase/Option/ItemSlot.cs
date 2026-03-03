using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Options in an encounter step can have slots for dragging items into. This class represents the properties of such a slot.
/// </summary>
public class ItemSlot
{
    /// <summary>
    /// List of all specific items that may be dragged into this slot.
    /// </summary>
    public List<ItemDef> SpecificItems { get; private set; }

    /// <summary>
    /// List of all item tags that an item dragged into this slot must have at least one of.
    /// </summary>
    public List<ItemTagDef> ItemTags { get; private set; }

    /// <summary>
    /// Chance that the item dragged into this slot will be destroyed when the option is selected. This is a value between 0 and 1, where 0 means the item will never be destroyed and 1 means it will always be destroyed.
    /// </summary>
    public float DestructionChance { get; private set; }

    /// <summary>
    /// How much the option difficulty will be reduced if the slot is filled and the option is selected.
    /// </summary>
    public int DifficultyReduction { get; private set; }

    /// <summary>
    /// Returns a list of all items in the players inventory that can be dragged into this slot.
    /// </summary>
    public List<Item> GetSlottableItems()
    {
        List<Item> items = new List<Item>();

        foreach (Item item in Game.Instance.Inventory)
        {
            if (SpecificItems.Contains(item.Def) || ItemTags.Exists(tag => item.Def.Tags.ToList().Contains(tag)))
            {
                items.Add(item);
            }
        }

        return items;
    }
}
