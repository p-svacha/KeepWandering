using System.Collections.Generic;
using System.Linq;

public class FixedOutcomeOption : EncounterOption
{
    public override EncounterStepOptionType Type => EncounterStepOptionType.FixedOutcome;

    /// <summary>
    /// The function that gets executed when choosing this encounter step option. Function must return the next step in the encounter.
    /// </summary>
    public System.Func<EncounterStep> Action { get; private set; }

    public FixedOutcomeOption(string text, string description, System.Func<EncounterStep> action, List<ItemSlot> itemSlots = null) : base(text, description, itemSlots)
    {
        Action = action;

        // Validate
        if (ItemSlots.Any(slot => slot.DefaultDifficultyReduction != 0))
            throw new System.Exception("FixedOutcomeOption cannot have item slots with difficulty reduction, since it does not involve any checks. All item slots must have a default difficulty reduction of 0.");
        if (ItemSlots.Any(slot => slot.DifficultyReductionOverrides.Count > 0))
            throw new System.Exception("FixedOutcomeOption cannot have item slots with difficulty reduction overrides, since it does not involve any checks. All item slots must have no difficulty reduction overrides.");
    }

    public override EncounterStep Execute(out OptionOutcomeDef outcome)
    {
        outcome = null;
        return Action.Invoke();
    }
}
