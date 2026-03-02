using System.Collections.Generic;
using UnityEngine;

public static class EncounterDefs
{
    public static List<EncounterDef> Defs => new List<EncounterDef>()
    {
        new EncounterDef()
        {
            DefName = "Crate",
            DevNotes = "A crate lies before the player, containing a randomised visible player. The player can try to pry the item out, risking an injury in the process.",
            EncounterClass = typeof(Encounter_Crate),
            EncounterType = EncounterType.Location,
            BaseProbability = 6,
            Biomes = new Dictionary<BiomeDef, float>()
            {
                {BiomeDefOf.Farmland, 1.1f},
                {BiomeDefOf.City, 0.2f},
                {BiomeDefOf.Woods, 0.9f},
            },
        },
    };
}
