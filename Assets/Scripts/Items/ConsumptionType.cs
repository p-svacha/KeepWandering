using System.Collections.Generic;
using UnityEngine;

public class ConsumptionTypeDef : Def
{
    public override string DefTypeLabel => "Consumption Type";

    public ConsumptionTypeDef(string defName) : base(defName) { }

    public string ConsumptionVerb { get; init; }
}

public static class ConsumptionTypeDefs
{
    public static List<ConsumptionTypeDef> Defs => new List<ConsumptionTypeDef>()
    {
        new ConsumptionTypeDef("Food")
        {
            Label = "Food",
            ConsumptionVerb = "eat"
        },
        new ConsumptionTypeDef("Drink")
        {
            Label = "Drink",
            ConsumptionVerb = "drink"
        },
        new ConsumptionTypeDef("Drug")
        {
            Label = "Drug",
            ConsumptionVerb = "take"
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
