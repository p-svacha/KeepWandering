using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EncounterStepOption
{
    public abstract EncounterStepOptionType Type { get; }

    /// <summary>
    /// Text that gets displayed on the button of the encounter step option.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// The list of items need to be dragged to this option in order to be able to select it.
    /// </summary>
    public List<ItemSlot> RequirementSlots { get; private set; }

    /// <summary>
    /// Executes the logic of the encounter step option and returns the next encounter step to transition to.
    /// </summary>
    public abstract EncounterStep Execute();

    
    public EncounterStepOption(string text, List<ItemSlot> requirementSlots = null)
    {
        Text = text;
        RequirementSlots = requirementSlots ?? new List<ItemSlot>();
    }
}
