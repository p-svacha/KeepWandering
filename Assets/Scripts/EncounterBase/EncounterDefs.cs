using System.Collections.Generic;
using UnityEngine;

public static class EncounterDefs
{
    public static List<EncounterDef> Defs => new List<EncounterDef>()
    {
        new EncounterDef()
        {
            DefName = "MorningEncounter",
            EncounterClass = typeof(Encounter_Morning),
            Type = EncounterType.Morning
        },

        new EncounterDef()
        {
            DefName = "EveningFallback",
            EncounterClass = typeof(BiomeEncounter_Fallback),
            Type = EncounterType.Biome,
        },

        new EncounterDef()
        {
            DefName = "Crate",
            Label = "Crate",
            DevNotes = "A locked container where players can peek inside to identify hidden loot before deciding to squeeze items through a hole, pry it open with tools, or smash it at the risk of destroying the contents.",
            EncounterClass = typeof(Encounter_Crate),
            Type = EncounterType.Location,
            BaseProbability = 6,
            Biomes = new Dictionary<BiomeDef, float>()
            {
                {BiomeDefOf.Farmland, 1.1f},
                {BiomeDefOf.City, 0.2f},
                {BiomeDefOf.Woods, 0.9f},
            },
            CameraZoomLevel = EncounterCamera.DEFAULT_CAMERA_SIZE,
        },

        new EncounterDef()
        {
            DefName = "RadioTower",
            Label = "Radio Tower",
            EncounterClass = typeof(Encounter_RadioTower),
            Type = EncounterType.Landmark,
            CameraZoomLevel = 12f,
            MinOccurences = 2,
            MaxOccurences = 2,
            MinDistanceFromStart = 8,
            MinDistanceBetween = 12,
        },

        new EncounterDef()
        {
            DefName = "QuarantineFence",
            Label = "Quarantine Fence",
            EncounterClass = typeof(Encounter_QuarantineFence),
            Type = EncounterType.Special,
            CameraZoomLevel = 8.5f,
        },

        new EncounterDef()
        {
            DefName = "HomeOfR",
            Label = "Home of R",
            EncounterClass = typeof(Encounter_HomeOfR),
            Type = EncounterType.Special,
            CameraZoomLevel = 6f,
        }
    };
}
