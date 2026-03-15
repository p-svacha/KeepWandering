using System.Collections.Generic;
using UnityEngine;

public static class QuestDefs
{
    public static List<QuestDef> Defs => new List<QuestDef>()
    {
        new QuestDef("FindR")
        {
            Description = "R lives on random tile in a specific city.",
        },
        new QuestDef("DeliverMedicineToR")
        {
            Description = "R's partner is sick. R needs medicine to get better.",
        },
        new QuestDef("GoToUnpoweredFence")
        {
            Description = "There is a border tile where the electric fence is not unpowered. With a fence cutter it is possible to cut through the fence and get to the other side.",
        },
        new QuestDef("InvestigateRumour")
        {
            Description = "A quest created from learning a rumour. Multiple instances can be active at the same time.",
            IsRepeatable = true,
        }
    };
}
