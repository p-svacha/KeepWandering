using System.Collections.Generic;
using UnityEngine;

public class ItemDef : Def
{
    public override string DefTypeLabel => "Item";
    public override Sprite Sprite => Resources.Load<Sprite>("Items/" + DefName);

    public static int MIN_TAG_LEVEL = 1;
    public static int MAX_TAG_LEVEL = 5;

    /// <summary>
    /// If true, this item will not appear in random selections.
    /// </summary>
    public bool IsQuestItem { get; init; }

    // General
    public Dictionary<ItemTagDef, int> Tags = new Dictionary<ItemTagDef, int>(); // Each tag has a level that defines the difficulty reduction when used in a slot requiring that tag.
    public int Value { get; init; } = 0;


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
        return Tags.ContainsKey(tag);
    }


    public override bool Validate()
    {
        foreach (var tag in Tags)
        {
            if (tag.Value < MIN_TAG_LEVEL || tag.Value > MAX_TAG_LEVEL)
            {
                Debug.LogError($"ItemDef '{DefName}' has tag '{tag.Key.DefName}' with invalid level {tag.Value}. Level must be between {MIN_TAG_LEVEL} and {MAX_TAG_LEVEL}.");
                return false;
            }
        }

        return base.Validate();
    }
}
