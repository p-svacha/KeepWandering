using System.Collections.Generic;
using UnityEngine;

public static class ConsumptionTypeDefs
{
    public static List<ConsumptionTypeDef> Defs => new List<ConsumptionTypeDef>()
    {
        new ConsumptionTypeDef()
        {
            DefName = "Eat",
            Label = "eat"
        },
        new ConsumptionTypeDef()
        {
            DefName = "Drink",
            Label = "drink"
        },
    };
}
