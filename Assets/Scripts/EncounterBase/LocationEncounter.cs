using UnityEngine;

/// <summary>
/// An instance of a location encounter that is bound to a world tile.
/// </summary>
public abstract class LocationEncounter : Encounter
{
    public WorldMapTile Tile { get; private set; }
    public bool IsHidden { get; private set; }
    public int NumVisits { get; private set; }
    protected bool IsFirstVisit => NumVisits == 1;
    public int LastVisitDay { get; private set; }
    public int DaysSinceLastVisit => Game.Day - LastVisitDay;


    public new void Init(Game game, EncounterDef def) => throw new System.InvalidOperationException("Use the Init method that includes a WorldMapTile parameter.");
    public void Init(Game game, EncounterDef def, WorldMapTile tile)
    {
        Tile = tile;
        Tile.SetEncounter(this);

        IsHidden = true;
        NumVisits = 0;

        base.Init(game, def);
    }

    public void Reveal()
    {
        IsHidden = false;
        WorldMapRenderer.Instance.SetMarkerTile(Tile, Def);
    }

    protected override sealed void OnStartExtension()
    {
        base.OnStartExtension();
        NumVisits++;
        LastVisitDay = Game.Day;
    }
}
