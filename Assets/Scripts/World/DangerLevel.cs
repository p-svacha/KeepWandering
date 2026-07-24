using System.Collections.Generic;
using UnityEngine;

public enum DangerLevel
{
    VerySafe = 0,
    Safe = 1,
    Precarious = 2,
    Dangerous = 3,
    VeryDangerous = 4
}

public class DangerLevelDef : Def
{
    public override string DefTypeLabel => "Danger Level";

    /// <summary>
    /// The color the UI text is displayed in when the player is in a location with this danger level.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// Numerized enum for sorting and incrementing/decrementing.
    /// </summary>
    public DangerLevel DangerLevel { get; init; }

    /// <summary>
    /// The probabilities for how likely it is for a night encounter to occur (and with what intensity) at the end of the day when the player is in a location with this danger level. Key of 0 means no encounter.
    /// </summary>
    public Dictionary<int, float> NightEncounterIntensities { get; init; } = null;

    public DangerLevelDef(string defName) : base(defName) { }
}

public static class DangerLevelDefs
{
    public static List<DangerLevelDef> Defs => new List<DangerLevelDef>()
    {
        new DangerLevelDef("VerySafe")
        {
            Label = "Very Safe",
            Description = "Noone will attack during the night.",
            DangerLevel = DangerLevel.VerySafe,
            Color = ResourceManager.Color_Text_VeryPositive,
            NightEncounterIntensities = new Dictionary<int, float>()
            {
                { 0, 100 },
            },
        },
        new DangerLevelDef("Safe")
        {
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
        new DangerLevelDef("Precarious")
        {
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
        new DangerLevelDef("Dangerous")
        {
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
        new DangerLevelDef("VeryDangerous")
        {
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

[DefOf]
public static class DangerLevelDefOf
{
    public static DangerLevelDef VerySafe;
    public static DangerLevelDef Safe;
    public static DangerLevelDef Precarious;
    public static DangerLevelDef Dangerous;
    public static DangerLevelDef VeryDangerous;
}

