using UnityEngine;

/// <summary>
/// Defines a rumour that the player can learn. A rumour is a randomized way to acquire a quest.
/// <br/>Learning a rumour starts the referenced quest, which may automatically place an encounter on a nearby tile.
/// </summary>
public class RumourDef : Def
{
    public override string DefTypeLabel => "Rumour";

    /// <summary>
    /// The DefName of the QuestDef that is given to the player when this rumour is learned.
    /// <br/>Resolved to <see cref="QuestDef"/> after all Defs are loaded.
    /// </summary>
    public string QuestDefName { get; init; }

    /// <summary>
    /// The resolved QuestDef that is started when this rumour is learned.
    /// </summary>
    public QuestDef QuestDef { get; private set; }

    /// <summary>
    /// The text describing the rumour, shown to the player when they learn it fully.
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string RumourText { get; init; } = "";

    /// <summary>
    /// The text shown when the rumour is learned partially (location revealed but not what to expect).
    /// <br/>Can contain {0} which will be replaced with the coordinates of the placed encounter.
    /// </summary>
    public string PartialRumourText { get; init; } = "";

    public RumourDef(string defName) : base(defName) { }

    public override void OnLoadingDefsDone()
    {
        base.OnLoadingDefsDone();

        if (QuestDefName != null && DefDatabase<QuestDef>.TryGetNamed(QuestDefName, out var questDef))
            QuestDef = questDef;
        else
            throw new System.Exception($"RumourDef '{DefName}': Could not resolve QuestDef '{QuestDefName}'.");
    }
}
