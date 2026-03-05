using System.Collections.Generic;

public abstract class EncounterStepOption
{
    public abstract EncounterStepOptionType Type { get; }

    /// <summary>
    /// Text that gets displayed on the button of the encounter step option.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// Description that gets displayer when the player hovers over the encounter step option in the UI. This should give some hints to the player about the consequences of selecting this option.
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// The definition of all item slots that are part of this encounter step option. The player can drag items from their inventory into these slots to meet the requirements of the option and/or reduce the option difficulty.
    /// </summary>
    public List<ItemSlot> ItemSlots { get; private set; }

    /// <summary>
    /// Executes the logic of the encounter step option and returns the next encounter step to transition to.
    /// </summary>
    public abstract EncounterStep Execute();


    public EncounterStepOption(string text, string description, List<ItemSlot> itemSlots = null)
    {
        Text = text;
        Description = description;
        ItemSlots = itemSlots ?? new List<ItemSlot>();
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
