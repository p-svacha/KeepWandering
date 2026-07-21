using UnityEngine;

/// <summary>
/// An instance of a location encounter that is bound to a world tile.
/// </summary>
public abstract class LocationEncounter : Encounter
{
    public bool IsHidden { get; private set; }
    public bool IsVisible => !IsHidden;
    public int NumVisits { get; private set; }
    protected bool IsFirstVisit => NumVisits == 1;
    public int LastVisitDay { get; private set; }
    public int DaysSinceLastVisit => Game.Day - LastVisitDay;


    public override void Init(Game game, EncounterDef def, WorldMapTile tile)
    {
        base.Init(game, def, tile);

        Tile.SetEncounter(this);
        IsHidden = true;
        NumVisits = 0;
    }

    public void Reveal()
    {
        IsHidden = false;
    }

    protected override sealed void OnStartExtension()
    {
        base.OnStartExtension();
        NumVisits++;
        LastVisitDay = Game.Day;
    }
}
