using System.Collections.Generic;
using UnityEngine;

public static class RumourDefs
{
    public static List<RumourDef> Defs => new List<RumourDef>()
    {
        new RumourDef("SupplyStash")
        {
            Label = "supply stash",
            Description = "A hidden stash of supplies nearby.",
            EncounterDef = EncounterDefOf.SupplyStash,
            RumourText = "There's a hidden supply stash at {0}.",
            PartialRumourText = "There's something hidden at {0}.",
            QuestText = "Investigate the supply stash at {0}.",
            PartialQuestText = "Investigate the location at {0}.",
            IsRepeatable = true,
            MaxPlacementRadius = 4,
        },
    };
}
