using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EncounterOption
{
    /// <summary>
    /// Type of the option. Either FixedOutcome or SkillCheck.
    /// </summary>
    public abstract EncounterOptionType Type { get; }

    /// <summary>
    /// Text that gets displayed on the button of the encounter step option.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Description that gets displayer when the player hovers over the encounter step option in the UI. This should give some hints to the player about the consequences of selecting this option.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// The world space sprite in the encounter scene this option is associated with. The option will be visually bound to this sprite, and the player can click on the sprite to select the option. If null, the option will only be selectable through the UI.
    /// </summary>
    public GameObject Sprite { get; init; } = null;

    /// <summary>
    /// When true, the option can only be selected once per encounter visit (day). It will automatically be hidden after being selected once.
    /// </summary>
    public bool OncePerDay { get; init; } = false;

    /// <summary>
    /// When true, the option can only be selected once ever. It will automatically be hidden after being selected once, with no reset when revisiting the encounter.
    /// </summary>
    public bool OnceEver { get; init; } = false;

    /// <summary>
    /// The definition of all item slots that are part of this encounter step option. The player can drag items from their inventory into these slots to meet the requirements of the option and/or reduce the option difficulty.
    /// </summary>
    public List<ItemSlot> ItemSlots { get; init; } = new List<ItemSlot>();

    /// <summary>
    /// The stat requirements that must be met in order to select this encounter step option. Each stat has a minimum value that must be met. If the player does not meet the requirements, the option will still be displayed, but it will be disabled and the player will not be able to select it.
    /// </summary>
    public Dictionary<StatDef, int> SkillRequirements { get; init; } = new Dictionary<StatDef, int>();

    /// <summary>
    /// Executes the logic of the encounter step option and returns the text to be displayed on the next step.
    /// </summary>
    public abstract string Execute(out OptionOutcomeDef outcome);

    /// <summary>
    /// Checks if the option has been set up correctly. Throws an exception if not.
    /// Initializes the option.
    /// </summary>
    public virtual void Init()
    {
        // Initialize
        foreach (ItemSlot slot in ItemSlots) slot.SetOption(this);

        // Validate
        if (string.IsNullOrEmpty(Text)) throw new System.Exception($"Encounter option '{Text}' text cannot be null or empty in encounter '{Game.Instance.CurrentEncounter.Def.DefName}'.");
        if (OncePerDay && OnceEver) throw new System.Exception($"Encounter option '{Text}' cannot be both once per day and once ever in encounter '{Game.Instance.CurrentEncounter.Def.DefName}'.");
        foreach (ItemSlot slot in ItemSlots)
        {
            try
            {
                slot.Validate();
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"Encounter option '{Text}' in encounter '{Game.Instance.CurrentEncounter.Def.DefName}' has an invalid item slot: {e.Message}");
            }
        }
        foreach(var statRequirement in SkillRequirements)
        {
            if (statRequirement.Value < 0) throw new System.Exception($"Stat requirement for {statRequirement.Key.DefName} in encounter option '{Text}' in encounter '{Game.Instance.CurrentEncounter.Def.DefName}' cannot be negative.");
        }
    }

    public bool HasRequirements()
    {
        return ItemSlots.Any(slot => slot.IsRequired) || SkillRequirements.Count > 0;
    }

    /// <summary>
    /// Checks and returns if this option is currently selectable.
    /// <br/>If countInventoryItems is true, the method will also check if the player has enough items in their inventory to fill the required item slots. If false, it will only check if the required item slots are filled.
    /// </summary>
    public bool CanSelect(bool countInventoryItems = false)
    {
        // Item slot requirements
        foreach (ItemSlot itemSlot in ItemSlots)
        {
            if (itemSlot.IsRequired)
            {
                if (!countInventoryItems && !itemSlot.IsFilled) return false;
                if (countInventoryItems && !itemSlot.GetSlottableItemDefs().Any(itemDef => Game.Instance.PlayerHasItem(itemDef)) && !itemSlot.IsFilled) return false;
            }
        }

        // Stat requirements
        foreach (var statRequirement in SkillRequirements)
        {
            StatDef statDef = statRequirement.Key;
            int requiredValue = statRequirement.Value;
            int playerValue = Game.Instance.Player.GetStatValue(statDef);
            if (playerValue < requiredValue) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a string explaining why the option cannot be selected. If the option can be selected, returns an empty string.
    /// </summary>
    public string GetNonSelectableReason()
    {
        // Item slot requirements
        foreach (ItemSlot itemSlot in ItemSlots)
        {
            if (itemSlot.IsRequired && !itemSlot.IsFilled)
            {
                if (itemSlot.Item != null)
                {
                    return $"Requires {itemSlot.Item.Label}.";
                }
                if (itemSlot.CustomItemSet != null)
                {
                    string allowedItems = string.Join(", ", itemSlot.CustomItemSet.Select(itemDef => itemDef.Label));
                    return $"Requires one of the following items: {allowedItems}.";
                }
                if (itemSlot.Tag != null)
                {
                    if(itemSlot.HasrequiredTagLevel)
                    {
                        return $"Requires an item with tag {itemSlot.Tag.Label} of at least level {itemSlot.RequiredTagLevel}.";
                    }
                    else
                    {
                        return $"Requires an item with tag {itemSlot.Tag.Label}.";
                    }
                }
            }
        }
        // Stat requirements
        foreach (var statRequirement in SkillRequirements)
        {
            StatDef statDef = statRequirement.Key;
            int requiredValue = statRequirement.Value;
            int playerValue = Game.Instance.Player.GetStatValue(statDef);
            if (playerValue < requiredValue) return $"Requires {statDef.DefName} of at least {requiredValue}.";
        }
        return "Unknown reason.";
    }
}
