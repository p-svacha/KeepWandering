using UnityEngine;

public enum EncounterType
{
    /// <summary>
    /// Safety check.
    /// </summary>
    Invalid,

    /// <summary>
    /// Only used once for the morning encounter. Same every day.
    /// </summary>
    Morning,

    /// <summary>
    /// These are the main encounters that the player encounters in the afternoon and are bound to a specific tile on the world map. Location encounters are persistent and can be returned to later with the same state as they were left in.
    /// </summary>
    Location,

    /// <summary>
    /// A special kind of location encounter that is created during world generation and is visible on the world map from the start.
    /// </summary>
    Landmark,

    /// <summary>
    /// A special kind of location encounter that is created during world generation or the game on specific conditions. Location encounters of these do never appear naturally and only when they are specifically set on a tile.
    /// </summary>
    ForcePlacedOnly,

    /// <summary>
    /// Only used once for the evening encounter. Same every day but with variations based on the biome.
    /// </summary>
    Evening,

    /// <summary>
    /// These are special encounters that can be randomly encountered during the night. They are not tied to any specific location on the world map.
    /// </summary>
    Night
}
