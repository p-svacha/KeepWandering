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
            Description = "Provides a good amount of nutrition and a small amount of hydration.",
            Tags = { ItemTagDefOf.Food },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3f,
            OnConsumptionHydration = 1f,
        },

        new ItemDef()
        {
            DefName = "WaterBottle",
            Label = "bottle of water",
            Description = "Provides water for about 3 days.",
            Tags = { ItemTagDefOf.Drink },
            ConsumptionType = ConsumptionTypeDefOf.Drink,
            OnConsumptionHydration = 3f,
        },

        new ItemDef()
        {
            DefName = "Bandage",
            Label = "bandage",
            Description = "An effective way to tend all kinds of wounds.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
        },

        new ItemDef()
        {
            DefName = "Antibiotics",
            Label = "antibiotics",
            Description = "Treats infections.",
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
            Description = "Both useful as a weapon and as a tool.",
            Tags = { ItemTagDefOf.Weapon },
        },

        new ItemDef() {
            DefName = "NutSnack",
            Label = "nut snack",
            Description = "Provides a good amount of nutrition.",
            Tags = { ItemTagDefOf.Food },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3f,
        },

        new ItemDef()
        {
            DefName = "MedicalKit",
            Label = "medical kit",
            Description = "Can be used to tend or heal a variety of medical issues.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
            CanHealInfections = true,
            CanHealPoisoning = true,
        },

        new ItemDef()
        {
            DefName = "Antidote",
            Label = "antidote",
            Description = "Heals poisoning.",
            Tags = { ItemTagDefOf.Medical },
            CanHealPoisoning = true,
        },

        new ItemDef()
        {
            DefName = "Coin",
            Label = "coin",
            Description = "Commonly accepted as currency.",
        },

        new ItemDef()
        {
            DefName = "Crowbar",
            Label = "crowbar",
            Description = "Very useful for opening things that were not meant to be opened.",
            Tags = { ItemTagDefOf.Tool },
        },

        new ItemDef()
        {
            DefName = "Rope",
            Label = "rope",
            Description = "Useful to tie things together or climbing.",
        },

        new ItemDef()
        {
            DefName = "FenceCutter",
            Label = "fence cutter",
            Description = "There's a specific type of fence this could be very useful for.",
            Tags = new List<ItemTagDef>() { ItemTagDefOf.Tool },
            IsQuestItem = true,
        }
    };
}