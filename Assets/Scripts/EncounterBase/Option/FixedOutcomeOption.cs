using System.Collections.Generic;
using System.Linq;

public class FixedOutcomeOption : EncounterOption
{
    public override EncounterStepOptionType Type => EncounterStepOptionType.FixedOutcome;

    /// <summary>
    /// The function that gets executed when choosing this encounter step option. Handles the logic of the outcome and returns the text displayed on the next step.
    /// </summary>
    public System.Func<string> Action { get; init; }

    public override void Init()
    {
        base.Init();

        // Validate
        if (Action == null) throw new System.Exception("Action function cannot be null for FixedOutcomeOption.");
        if (ItemSlots.Any(slot => slot.DifficultyReduction != 0))
            throw new System.Exception("FixedOutcomeOption cannot have item slots with difficulty reduction, since it does not involve any checks. All item slots must have a default difficulty reduction of 0.");
        if (ItemSlots.Any(slot => slot.DifficultyReductionOverrides.Count > 0))
            throw new System.Exception("FixedOutcomeOption cannot have item slots with difficulty reduction overrides, since it does not involve any checks. All item slots must have no difficulty reduction overrides.");
    }

    public override string Execute(out OptionOutcomeDef outcome)
    {
        outcome = null;
        return Action.Invoke();
    }
}
