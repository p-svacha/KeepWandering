using UnityEngine;

public abstract class NightEncounter : Encounter
{
    public const int MAX_INTENSITY = 3;

    private Camp Camp => Camp.Instance;
    private CampRenderer CampRenderer;

    public int Intensity { get; protected set; } // [1-3] A measure of how dangerous the encounter is.

    public new void Init(Game game, EncounterDef def, WorldMapTile tile) => throw new System.InvalidOperationException("Use the Init method that includes an intensity parameter.");
    public void Init(Game game, EncounterDef def, WorldMapTile tile, int intensity)
    {
        Intensity = intensity;
        CampRenderer = Game.EncounterContainer.transform.Find($"NightCamp").GetChild(0).GetComponent<CampRenderer>();
        CampRenderer.gameObject.SetActive(true);
        base.Init(game, def, tile);
    }

    protected override void RefreshSprites()
    {
        CampRenderer.Refresh();
    }

    protected override void OnEnd()
    {
        CampRenderer.gameObject.SetActive(false);
    }

    protected override bool IsMoveOnOptionAvailable() => false;

    public override float CameraXOffset => -2f; // so camp is visible
}
