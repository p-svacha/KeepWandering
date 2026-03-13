using System.Collections.Generic;
using UnityEngine;

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
