using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDef : Def
{
    public override string DefTypeLabel => "Item";
    public override Sprite Sprite => Resources.Load<Sprite>("Items/" + DefName);

    // Tags
    public List<ItemTagDef> Tags { get; init; } = new List<ItemTagDef>();

    // Food
    public bool IsEdible { get; init; } = false;
    public float OnEatNutrition { get; init; } = 0f;
    public float OnEatHydration { get; init; } = 0f;

    // Drink
    public bool IsDrinkable { get; init; } = false;
    public float OnDrinkHydration { get; init; } = 0f;

    // Medical
    public bool CanTendWounds { get; init; } = false;
    public bool CanHealInfections { get; init; } = false;
    public bool CanHealPoisoning { get; init; } = false;

    public bool HasTag(ItemTagDef tag)
    {
        return Tags.Contains(tag);
    }
}
