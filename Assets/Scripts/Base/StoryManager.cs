using UnityEngine;

/// <summary>
/// Responsible for handling all overarching story logic, ways to win the game, special locations, etc.; so it is all centralized in one place.
/// </summary>
public static class StoryManager
{
    private static Game Game => Game.Instance;
    private static WorldMap WorldMap => Game.WorldMap;

    // Important locations

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
        HomeOfR = WorldMap.GetRandomTile(biome: BiomeDefOf.City);
        CuttableFenceTile = WorldMap.GetRandomTile(mustBorderFence: true);
        ClosestAreaOfCuttableFence = CuttableFenceTile.GetClosestArea();
    }
}
