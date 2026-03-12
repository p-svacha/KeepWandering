using System.Collections.Generic;
using UnityEngine;

public static class DangerLevelDefs
{
    public static List<DangerLevelDef> Defs => new List<DangerLevelDef>()
    {
        new DangerLevelDef()
        {
            DefName = "VerySafe",
            Label = "Very Safe",
            Description = "Noone will attack during the night.",
            DangerLevel = DangerLevel.VerySafe,
            Color = ResourceManager.Color_Text_VeryPositive,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 100 },
            },
        },
        new DangerLevelDef()
        {
            DefName = "Safe",
            Label = "Safe",
            Description = "There is a very small chance of a small attack happening at night.",
            DangerLevel = DangerLevel.Safe,
            Color = ResourceManager.Color_Text_Positive,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 95 },
                { 1, 5 },
            },
        },
        new DangerLevelDef()
        {
            DefName = "Precarious",
            Label = "Precarious",
            Description = "There is a small chance of an attack happening at night.",
            DangerLevel = DangerLevel.Precarious,
            Color = ResourceManager.Color_Text_Negative,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 75 },
                { 1, 20 },
                { 2, 5 },
            },
        },
        new DangerLevelDef()
        {
            DefName = "Dangerous",
            Label = "Dangerous",
            Description = "There is a significant chance of an attack happening at night.",
            DangerLevel = DangerLevel.Dangerous,
            Color = ResourceManager.Color_Text_VeryNegative,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 50 },
                { 1, 20 },
                { 2, 20 },
                { 3, 10 },
            },
        },
        new DangerLevelDef()
        {
            DefName = "VeryDangerous",
            Label = "Very Dangerous",
            Description = "There is a very high chance of a potentially devastating attack happening at night.",
            DangerLevel = DangerLevel.VeryDangerous,
            Color = ResourceManager.Color_Text_ExtremelyNegative,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 20 },
                { 1, 10 },
                { 2, 35 },
                { 3, 35 },
            },
        }
    };
}
