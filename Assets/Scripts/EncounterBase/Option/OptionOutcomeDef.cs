using UnityEngine;

public class OptionOutcomeDef : Def
{
    public override string DefTypeLabel => "Option Outcome";

    /// <summary>
    /// The higher the number, the better the outcome. Used for sorting or logic that applies to multiple outcomes above/below a certain level.
    /// </summary>
    public SuccessLevel SuccessLevel { get; init; }

    /// <summary>
    /// If the player very generally achieves what they wanted with this option.
    /// </summary>
    public bool IsSuccess { get; init; }

    public Color Color { get; init; }
}

public enum SuccessLevel
{
    CriticalSuccess = 5,
    Success = 4,
    PartialSuccess = 3,
    Failure = 2,
    CriticalFailure = 1,
}
