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

    /// <summary>
    /// This indicates that the item is usable in the evening as a part of the camp.
    /// </summary>
    public bool IsCampComponent { get; init; }

    // General
    public Dictionary<ItemTagDef, int> Tags = new Dictionary<ItemTagDef, int>(); // Each tag has a level that defines the difficulty reduction when used in a slot requiring that tag.
    public int Value { get; init; } = 0;
    public int MinInitialDurability { get; init; } = 1;
    public int MaxInitialDurability { get; init; } = 5;

    /// <summary>
    /// Passive stat changes that occur when the item is in the player's inventory.
    /// </summary>
    public Dictionary<StatDef, int> PassiveStatChanges { get; init; } = new Dictionary<StatDef, int>();


    // Consumption
    public ConsumptionProperties ConsumptionProperties { get; init; } = null;
    public bool IsConsumable => ConsumptionProperties != null;
    public ItemDef CookResult { get; init; } = null; // If this item can be cooked, this is the result of cooking it. If null, the item cannot be cooked.
    public bool IsCookable => CookResult != null;


    // Medical
    public bool HasMedicalProperties => HasTag(ItemTagDefOf.WoundBandaging) || HasTag(ItemTagDefOf.InfectionTreatment) || (ConsumptionProperties != null && ConsumptionProperties.SeverityReduction > 0f);

    public ItemDef(string defName) : base(defName) { }

    public bool HasTag(ItemTagDef tag)
    {
        return Tags.ContainsKey(tag);
    }


    public override bool Validate()
    {
        if (Sprite == null) ThrowValidationError($"ItemDef '{DefName}' has no sprite assigned. Make sure there is a sprite in Resources/Items/{DefName}.png");

        foreach (var tag in Tags)
        {
            if (tag.Value < MIN_TAG_LEVEL || tag.Value > MAX_TAG_LEVEL)
            {
                ThrowValidationError($"ItemDef '{DefName}' has tag '{tag.Key.DefName}' with invalid level {tag.Value}. Level must be between {MIN_TAG_LEVEL} and {MAX_TAG_LEVEL}.");
            }
        }

        foreach(var statChange in PassiveStatChanges)
        {
            if (statChange.Value == 0) ThrowValidationError($"ItemDef '{DefName}' has a passive stat change for '{statChange.Key.DefName}' with a value of 0. Stat changes must be non-zero.");
        }

        if (ConsumptionProperties != null) ConsumptionProperties.Validate();

        return base.Validate();
    }
}
