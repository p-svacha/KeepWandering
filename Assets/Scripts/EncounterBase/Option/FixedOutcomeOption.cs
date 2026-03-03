using System.Collections.Generic;
using UnityEngine;

public class FixedOutcomeOption : EncounterStepOption
{
    public override EncounterStepOptionType Type => EncounterStepOptionType.FixedOutcome;

    /// <summary>
    /// The function that gets executed when choosing this encounter step option. Function must return the next step in the encounter.
    /// </summary>
    public System.Func<EncounterStep> Action { get; private set; }

    public FixedOutcomeOption(string text, System.Func<EncounterStep> action, List<ItemSlot> requirementSlots = null) : base(text, requirementSlots)
    {
        Action = action;
    }

    public override EncounterStep Execute()
    {
        return Action.Invoke();
    }
}
