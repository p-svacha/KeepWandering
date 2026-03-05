using System.Collections.Generic;
using UnityEngine;

public static class TimeOfDayDefs
{
    public static List<TimeOfDayDef> Defs => new List<TimeOfDayDef>()
    {
        new TimeOfDayDef()
        {
            DefName = "Morning",
            Label = "Morning",
            Description = "In the morning, the player can choose their action for the day. Move, Stay or Rest. They can use items freely.",
        },
        new TimeOfDayDef()
        {
            DefName = "Afternoon",
            Label = "Afternoon",
            Description = "In the afternoon, the player encounters the Location Encounter of the world tile they are currently on. They can only use items when the encounter is over before moving on, except for encounter options.",
        },
        new TimeOfDayDef()
        {
            DefName = "Evening",
            Label = "Evening",
            Description = "In the evening, the player encounters the Biome Encounter based on the biome they are in. They can only use items when the encounter is over before moving on, except for encounter options.",
        },
        new TimeOfDayDef()
        {
            DefName = "Night",
            Label = "Night",
            Description = "The night only comes if there is one or more Night Encounters. The player can only use items when the encounter is over before moving on, except for encounter options.",
        }
    };
}
