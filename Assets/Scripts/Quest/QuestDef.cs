using UnityEngine;

public class QuestDef : Def
{
    public override string DefTypeLabel => "Quest";

    /// <summary>
    /// Whether this quest can have multiple active instances at the same time.
    /// When a repeatable quest is completed, its state returns to Inactive instead of Completed.
    /// </summary>
    public bool IsRepeatable { get; init; } = false;

    public QuestDef(string defName) : base(defName) { }
}
