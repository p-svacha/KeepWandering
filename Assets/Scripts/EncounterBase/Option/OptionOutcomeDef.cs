using UnityEngine;

public class OptionOutcomeDef : Def
{
    public override string DefTypeLabel => "Option Outcome";

    /// <summary>
    /// The higher the number, the better the outcome. Used for sorting.
    /// </summary>
    public int SuccessLevel { get; init; }
    public Color Color { get; init; }
}
