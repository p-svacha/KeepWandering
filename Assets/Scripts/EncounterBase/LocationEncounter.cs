using UnityEngine;

/// <summary>
/// An instance of a location encounter that is bound to a world tile.
/// </summary>
public abstract class LocationEncounter : Encounter
{
    public WorldMapTile Tile { get; private set; }

    public new void Init(Game game, EncounterDef def) => throw new System.InvalidOperationException("Use the Init method that includes a WorldMapTile parameter.");
    public void Init(Game game, EncounterDef def, WorldMapTile tile)
    {
        base.Init(game, def);
        Tile = tile;
    }
}
