using UnityEngine;

public abstract class NightEncounter : Encounter
{
    public const int MAX_INTENSITY = 3;

    public int Intensity { get; protected set; } // [1-3] A measure of how dangerous the encounter is.

    public new void Init(Game game, EncounterDef def) => throw new System.InvalidOperationException("Use the Init method that includes an intensity parameter.");
    public void Init(Game game, EncounterDef def, int intensity)
    {
        Intensity = intensity;
        base.Init(game, def);
    }

    protected override bool IsMoveOnOptionAvailable() => false;
}
