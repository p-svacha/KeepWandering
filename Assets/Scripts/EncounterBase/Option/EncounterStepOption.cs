using System.Collections.Generic;
using System.Linq;

public abstract class EncounterOption
{
    public abstract EncounterStepOptionType Type { get; }

    /// <summary>
    /// Text that gets displayed on the button of the encounter step option.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Description that gets displayer when the player hovers over the encounter step option in the UI. This should give some hints to the player about the consequences of selecting this option.
    /// </summary>
    public string Description { get; init; } = "";

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
        if (string.IsNullOrEmpty(Text)) throw new System.Exception("Encounter option text cannot be null or empty.");
        if (OncePerDay && OnceEver) throw new System.Exception("Encounter option cannot be both once per day and once ever.");
        foreach (ItemSlot slot in ItemSlots) slot.Validate();
    }

    public bool HasRequirements()
    {
        return ItemSlots.Any(slot => slot.IsRequired) || SkillRequirements.Count > 0;
    }

    public bool CanSelect()
    {
        // Item slot requirements
        foreach (ItemSlot itemSlot in ItemSlots)
        {
            if (itemSlot.IsRequired && !itemSlot.IsFilled) return false;
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
