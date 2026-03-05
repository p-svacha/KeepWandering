using System.Collections.Generic;

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
    /// The definition of all item slots that are part of this encounter step option. The player can drag items from their inventory into these slots to meet the requirements of the option and/or reduce the option difficulty.
    /// </summary>
    public List<ItemSlot> ItemSlots { get; init; } = new List<ItemSlot>();

    /// <summary>
    /// Executes the logic of the encounter step option and returns the next encounter step to transition to.
    /// </summary>
    public abstract EncounterStep Execute(out OptionOutcomeDef outcome);

    /// <summary>
    /// Checks if the option has been set up correctly. Throws an exception if not.
    /// Initializes the option.
    /// </summary>
    public virtual void Init()
    {
        // Validate
        if (string.IsNullOrEmpty(Text)) throw new System.Exception("Encounter option text cannot be null or empty.");
        

        // Initialize
        foreach (ItemSlot slot in ItemSlots) slot.SetOption(this);
    }

    public bool CanSelect()
    {
        foreach (ItemSlot itemSlot in ItemSlots)
        {
            if (itemSlot.IsRequired && !itemSlot.IsFilled) return false;
        }

        return true;
    }
}
