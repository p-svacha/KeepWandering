using System.Collections.Generic;
using UnityEngine;

public class HealthConditionStage
{
    /// <summary>
    /// If set, this label overrides the default label of the health condition while this stage is active.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// If set, this description overrides the default description of the health condition while this stage is active.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// The stats affected by this stage.
    /// </summary>
    public Dictionary<StatDef, int> StatModifiers { get; init; } = new Dictionary<StatDef, int>();
}
