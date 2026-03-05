using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

/// <summary>
/// Options in an encounter step can have slots for dragging items into. This class represents a single active instance of such a slot of an option that is currently being displayed in the UI.
/// It contains all properties of the slot.
/// </summary>
public class ItemSlot
{
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
    public bool IsRequired { get; private set; }

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
    public int DefaultDifficultyReduction { get; private set; }

    /// <summary>
    /// Specific items that reduce the option difficulty by an amount that overrides the default difficulty reduction if dragged into this slot.
    /// </summary>
    public Dictionary<ItemDef, int> DifficultyReductionOverrides { get; private set; }
    public bool HasCustomDifficultyReductions => DifficultyReductionOverrides.Count > 0;

    public ItemSlot(bool isRequired = false, List<ItemDef> specificItems = null, List<ItemTagDef> itemTags = null, float destructionChance = 0f, int defaultDifficultyReduction = 0, Dictionary<ItemDef, int> difficultyReductionOverrides = null)
    {
        IsRequired = isRequired;
        SpecificItems = specificItems ?? new List<ItemDef>();
        ItemTags = itemTags ?? new List<ItemTagDef>();
        DestructionChance = destructionChance;
        DefaultDifficultyReduction = defaultDifficultyReduction;
        DifficultyReductionOverrides = difficultyReductionOverrides ?? new Dictionary<ItemDef, int>();

        // Validate
        if (DestructionChance < 0f || DestructionChance > 1f)
        {
            Debug.LogError("DestructionChance must be between 0 and 1.");
            DestructionChance = Mathf.Clamp01(DestructionChance);
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
        if (!SpecificItems.Contains(item.Def) && !ItemTags.Exists(tag => item.Def.Tags.ToList().Contains(tag)))
            throw new System.Exception("Item does not match slot requirements.");

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
        if (DifficultyReductionOverrides.ContainsKey(itemDef))
        {
            return DifficultyReductionOverrides[itemDef];
        }
        else
        {
            return DefaultDifficultyReduction;
        }
    }

    public bool CanAcceptItem(Item item)
    {
        return GetSlottableItemDefs().Contains(item.Def);
    }

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

    public List<ItemDef> GetSlottableItemDefs()
    {
        List<ItemDef> itemDefs = new List<ItemDef>();
        foreach (ItemDef itemDef in DefDatabase<ItemDef>.AllDefs)
        {
            if (SpecificItems.Contains(itemDef) || ItemTags.Exists(tag => itemDef.Tags.ToList().Contains(tag)))
            {
                itemDefs.Add(itemDef);
            }
        }
        return itemDefs;
    }
}
