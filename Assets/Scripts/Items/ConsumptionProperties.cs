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
    public ConsumptionTypeDef ConsumptionType { get; private set; }

    /// <summary>
    /// The amount of nutrition provided by consuming the item.
    /// </summary>
    public float Nutrition { get; private set; } = 0f;

    /// <summary>
    /// The amount of hydration provided by consuming the item.
    /// </summary>
    public float Hydration { get; private set; } = 0f;

    /// <summary>
    /// The amount by which the severity of a random negative health condition (e.g., infection, poisoning, etc.) is reduced when the item is consumed. This cannot fully cure a condition.
    /// </summary>
    public float SeverityReduction { get; private set; } = 0f;

    public ConsumptionProperties(ConsumptionTypeDef consumptionType, float Nutrition = 0f, float Hydration = 0f, float SeverityReduction = 0f)
    {
        ConsumptionType = consumptionType;
        this.Nutrition = Nutrition;
        this.Hydration = Hydration;
        this.SeverityReduction = SeverityReduction;
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
