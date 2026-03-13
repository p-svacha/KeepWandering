using System.Collections.Generic;
using UnityEngine;

public class ItemDef : Def
{
    public override string DefTypeLabel => "Item";
    public override Sprite Sprite => Resources.Load<Sprite>("Items/" + DefName);

    /// <summary>
    /// If true, this item will not appear in random selections.
    /// </summary>
    public bool IsQuestItem { get; init; }

    // Tags
    public ItemTagCollection Tags { get; init; } = new ItemTagCollection();


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


    public override bool Validate()
    {
        foreach(var mod in Tags.Modifiers)
        {
            if (!Tags.Contains(mod.Key))
            {
                throw new System.Exception($"ItemDef {DefName} has a tag value modifier for {mod.Key.DefName} but does not have that tag.");
            }
        }

        return base.Validate();
    }
}
