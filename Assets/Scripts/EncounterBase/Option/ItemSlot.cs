using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Options in an encounter step can have slots for dragging items into. This class represents a single active instance of such a slot of an option that is currently being displayed in the UI.
/// It contains all properties of the slot.
/// </summary>
public class ItemSlot
{
    public const int MIN_DIFFICULTY_REDUCTION = 5;

    /// <summary>
    /// The option this slot belongs to.
    /// </summary>
    public EncounterOption Option { get; private set; }

    /// <summary>
    /// The item that is currently dragged into this slot. This is null if the slot is currently empty.
    /// </summary>
    public Item FilledItem { get; private set; }
    public bool IsFilled => FilledItem != null;

    /// <summary>
    /// If true, the option this slot belongs to cannot be selected unless an item is dragged into this slot.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// The specific item that can be dragged into this slot.
    /// </summary>
    public ItemDef Item { get; init; } = null;

    /// <summary>
    /// The custom set of allowed items that can be dragged into this slot.
    /// </summary>
    public List<ItemDef> AllowedItems { get; init; } = null;

    /// <summary>
    /// The item tag that an item dragged into this slot must have.
    /// </summary>
    public ItemTagDef Tag { get; init; } = null;

    /// <summary>
    /// Chance that the item dragged into this slot will be destroyed when the option is selected. This is a value between 0 and 1, where 0 means the item will never be destroyed and 1 means it will always be destroyed.
    /// </summary>
    public float DestructionChance { get; init; }

    /// <summary>
    /// How much the option difficulty will be reduced if the slot is filled and the option is selected.
    /// </summary>
    public int DifficultyReduction { get; init; }

    /// <summary>
    /// Specific items that reduce the option difficulty by an amount that overrides the default difficulty reduction if dragged into this slot.
    /// </summary>
    public Dictionary<ItemDef, int> DifficultyReductionOverrides { get; init; } = new Dictionary<ItemDef, int>();

    /// <summary>
    /// Returns false if all accepted items reduce the option difficulty by the same amount, true if there are specific items that reduce the option difficulty by different amounts (either by slot override, or item tag override).
    /// </summary>
    public bool HasMultipleDifficultyReductions()
    {
        foreach (ItemDef itemDef in GetSlottableItemDefs())
        {
            if (GetDifficultyReduction(itemDef) != DifficultyReduction) return true;
        }
        return false;
    }

    public void Validate()
    {
        if (Item != null && Tag != null) throw new System.Exception("ItemSlot cannot have both a specific item and a specific tag.");
        if (Item != null && AllowedItems != null) throw new System.Exception("ItemSlot cannot have both a specific item and a list of allowed items.");
        if (AllowedItems != null && Tag != null) throw new System.Exception("ItemSlot cannot have both a list of allowed items and a specific tag.");
        if (Item == null && Tag == null && AllowedItems == null) throw new System.Exception("ItemSlot must have either a specific item, a specific tag, or a list of allowed items.");


        if (DestructionChance < 0f || DestructionChance > 1f) throw new System.Exception("DestructionChance must be between 0 and 1.");

        foreach (var customValue in DifficultyReductionOverrides)
        {
            if (!CanAcceptItemDef(customValue.Key))
            {
                throw new System.Exception($"Difficulty reduction override for item {customValue.Key.Label} is invalid because the item does not match the slot requirements.");
            }
            if (customValue.Value < MIN_DIFFICULTY_REDUCTION) throw new System.Exception($"Difficulty reduction override for item {customValue.Key.Label} must be a positive integer.");
        }
    }

    public void SetOption(EncounterOption option)
    {
        Option = option;
    }

    public void Fill(Item item)
    {
        // Validate
        if (item == null) throw new System.Exception("Cannot fill item slot with null item.");
        if (!CanAcceptItem(item)) throw new System.Exception("Item does not match slot requirements.");

        // If already filled, empty the old item first
        if (IsFilled) Empty();

        FilledItem = item;
        item.Hide();
        item.Freeze();

        UI_EncounterDisplay.Instance.RefreshOption(Option);
    }

    public void Empty()
    {
        FilledItem.Show();
        Game.Instance.DropItemIntoCart(FilledItem);
        FilledItem = null;

        UI_EncounterDisplay.Instance.RefreshOption(Option);
    }

    /// <summary>
    /// Detaches the filled item from this slot without returning it to the cart.
    /// The caller is responsible for handling the item (destroying or returning it).
    /// </summary>
    public Item TakeItem()
    {
        Item item = FilledItem;
        FilledItem = null;
        return item;
    }

    public int GetDifficultyReduction(ItemDef itemDef)
    {
        // Priority 1: Item-specific override
        if (DifficultyReductionOverrides.ContainsKey(itemDef))
        {
            return DifficultyReductionOverrides[itemDef];
        }

        // Priority 2: Default reduction adjusted by the item's tag value modifier
        if (Tag != null)
        {
            if (itemDef.HasTag(Tag) && itemDef.Tags.HasModifier(Tag))
            {
                int newReduction = DifficultyReduction + itemDef.Tags.GetModifier(Tag);
                if (newReduction < MIN_DIFFICULTY_REDUCTION) return MIN_DIFFICULTY_REDUCTION;
                return newReduction;
            }
        }

        // Priority 3: Default reduction
        return DifficultyReduction;
    }

    public bool CanAcceptItem(Item item) => CanAcceptItemDef(item.Def);
    public bool CanAcceptItemDef(ItemDef itemDef)
    {
        return GetSlottableItemDefs().Contains(itemDef);
    }

    /// <summary>
    /// Returns a list of all items in the players inventory that can be dragged into this slot.
    /// </summary>
    public List<Item> GetSlottableItems()
    {
        List<Item> items = new List<Item>();

        foreach (Item item in Game.Instance.Inventory)
        {
            if (CanAcceptItem(item))
            {
                items.Add(item);
            }
        }

        return items;
    }

    public List<ItemDef> GetSlottableItemDefs()
    {
        List<ItemDef> itemDefs = new List<ItemDef>();
        foreach (ItemDef itemDef in DefDatabase<ItemDef>.AllDefs)
        {
            if (Item != null && itemDef != Item) continue;
            if (Tag != null && !itemDef.HasTag(Tag)) continue;
            if (AllowedItems != null && !AllowedItems.Contains(itemDef)) continue;
            itemDefs.Add(itemDef);
        }

        // Sort by difficulty reduction (lowest first)
        itemDefs.Sort((a, b) => GetDifficultyReduction(a).CompareTo(GetDifficultyReduction(b)));

        Debug.Log($"Found {itemDefs.Count} slottable item defs for slot {this}: {string.Join(", ", itemDefs.Select(x => x.DefName))}");

        return itemDefs;
    }

    public override string ToString()
    {
        if (Item != null) return $"Slot for {Item.DefName}";
        if (Tag != null) return $"Slot for items with {Tag.DefName} tag";
        if (AllowedItems != null) return $"Slot for specific items: {string.Join(", ", AllowedItems.Select(x => x.DefName))}";
        return "Invalid slot";
    }
}
