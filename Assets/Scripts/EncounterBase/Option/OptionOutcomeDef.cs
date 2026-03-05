using UnityEngine;

public class OptionOutcomeDef : Def
{
    public override string DefTypeLabel => "Option Outcome";

    /// <summary>
    /// The higher the number, the better the outcome. Used for sorting or logic that applies to multiple outcomes above/below a certain level.
    /// </summary>
    public int SuccessLevel { get; init; }

    /// <summary>
    /// If the player very generally achieves what they wanted with this option.
    /// </summary>
    public bool IsSuccess { get; init; }

    public Color Color { get; init; }
}
