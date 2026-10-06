using System.Collections.Generic;
using UnityEngine;

public static class QuestDefs
{
    public static List<QuestDef> Defs => new List<QuestDef>()
    {
        new QuestDef("FindR")
        {
            Description = "R lives on random tile in a specific city.",
            QuestText = "Find R in {LOC}.",
            RequiresLocation = true,
        },
        new QuestDef("DeliverMedicineToR")
        {
            Description = "R's partner is sick. R needs medicine to get better.",
            QuestText = "Deliver something that can heal infections to R.",
            RequiresLocation = true,
        },
        new QuestDef("GoToUnpoweredFence")
        {
            Description = "There is a border tile where the electric fence is not unpowered. With a fence cutter it is possible to cut through the fence and get to the other side.",
            QuestText = "The fence at {LOC} is unpowered and can be cut with a fence cutter.",
            RequiresLocation = true,
        },
        new QuestDef("InvestigateSupplyStash")
        {
            Description = "A rumour pointed to a hidden supply stash nearby.",
            CanHaveMultipleInstances = true,
            PlacedEncounterDefName = "SupplyStash",
            EncounterPlacementRadius = 4,
            QuestText = "Investigate the supply stash at {LOC}.",
            RequiresLocation = true,
        },
        new QuestDef("DeliverGuardBribe")
        {
            Description = "A guard is willing to accept a bribe to look the other way.",
            QuestText = "Deliver {ITEM} to the guard at {LOC}.",
            RequiresLocation = true,
            RequiresItem = true,
        }
    };
}

[DefOf]
public static class QuestDefOf
{
    public static QuestDef FindR;
    public static QuestDef DeliverMedicineToR;
    public static QuestDef GoToUnpoweredFence;
    public static QuestDef InvestigateSupplyStash;
    public static QuestDef DeliverGuardBribe;
}
