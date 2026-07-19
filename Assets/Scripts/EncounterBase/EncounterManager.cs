using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The EncounterManager is responsible for chosing which encounters will appear and instantiating them.
public class EncounterManager
{
    private Game Game;
    private WorldMap WorldMap => WorldMap.Instance;
    public Dictionary<EncounterDef, int> NightEncounterAppearances;

    // Forced (dev mode)
    private EncounterDef ForcedLocationEncounter;

    public EncounterManager(Game game)
    {
        Game = game;
        NightEncounterAppearances = new Dictionary<EncounterDef, int>();
    }

    public Encounter GenerateEncounter(EncounterDef def, WorldMapTile tile)
    {
        if(def == null) throw new System.Exception("Cannot generate encounter: def is null.");

        Encounter encounter = System.Activator.CreateInstance(def.EncounterClass) as Encounter;

        if (encounter is NightEncounter) return encounter; // Skip init, because Night encounters are initialized differently (intensity level)

        encounter.Init(Game, def, tile);
        return encounter;
    }

    #region Encounter Selection

    /// <summary>
    /// Choses and returns a new encounter for a tile that the player first steps on.
    /// </summary>
    public EncounterDef SelectLocationEncounterDefFor(WorldMapTile tile)
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
            eventTable.Add(def, GetLocationEncounterProbability(def, tile));
        }
        EncounterDef chosenEncounterDef = eventTable.GetWeightedRandomElement();
        return chosenEncounterDef;
    }

    private float GetLocationEncounterProbability(EncounterDef def, WorldMapTile tile)
    {
        // Get relevant data
        int numAppearances = Game.WorldMap.GetNumAppearances(def);
        bool hasBiomeOverride = def.BiomeProbabilityOverrides.TryGetValue(tile.Biome, out float biomeOverride);

        // Check if the event can even occur
        if (numAppearances >= def.MaxOccurences && def.MaxOccurences > 0) return 0f; // Cannot occur more than once and it already happened
        if (hasBiomeOverride && biomeOverride <= 0f) return 0f; // Cannot occur in this biome
        if (def.MinDistanceBetween > 0)
        {
            bool tooClose = false;
            foreach (WorldMapTile otherTile in WorldMap.Tiles.Values)
            {
                if (otherTile.Encounter != null && otherTile.Encounter.Def == def)
                {
                    int distance = tile.GetHexDistance(otherTile);
                    if (distance < def.MinDistanceBetween)
                    {
                        tooClose = true;
                        break;
                    }
                }
            }
            if (tooClose) return 0f; // Too close to another tile with the same encounter
        }

        // Base probability
        float probability = def.BaseProbability;
        if (hasBiomeOverride) probability = biomeOverride;

        // Repetition modifier
        if (numAppearances > 0)
        {
            probability /= (numAppearances + 1);
        }

        return probability;
    }

    public void ForceEncounter(EncounterDef encounterDef)
    {
        ForcedLocationEncounter = encounterDef;
    }

    public EncounterDef SelectNightEncounterDefFor(WorldMapTile tile)
    {
        // Create a weighted table with the probabilities of each event and chose one
        Dictionary<EncounterDef, float> eventTable = new Dictionary<EncounterDef, float>();
        foreach (EncounterDef def in DefDatabase<EncounterDef>.AllDefs.Where(e => e.Type == EncounterType.Night))
        {
            eventTable.Add(def, GetNightEncounterProbability(def, tile));
        }
        EncounterDef chosenEncounterDef = eventTable.GetWeightedRandomElement();
        return chosenEncounterDef;
    }

    private float GetNightEncounterProbability(EncounterDef def, WorldMapTile tile)
    {
        // Get relevant data
        int numAppearances = NightEncounterAppearances.TryGetValue(def, out int appearances) ? appearances : 0;
        bool hasBiomeOverride = def.BiomeProbabilityOverrides.TryGetValue(tile.Biome, out float biomeOverride);

        // Check if the event can even occur
        if (hasBiomeOverride && biomeOverride <= 0f) return 0f; // Cannot occur in this biome

        // Base probability
        float probability = def.BaseProbability;
        if (hasBiomeOverride) probability = biomeOverride;

        // Repetition modifier
        if (numAppearances > 0)
        {
            probability /= (numAppearances + 1);
        }

        return probability;
    }

    #endregion


}
