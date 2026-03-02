using System.Collections.Generic;
using UnityEngine;

public class EncounterDef : Def
{
    public override string DefTypeLabel => "Encounter";

    /// <summary>
    /// The class that gets instantiated when this encounter is selected. Must be a subclass of Encounter.
    /// <br/>The subclass may contain encounter-specific logic that is too complex to be handled by the EncounterDef alone. This often involves some random elements that need to be determined at the time of encounter instantiation.
    /// </summary>
    public System.Type EncounterClass { get; init; } = typeof(Encounter);

    /// <summary>
    /// The type of this encounter, which determines when it can occur.
    /// </summary>
    public EncounterType EncounterType { get; init; }

    /// <summary>
    /// Notes explaining this encounter in plain terms, intended for developers.
    /// </summary>
    public string DevNotes { get; init; }

    /// <summary>
    /// Base probability of this encounter being selected in a weighted random selection.
    /// </summary>
    public float BaseProbability { get; init; }

    /// <summary>
    /// The biomes in which this encounter can occur, along with a multiplier to the base probability for each biome. If a biome is not listed here, the encounter cannot occur in that biome.
    /// </summary>
    public Dictionary<BiomeDef , float> Biomes { get; init; } = new Dictionary<BiomeDef, float>();

    /// <summary>
    /// If true, this encounter can only occur once per game (for location encounters this means only on one tile).
    /// </summary>
    public bool CanOnlyOccurOnce { get; init; } = false;
}