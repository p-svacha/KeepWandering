using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDef : Def
{
    public override string DefTypeLabel => "Item";
    public override Sprite Sprite => Resources.Load<Sprite>("Items/" + DefName);

    /// <summary>
    /// If true, this item will not appear in random selections.
    /// </summary>
    public bool IsQuestItem { get; init; }
    public List<ItemTagDef> Tags { get; init; } = new List<ItemTagDef>();

    // Consumption
    public ConsumptionTypeDef ConsumptionType { get; init; } = null;
    public bool IsConsumable => ConsumptionType != null;
    public float OnConsumptionNutrition { get; init; } = 0f;
    public float OnConsumptionHydration { get; init; } = 0f;


    // Medical
    public float SeverityReduction { get; init; } = 0f;
    public bool CanReduceSeverity => SeverityReduction > 0f;
    public bool CanTendWounds { get; init; } = false;
    public bool CanHealInfections { get; init; } = false;
    public bool CanHealPoisoning { get; init; } = false;

    public ItemDef(string defName) : base(defName) { }

    public bool HasTag(ItemTagDef tag)
    {
        return Tags.Contains(tag);
    }
}
