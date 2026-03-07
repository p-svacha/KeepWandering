using System.Collections.Generic;
using UnityEngine;

public class EncounterDef : Def
{
    public override string DefTypeLabel => "Encounter";
    public Sprite WorldMapMarker => ResourceManager.LoadSprite("EncounterMarker/" + DefName);

    /// <summary>
    /// The class that gets instantiated when this encounter is selected. Must be a subclass of Encounter.
    /// <br/>The subclass may contain encounter-specific logic that is too complex to be handled by the EncounterDef alone. This often involves some random elements that need to be determined at the time of encounter instantiation.
    /// </summary>
    public System.Type EncounterClass { get; init; } = typeof(Encounter);

    /// <summary>
    /// The type of this encounter, which determines when it can occur.
    /// </summary>
    public EncounterType Type { get; init; } = EncounterType.Invalid;

    /// <summary>
    /// Notes explaining this encounter in plain terms, intended for developers.
    /// </summary>
    public string DevNotes { get; init; }

    /// <summary>
    /// Base probability of this encounter being selected in a weighted random selection. The probability is relative to the base probabilities of all other encounters that are eligible to occur at the same time. This applies to all encounters except Biome encounters, as those appear at fixed times.
    /// </summary>
    public float BaseProbability { get; init; } = 0f;

    /// <summary>
    /// The biomes in which this encounter can occur, along with a multiplier to the base probability for each biome. If a biome is not listed here, the encounter cannot occur in that biome.
    /// </summary>
    public Dictionary<BiomeDef, float> Biomes { get; init; } = new Dictionary<BiomeDef, float>();

    /// <summary>
    /// If set to a positive number, this encounter can only occur up to this many times in a single playthrough. If -1 (the default), there is no limit to the number of times this encounter can occur.
    /// </summary>
    public int MaxOccurences { get; init; } = -1;

    /// <summary>
    /// How many times a landmark is guaranteed to appear on the world map. This only applies to encounters of type Landmark.
    /// </summary>
    public int MinOccurences { get; init; } = 0;

    /// <summary>
    /// The minimum hex tile distance from the starting tile that this encounter can occur at. This is used to prevent certain encounters from occurring too close to the start of the game.
    /// </summary>
    public int MinDistanceFromStart { get; init; } = -1;

    /// <summary>
    /// The minimum amount of tiles that must be between two occurrences of this encounter. This is used to prevent certain encounters from occurring too close to each other.
    /// </summary>
    public int MinDistanceBetween { get; init; } = -1;

    /// <summary>
    /// The orthographic size of the camera when this encounter is active.
    /// </summary>
    public float CameraZoomLevel { get; init; } = EncounterCamera.DEFAULT_CAMERA_SIZE;


    public override bool Validate()
    {
        if (Type == EncounterType.Invalid) throw new System.Exception("Encounter type must be set.");
        if (MaxOccurences == 0) throw new System.Exception("MaxOccurences cannot be set to 0. Use -1 for no limit.");
        if (MinDistanceFromStart == 0 || MinDistanceFromStart == 1) throw new System.Exception("MinDistanceFromStart cannot be set to 0 or 1. Use -1 for no minimum distance.");
        if (Type != EncounterType.Landmark && MinOccurences != 0) throw new System.Exception("Only landmark encounters can have a minimum number of occurences.");

        if (EncounterClass == null) throw new System.Exception("EncounterClass cannot be null.");
        if (!EncounterClass.IsSubclassOf(typeof(Encounter))) throw new System.Exception("EncounterClass must be a subclass of Encounter.");

        if (Type == EncounterType.Location)
        {
            if (BaseProbability == 0f) throw new System.Exception("Location encounters must have a base probability set.");
            if (!EncounterClass.IsSubclassOf(typeof(LocationEncounter))) throw new System.Exception("EncounterClass must be a subclass of LocationEncounter.");
        }

        if (Type == EncounterType.Landmark)
        {
            if (!EncounterClass.IsSubclassOf(typeof(LocationEncounter))) throw new System.Exception("EncounterClass must be a subclass of LocationEncounter.");
            if (BaseProbability != 0f) throw new System.Exception("Landmark encounters cannot have a probability, as their placement depends purely on Min and MaxOccurences.");
            if (MaxOccurences <= 0) throw new System.Exception("Landmark encounters must have a positive maximum number of occurences.");
            if (MaxOccurences < MinOccurences) throw new System.Exception("MaxOccurences cannot be less than MinOccurences.");
        }

        if (Type == EncounterType.Special)
        {
            if (!EncounterClass.IsSubclassOf(typeof(LocationEncounter))) throw new System.Exception("EncounterClass must be a subclass of LocationEncounter.");
            if (BaseProbability != 0f) throw new System.Exception("Special encounters cannot have a probability, as they are only force placed.");
            if (Biomes.Count > 0) throw new System.Exception("Special encounters cannot have biome-specific probabilities, as they are only force placed.");
            if (MaxOccurences != -1) throw new System.Exception("Special encounters cannot be limited, as they are only force placed.");
            if (MinDistanceFromStart != -1) throw new System.Exception("Special encounters cannot have a minimum distance from the starting tile, as they are only force placed.");
            if (MinDistanceBetween != -1) throw new System.Exception("Special encounters cannot have a minimum distance between occurences, as they are only force placed.");
        }

        if (Type == EncounterType.Biome)
        {
            if (MinDistanceFromStart != -1) throw new System.Exception("Biome encounters cannot have a minimum distance from the starting tile.");
            if (MaxOccurences != -1) throw new System.Exception("Biome encounters cannot be limited.");
            if (BaseProbability != 0f) throw new System.Exception("Biome encounters cannot have a probability set.");
            if (Biomes != null && Biomes.Count > 0) throw new System.Exception("Biome encounters cannot have biome-specific probabilities.");
            if (MinDistanceBetween != -1) throw new System.Exception("Biome encounters cannot have a minimum distance between occurences, as they only appear once per biome and are not randomly placed.");
        }

        if (Type == EncounterType.Night)
        {
            if (BaseProbability == 0f) throw new System.Exception("Night encounters must have a base probability set.");
            if (MinDistanceBetween != -1) throw new System.Exception("Night encounters cannot have a minimum distance between occurences, as they happen independently from location in the world.");
        }

        return base.Validate();
    }
}