using UnityEngine;

/// <summary>
/// Responsible for handling all overarching story logic, ways to win the game, special locations, etc.; so it is all centralized in one place.
/// </summary>
public static class StoryManager
{
    private static Game Game => Game.Instance;
    private static WorldMap WorldMap => Game.WorldMap;


    // One radio tower landmark will have a note hinting at R's location and the existence of the cuttable fence.
    public static Encounter_RadioTower RadioTowerWithNote;

    // On one fence encounter on the perimeter the fence is unpowered from the start. It can be cut with a fence cutter.
    public static WorldMapTile CuttableFenceTile { get; private set; }
    public static Area ClosestAreaOfCuttableFence { get; private set; }

    // R is a person who knows about the location of the unpowered fence, and they give the player the fence cutter to escape.
    public static WorldMapTile HomeOfR { get; private set; }
    public static Area CityOfR => HomeOfR.City;

    /// <summary>
    /// Called once when a new game is started after world generation is done.
    /// </summary>
    public static void OnGameStarted()
    {
        RadioTowerWithNote = WorldMap.GetRandomTile(encounter: EncounterDefOf.RadioTower).Encounter as Encounter_RadioTower;
        RadioTowerWithNote.HasNoteOnDoor = true;

        HomeOfR = WorldMap.GetRandomTile(biome: BiomeDefOf.City);
        Game.SetLocationEncounter(HomeOfR, EncounterDefOf.HomeOfR, hidden: true);

        CuttableFenceTile = WorldMap.GetRandomTile(encounter: EncounterDefOf.QuarantineFence);
        (CuttableFenceTile.Encounter as Encounter_QuarantineFence).IsElectrified = false;
        ClosestAreaOfCuttableFence = CuttableFenceTile.GetClosestArea();
    }
}
