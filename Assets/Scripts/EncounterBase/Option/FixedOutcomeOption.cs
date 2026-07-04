using System.Collections.Generic;
using System.Linq;

public class FixedOutcomeOption : EncounterOption
{
    public override EncounterOptionType Type => EncounterOptionType.FixedOutcome;

    /// <summary>
    /// The function that gets executed when choosing this encounter step option. Handles the logic of the outcome and returns the text displayed on the next step.
    /// </summary>
    public System.Func<string> Action { get; init; }

    public override void Init()
    {
        base.Init();

        // Validate
        if (Action == null) throw new System.Exception("Action function cannot be null for FixedOutcomeOption.");
    }

    public override string Execute(out OptionOutcomeDef outcome)
    {
        outcome = null;
        return Action.Invoke();
    }
}
