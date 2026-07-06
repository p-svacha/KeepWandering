using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Options in an encounter step can have slots for dragging items into. This class represents a single active instance of such a slot of an option that is currently being displayed in the UI.
/// It contains all properties of the slot.
/// </summary>
public class ItemSlot
{
    public static Dictionary<int, int> ITEM_LEVEL_DIFFICULTY_REDUCTIONS = new Dictionary<int, int>()
    {
        { 1, 20 },
        { 2, 40 },
        { 3, 60 },
        { 4, 80 },
        { 5, 100 },
    };

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
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// If true, the item dragged into this slot will be destroyed when the option is selected. If false, the item will lose 1 durability.
    /// </summary>
    public bool IsDestroyingItem { get; init; } = false;

    /// <summary>
    /// The specific item that can be dragged into this slot.
    /// </summary>
    public ItemDef Item { get; init; } = null;

    /// <summary>
    /// The custom set of allowed items that can be dragged into this slot.
    /// </summary>
    public ItemSet CustomItemSet { get; init; } = null;

    /// <summary>
    /// The item tag that an item dragged into this slot must have.
    /// </summary>
    public ItemTagDef Tag { get; init; } = null;

    /// <summary>
    /// If > 0, this defines that the item with the specified tag must have at least this level in order to be accepted into this slot. If the item has a lower level, it will not be accepted.
    /// </summary>
    public int RequiredTagLevel { get; init; } = -1;
    public bool HasrequiredTagLevel => RequiredTagLevel != -1;

    public void Validate()
    {
        // Make sure slot has exactly 1 option configured
        if (Item == null && Tag == null && (CustomItemSet == null || CustomItemSet.Items == null)) throw new System.Exception("ItemSlot must have either a specific item, a specific tag, or a list of allowed items.");

        if (Item != null && Tag != null) throw new System.Exception("ItemSlot cannot have both a specific item and a specific tag.");
        if (CustomItemSet != null && CustomItemSet.Items != null && Tag != null) throw new System.Exception("ItemSlot cannot have both a list of allowed items and a specific tag.");

        // Validation for specific item(s)
        if (Item != null || (CustomItemSet != null && CustomItemSet.Items != null))
        {
            if (Item != null && CustomItemSet != null && CustomItemSet.Items != null) throw new System.Exception("ItemSlot cannot have both a specific item and a list of allowed items.");
            if (!IsRequired) throw new System.Exception("ItemSlot with a specific item or list of allowed items must be required, as only tag slots can have difficulty reductions for optional slots.");
            if (HasrequiredTagLevel) throw new System.Exception("ItemSlot with a specific item or list of allowed items cannot have a required tag level, as only tag slots can have difficulty reductions.");
            if (!IsDestroyingItem) throw new System.Exception("ItemSlot with a specific item or list of allowed items must destroy the item, as the durability system is intended to work with tags.");

            // Custom item set
            if (CustomItemSet != null && CustomItemSet.Items != null && string.IsNullOrEmpty(CustomItemSet.Name)) throw new System.Exception("ItemSlot with a list of allowed items must have a display label set.");
            if (CustomItemSet != null && CustomItemSet.Items != null && CustomItemSet.Items.Count == 0) throw new System.Exception("ItemSlot with a list of allowed items must have at least one item in the list.");
        }

        // Validation for tag
        if (Tag != null)
        {
            if (HasrequiredTagLevel && RequiredTagLevel < ItemDef.MIN_TAG_LEVEL) throw new System.Exception($"ItemSlot has a required tag level of {RequiredTagLevel}, which is below the minimum allowed level of {ItemDef.MIN_TAG_LEVEL}.");
            if (HasrequiredTagLevel && RequiredTagLevel > ItemDef.MAX_TAG_LEVEL) throw new System.Exception($"ItemSlot has a required tag level of {RequiredTagLevel}, which is above the maximum allowed level of {ItemDef.MAX_TAG_LEVEL}.");
        }

        // Must have set a tag if not required (because then the slot is to reduce difficulty, which is only possible with a tag)
        if (!IsRequired && Tag == null) throw new System.Exception("ItemSlot that is not required must have a tag set, as only tag slots can have difficulty reductions for optional slots.");
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

        // If this option is sprite-bound, ensure the sprite indicator is locked
        if (Option.Sprite != null)
        {
            // Find the indicator for this sprite
            var indicators = SpriteOptionInteractionManager.GetActiveIndicators();
            if (indicators.TryGetValue(Option.Sprite, out SpriteOptionIndicator indicator))
            {
                SpriteOptionInteractionManager.EnsureLocked(indicator);
            }
        }

        UI_EncounterDisplay.Instance.RefreshOption(Option);
    }

    public void Empty()
    {
        FilledItem.Show();
        Game.Instance.DropItemIntoCart(FilledItem);
        FilledItem = null;

        UI_EncounterDisplay.Instance.RefreshOption(Option);
    }

    public int GetDifficultyReduction(ItemDef itemDef)
    {
        return ITEM_LEVEL_DIFFICULTY_REDUCTIONS[itemDef.Tags[Tag]];
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
            if (CustomItemSet != null && !CustomItemSet.Items.Contains(itemDef)) continue;
            if (HasrequiredTagLevel && (!itemDef.HasTag(Tag) || itemDef.Tags[Tag] < RequiredTagLevel)) continue;
            itemDefs.Add(itemDef);
        }

        // Sort by difficulty reduction (tag only)
        if (Tag != null)
        {
            itemDefs.Sort((a, b) => GetDifficultyReduction(b).CompareTo(GetDifficultyReduction(a)));
        }

        // Debug.Log($"Found {itemDefs.Count} slottable item defs for slot {Label()}: {string.Join(", ", itemDefs.Select(x => x.DefName))}");

        return itemDefs;
    }

    public override string ToString()
    {
        if (Item != null) return $"Slot for {Item.DefName}";
        if (Tag != null) return $"Slot for items with {Tag.DefName} tag";
        if (CustomItemSet != null) return $"Slot for specific items: {string.Join(", ", CustomItemSet.Items.Select(x => x.DefName))}";
        return "Invalid slot";
    }

    public string Label()
    {
        if (Item != null) return Item.LabelCap;
        if (Tag != null)
        {
            if (HasrequiredTagLevel) return $"Tier {RequiredTagLevel}+ {Tag.LabelCap}";
            else return $"{Tag.LabelCap}";
        }
        if (CustomItemSet != null) return CustomItemSet.Name;

        throw new System.Exception("Invalid slot: cannot generate label for slot with no item, tag, or custom item set.");
    }
}
