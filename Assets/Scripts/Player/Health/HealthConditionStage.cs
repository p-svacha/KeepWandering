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
    /// Flag if the health condition is shown in the health report while this stage is active.
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// The severity value at which this stage becomes active. If the severity value is below this threshold, the previous stage becomes active.
    /// </summary>
    public float SeverityThreshold { get; init; }

    /// <summary>
    /// Text color of the health condition in the health report while this stage is active.
    /// </summary>
    public Color Color { get; init; } = ResourceManager.Color_Text_Default;

    /// <summary>
    /// The stats affected by this stage.
    /// </summary>
    public Dictionary<StatDef, int> StatModifiers { get; init; } = new Dictionary<StatDef, int>();

    /// <summary>
    /// The vitals that are affected by this stage at the end of the day, and how much their severity changes.
    /// </summary>
    public Dictionary<HealthConditionDef, float> EndOfDayVitalChanges { get; init; } = new Dictionary<HealthConditionDef, float>();

    /// <summary>
    /// The health conditions that can be applied to the player when this stage is active, and the chance of each being applied.
    /// </summary>
    public List<(HealthConditionDef Condition, float Chance)> AppliedHealthConditions { get; init; } = new List<(HealthConditionDef, float)>();

    /// <summary>
    /// The modifier to the rolled value of skillchecks that is applied when this stage is active. The first value is the modifier, and the second value is the chance of the modifier being applied.
    /// </summary>
    public (int Modifier, float Chance)? SkillCheckModifier { get; init; } = null;
}
