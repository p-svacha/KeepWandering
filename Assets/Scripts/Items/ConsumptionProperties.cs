using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the properties of an ItemDef that can be consumed, such as food or medicine. This class is used to define the effects of consuming the item, such as nutrition, hydration, and any additional effects.
/// </summary>
public class ConsumptionProperties
{
    /// <summary>
    /// The type of consumption for the item, such as food, drink, or drug. General categorization.
    /// </summary>
    public ConsumptionTypeDef ConsumptionType { get; init; }

    /// <summary>
    /// The amount of nutrition provided by consuming the item.
    /// </summary>
    public float Nutrition { get; init; } = 0f;

    /// <summary>
    /// The amount of hydration provided by consuming the item.
    /// </summary>
    public float Hydration { get; init; } = 0f;

    /// <summary>
    /// The amount by which the severity of a random negative health condition is reduced when the item is consumed. This cannot fully cure a condition.
    /// </summary>
    public float SeverityReduction { get; init; } = 0f;

    /// <summary>
    /// A dictionary of stat changes that occur when the item is consumed.
    /// </summary>
    public Dictionary<StatDef, int> StatChanges { get; init; } = new Dictionary<StatDef, int>();

    /// <summary>
    /// Health condition that gets applied when the item is consumed.
    /// </summary>
    public HealthConditionDef AppliedHealthCondition { get; init; }

    /// <summary>
    /// The chance that the applied health condition will be applied when the item is consumed. A value of 1 means it will always be applied, while a value of 0 means it will never be applied.
    /// </summary>
    public float AppliedHealthConditionChance { get; init; } = 1f;

    /// <summary>
    /// Can be used to apply a health condition with a specific severity when the item is consumed. If <= 0, the default initial severity from the health condition definition will be used. If > 0, this value will be used as the initial severity when applying the condition.
    /// </summary>
    public float AppliedHealthConditionSeverity { get; init; } = -1;

    public void Validate(ItemDef def)
    {
        foreach (var statChange in StatChanges)
        {
            if (statChange.Value == 0) def.ThrowValidationError($"ConsumptionProperties has a stat change for '{statChange.Key.DefName}' with a value of 0. Stat changes must be non-zero.");
        }

        if (AppliedHealthCondition == null && AppliedHealthConditionSeverity != -1)
        {
            def.ThrowValidationError($"ConsumptionProperties has a positive applied health condition severity of {AppliedHealthConditionSeverity} but no applied health condition. An applied health condition must be specified if the severity is greater than 0.");
        }
        if (AppliedHealthCondition == null && AppliedHealthConditionChance != 1)
        {
            def.ThrowValidationError($"ConsumptionProperties has an applied health condition chance of {AppliedHealthConditionChance} but no applied health condition. An applied health condition must be specified if the chance is not 1.");
        }
        if (AppliedHealthCondition != null && (AppliedHealthConditionChance <= 0 || AppliedHealthConditionChance > 1))
        {
            def.ThrowValidationError($"ConsumptionProperties has an applied health condition chance of {AppliedHealthConditionChance}. This value must be between 0 (exclusive) and 1 (inclusive).");
        }
    }
}

public class ConsumptionTypeDef : Def
{
    public override string DefTypeLabel => "Consumption Type";
    public ConsumptionTypeDef(string defName) : base(defName) { }
    public string Verb { get; init; }
}

public static class ConsumptionTypeDefs
{
    public static List<ConsumptionTypeDef> Defs => new List<ConsumptionTypeDef>()
    {
        new ConsumptionTypeDef("Food")
        {
            Label = "Food",
            Verb = "eat",
        },
        new ConsumptionTypeDef("Drink")
        {
            Label = "Drink",
            Verb = "drink",
        },
        new ConsumptionTypeDef("Drug")
        {
            Label = "Drug",
            Verb = "consume",
        }
    };
}

[DefOf]
public static class ConsumptionTypeDefOf
{
    public static ConsumptionTypeDef Food;
    public static ConsumptionTypeDef Drink;
    public static ConsumptionTypeDef Drug;
}
