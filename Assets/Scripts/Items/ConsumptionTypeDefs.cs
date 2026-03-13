using System.Collections.Generic;
using UnityEngine;

public static class ConsumptionTypeDefs
{
    public static List<ConsumptionTypeDef> Defs => new List<ConsumptionTypeDef>()
    {
        new ConsumptionTypeDef("Eat")
        {
            Label = "eat"
        },
        new ConsumptionTypeDef("Drink")
        {
            Label = "drink"
        },
    };
}
