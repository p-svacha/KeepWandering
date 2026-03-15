using UnityEngine;

/// <summary>
/// Defines a rumour that the player can learn. Learning a rumour places a specific encounter on a nearby tile
/// and creates a quest pointing to that location.
/// </summary>
public class RumourDef : Def
{
    public override string DefTypeLabel => "Rumour";

    /// <summary>
    /// The encounter that gets placed on the world map when this rumour is learned.
    /// </summary>
    public EncounterDef EncounterDef { get; init; }

    /// <summary>
    /// The text describing the rumour, shown to the player when they learn it fully.
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string RumourText { get; init; } = "";

    /// <summary>
    /// The text shown when the rumour is learned partially (location revealed but not what to expect).
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string PartialRumourText { get; init; } = "";

    /// <summary>
    /// The quest text when the rumour is learned fully.
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string QuestText { get; init; } = "";

    /// <summary>
    /// The quest text when the rumour is learned partially.
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string PartialQuestText { get; init; } = "";

    /// <summary>
    /// Whether this rumour can be learned multiple times. Repeatable rumours can generate multiple active quests of the same type.
    /// </summary>
    public bool IsRepeatable { get; init; } = false;

    /// <summary>
    /// Maximum hex radius from the player's current position where the encounter will be placed.
    /// </summary>
    public int MaxPlacementRadius { get; init; } = 4;

    public RumourDef(string defName) : base(defName) { }
}
