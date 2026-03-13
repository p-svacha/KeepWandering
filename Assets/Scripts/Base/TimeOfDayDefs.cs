using System.Collections.Generic;
using UnityEngine;

public static class TimeOfDayDefs
{
    public static List<TimeOfDayDef> Defs => new List<TimeOfDayDef>()
    {
        new TimeOfDayDef("Morning")
        {
            Label = "Morning",
            Description = "In the morning, the player can choose their action for the day. Move, Stay or Rest. They can use items freely.",
            SkyColor = new Color(0.66f, 0.75f, 0.78f),
            LightingAmbienceOverlayColor = new Color(0.94f, 0.90f, 0.55f, 0.08f),
        },
        new TimeOfDayDef("Afternoon")
        {
            Label = "Afternoon",
            Description = "In the afternoon, the player encounters the Location Encounter of the world tile they are currently on. They can only use items when the encounter is over before moving on, except for encounter options.",
            SkyColor = new Color(0.53f, 0.81f, 0.92f),
            LightingAmbienceOverlayColor = new Color(1f, 1f, 1f, 0f),
        },
        new TimeOfDayDef("Evening")
        {
            Label = "Evening",
            Description = "In the evening, the player encounters the Biome Encounter based on the biome they are in. They can only use items when the encounter is over before moving on, except for encounter options.",
            SkyColor = new Color(0.48f, 0.29f, 0.22f),
            LightingAmbienceOverlayColor = new Color(1f, 0.50f, 0.31f, 0.12f),
        },
        new TimeOfDayDef("Night")
        {
            Label = "Night",
            Description = "The night only comes if there is one or more Night Encounters. The player can only use items when the encounter is over before moving on, except for encounter options.",
            SkyColor = new Color(0.06f, 0.07f, 0.10f),
            LightingAmbienceOverlayColor = new Color(0.10f, 0.10f, 0.44f, 0.25f),
        }
    };
}
