using UnityEngine;

public class QuestDef : Def
{
    public override string DefTypeLabel => "Quest";

    /// <summary>
    /// Whether this quest can have multiple active instances at the same time.
    /// </summary>
    public bool CanHaveMultipleInstances { get; init; } = false;

    /// <summary>
    /// The DefName of the EncounterDef that is automatically placed on a nearby tile when this quest is started.
    /// <br/>Null means no encounter is auto-placed (the quest location is set manually or the quest has no location).
    /// <br/>Resolved to <see cref="PlacedEncounterDef"/> after all Defs are loaded.
    /// </summary>
    public string PlacedEncounterDefName { get; init; } = null;

    /// <summary>
    /// The resolved EncounterDef that is placed when this quest starts. Null if <see cref="PlacedEncounterDefName"/> is null.
    /// </summary>
    public EncounterDef PlacedEncounterDef { get; private set; }

    /// <summary>
    /// Maximum hex radius from the player's current position where the encounter will be placed.
    /// Only used when <see cref="PlacedEncounterDefName"/> is set.
    /// </summary>
    public int EncounterPlacementRadius { get; init; } = 4;

    /// <summary>
    /// The quest log text when the quest is fully known.
    /// <br/>Can contain {0} which will be replaced with the coordinates of the quest location.
    /// </summary>
    public string QuestText { get; init; } = "";

    /// <summary>
    /// If true, it is validated that the quest has a location when it is started. If false, the quest can be started without a location.
    /// </summary>
    public bool RequiresLocation { get; init; } = false;

    /// <summary>
    /// If true, it is validated that a related item is attached to the quest when it is started. If false, the quest can be started without a related item.
    /// </summary>
    public bool RequiresItem { get; init; } = false;

    public QuestDef(string defName) : base(defName) { }

    public override void OnLoadingDefsDone()
    {
        base.OnLoadingDefsDone();

        if (PlacedEncounterDefName != null)
        {
            if (DefDatabase<EncounterDef>.TryGetNamed(PlacedEncounterDefName, out var encounterDef))
                PlacedEncounterDef = encounterDef;
            else
                throw new System.Exception($"QuestDef '{DefName}': Could not resolve EncounterDef '{PlacedEncounterDefName}'.");
        }
    }
}
