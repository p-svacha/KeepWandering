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
    /// These are the encounters that the player encounters in the evening. They are purely based on the biome of the current tile and are not persistent, meaning they do not have a specific state, so they are always encountered in their default state. Biome encounters are not meant to be narratively significant, but rather to give some control to the player as most other things in the game are very random and out of the player's control.
    /// </summary>
    Biome,

    /// <summary>
    /// These are special encounters that can be randomly encountered during the night. They are not tied to any specific location on the world map.
    /// </summary>
    Night
}
