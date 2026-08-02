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
    public Dictionary<string, float> _EndOfDayVitalChanges { get; init; } = new Dictionary<string, float>();
    public Dictionary<HealthConditionDef, float> EndOfDayVitalChanges { get; private set; } // Resolved references

    /// <summary>
    /// The health conditions that can be applied to the player when this stage is active, and the chance of each being applied.
    /// </summary>
    public List<(string Condition, float Chance)> _AppliedHealthConditions { get; init; } = new List<(string, float)>();
    public List<(HealthConditionDef Condition, float Chance)> AppliedHealthConditions { get; private set; } // Resolved references

    /// <summary>
    /// The modifier to the rolled value of skillchecks that is applied when this stage is active. The first value is the modifier, and the second value is the chance of the modifier being applied.
    /// </summary>
    public (int Modifier, float Chance)? SkillCheckModifier { get; init; } = null;


    public void ResolveReferences(HealthConditionDef def)
    {
        // Resolve end of day vital changes
        EndOfDayVitalChanges = new Dictionary<HealthConditionDef, float>();
        foreach (var vitalChange in _EndOfDayVitalChanges)
        {
            HealthConditionDef vitalDef = DefDatabase<HealthConditionDef>.GetNamed(vitalChange.Key);
            if (!vitalDef.IsVital) def.ThrowValidationError($"HealthConditionStage has an end of day vital change for '{vitalChange.Key}' which is not a vital health condition.");
            EndOfDayVitalChanges.Add(vitalDef, vitalChange.Value);
        }
        // Resolve applied health conditions
        AppliedHealthConditions = new List<(HealthConditionDef Condition, float Chance)>();
        foreach (var appliedCondition in _AppliedHealthConditions)
        {
            HealthConditionDef conditionDef = DefDatabase<HealthConditionDef>.GetNamed(appliedCondition.Condition);
            AppliedHealthConditions.Add((conditionDef, appliedCondition.Chance));
        }
    }
}
