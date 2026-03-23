using System.Collections.Generic;
using UnityEngine;

public static class QuestDefs
{
    public static List<QuestDef> Defs => new List<QuestDef>()
    {
        new QuestDef("FindR")
        {
            Description = "R lives on random tile in a specific city.",
            QuestText = "Find R in {0}.",
        },
        new QuestDef("DeliverMedicineToR")
        {
            Description = "R's partner is sick. R needs medicine to get better.",
            QuestText = "Deliver something that can heal infections to R.",
        },
        new QuestDef("GoToUnpoweredFence")
        {
            Description = "There is a border tile where the electric fence is not unpowered. With a fence cutter it is possible to cut through the fence and get to the other side.",
            QuestText = "The fence at {0} is unpowered and can be cut with a fence cutter.",
            PartialQuestText = "The radio voice mentioned {0} and a fence cutter.",
        },
        new QuestDef("InvestigateSupplyStash")
        {
            Description = "A rumour pointed to a hidden supply stash nearby.",
            IsRepeatable = true,
            PlacedEncounterDefName = "SupplyStash",
            EncounterPlacementRadius = 4,
            QuestText = "Investigate the supply stash at {0}.",
            PartialQuestText = "Investigate the location at {0}.",
        }
    };
}
