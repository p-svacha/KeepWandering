using System.Collections.Generic;
using UnityEngine;

public static class ItemDefs
{
    public static List<ItemDef> Defs => new List<ItemDef>()
    {
        new ItemDef()
        {
            DefName = "Beans",
            Label = "can of beans",
            Description = "A can of beans that will provide a good amount of nutrition and a small amount of hydration.",
            Tags = { ItemTagDefOf.Food },
            IsEdible = true,
            OnEatNutrition = 3f,
            OnEatHydration = 1f,
        },

        new ItemDef()
        {
            DefName = "WaterBottle",
            Label = "bottle of water",
            Description = "A full water bottle providing water for about 3 days.",
            Tags = { ItemTagDefOf.Drink },
            IsDrinkable = true,
            OnDrinkHydration = 3f,
        },

        new ItemDef()
        {
            DefName = "Bandage",
            Label = "bandage",
            Description = "A simple bandage that can be used to tend wounds.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
        },

        new ItemDef()
        {
            DefName = "Antibiotics",
            Label = "antibiotics",
            Description = "A course of antibiotics that can be used to heal infections.",
            Tags = { ItemTagDefOf.Medical },
            CanHealInfections = true,
        },

        new ItemDef()
        {
            DefName = "Bone",
            Label = "bone",
            Description = "A bone that can be used as a weapon or tool.",
            Tags = { ItemTagDefOf.Weapon, ItemTagDefOf.ForDogs },
        },

        new ItemDef()
        {
            DefName = "Knife",
            Label = "knife",
            Description = "A sharp knife that can be used as a weapon or tool.",
            Tags = { ItemTagDefOf.Weapon },
        },

        new ItemDef() {
            DefName = "NutSnack",
            Label = "nut snack",
            Description = "A small bag of mixed nuts that will provide a small amount of nutrition and hydration.",
            Tags = { ItemTagDefOf.Food },
            IsEdible = true,
            OnEatNutrition = 1f,
            OnEatHydration = 0.5f,
        },

        new ItemDef()
        {
            DefName = "MedicalKit",
            Label = "medical kit",
            Description = "A medical kit that can be used to tend wounds and heal infections.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
            CanHealInfections = true,
        },

        new ItemDef()
        {
            DefName = "Antidote",
            Label = "antidote",
            Description = "An antidote that can be used to heal poisoning.",
            Tags = { ItemTagDefOf.Medical },
            CanHealPoisoning = true,
        },

        new ItemDef()
        {
            DefName = "Coin",
            Label = "coin",
            Description = "A coin often used as currency in trades.",
        }
    };
}