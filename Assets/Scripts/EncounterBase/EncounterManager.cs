using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The EncounterManager is responsible for chosing which encounters will appear and instantiating them.
public class EncounterManager
{
    private Game Game;

    // Forced (dev mode)
    private EncounterDef ForcedLocationEncounter;

    public EncounterManager(Game game)
    {
        Game = game;
    }

    public Encounter GenerateEncounter(EncounterDef def)
    {
        Encounter encounter = System.Activator.CreateInstance(def.EncounterClass) as Encounter;
        encounter.Init(Game, def);
        return encounter;
    }


    #region Encounter Selection

    /// <summary>
    /// Returns the default calculation of encounter probability taking in account the base probability, the location and days since it last occured.
    /// </summary>
    protected float GetEncounterProbability(EncounterDef def, WorldMapTile tile)
    {
        // Get relevant data
        int numAppearances = Game.WorldMap.GetNumAppearances(def);

        // Check if the event can even occur
        if (numAppearances >= def.MaxOccurences && def.MaxOccurences > 0) return 0f; // Cannot occur more than once and it already happened
        if (!def.Biomes.ContainsKey(tile.Biome)) return 0f; // Cannot occur in this biome

        // Base probability
        float probability = def.BaseProbability;

        // Biome modifier
        probability *= def.Biomes[tile.Biome];

        // Repetition modifier
        if (numAppearances > 0)
        {
            probability /= (numAppearances + 1);
        }

        return probability;
    }

    #endregion

    /// <summary>
    /// Choses and returns a new encounter for a tile that the player first steps on.
    /// </summary>
    public EncounterDef SelectRandomLocationEncounterDefFor(WorldMapTile tile)
    {
        // Forced encounter (dev mode)
        if (ForcedLocationEncounter != null)
        {
            EncounterDef forced = ForcedLocationEncounter;
            ForcedLocationEncounter = null;
            return forced;
        }

        // Create a weighted table with the probabilities of each event and chose one
        Dictionary<EncounterDef, float> eventTable = new Dictionary<EncounterDef, float>();
        foreach (EncounterDef def in DefDatabase<EncounterDef>.AllDefs.Where(e => e.Type == EncounterType.Location))
        {
            eventTable.Add(def, GetEncounterProbability(def, tile));
        }
        EncounterDef chosenEncounterDef = HelperFunctions.GetWeightedRandomElement(eventTable);
        return chosenEncounterDef;
    }

    public void ForceEncounter(EncounterDef encounterDef)
    {
        ForcedLocationEncounter = encounterDef;
    }
}
