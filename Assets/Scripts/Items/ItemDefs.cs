using System.Collections.Generic;
using UnityEngine;

public static class ItemDefs
{
    public static List<ItemDef> Defs => new List<ItemDef>()
    {

        new ItemDef("Antibiotics")
        {
            Label = "antibiotics",
            Description = "Treats infections.",
            Tags = { ItemTagDefOf.Medical },
            CanHealInfections = true,
        },

        new ItemDef("Antidote")
        {
            Label = "antidote",
            Description = "Heals poisoning.",
            Tags = { ItemTagDefOf.Medical },
            CanHealPoisoning = true,
        },

        new ItemDef("Bandage")
        {
            Label = "bandage",
            Description = "An effective way to tend all kinds of wounds.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
        },

        new ItemDef("Beans")
        {
            Label = "can of beans",
            Description = "Provides a good amount of nutrition and a small amount of hydration.",
            Tags = { ItemTagDefOf.Food },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3f,
            OnConsumptionHydration = 1f,
        },

        new ItemDef("Berries")
        {
            Label = "berries",
            Description = "Provides a small amount of nutrition and hydration.",
            Tags = { ItemTagDefOf.Food, ItemTagDefOf.Plant },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 1f,
            OnConsumptionHydration = 0.5f,
        },

        new ItemDef("Bone")
        {
            Label = "bone",
            Description = "A bone that can be used as a weapon or tool.",
            Tags = { ItemTagDefOf.Weapon, ItemTagDefOf.ForDogs, ItemTagDefOf.Trash },
        },

        new ItemDef("Coin")
        {
            Label = "coin",
            Description = "Commonly accepted as currency.",
        },

        new ItemDef("Crowbar")
        {
            Label = "crowbar",
            Description = "Very useful for opening things that were not meant to be opened.",
            Tags = { ItemTagDefOf.Tool, ItemTagDefOf.Weapon },
        },

        new ItemDef("FenceCutter")
        {
            Label = "fence cutter",
            Description = "There's a specific type of fence this could be very useful for.",
            Tags = new List<ItemTagDef>() { ItemTagDefOf.Tool },
            IsQuestItem = true,
        },

        new ItemDef("Knife")
        {
            Label = "knife",
            Description = "Both useful as a weapon and as a tool.",
            Tags = { ItemTagDefOf.Weapon, ItemTagDefOf.Tool },
        },

        new ItemDef("Lockpick")
        {
            Label = "lockpick",
            Description = "Useful for opening locked containers.",
            Tags = { ItemTagDefOf.Lockpicking },
        },

        new ItemDef("MedicalKit")
        {
            Label = "medical kit",
            Description = "Can be used to tend or heal a variety of medical issues.",
            Tags = { ItemTagDefOf.Medical },
            CanTendWounds = true,
            CanHealInfections = true,
            CanHealPoisoning = true,
        },

        new ItemDef("MedicinalHerbs")
        {
            Label = "medicinal herbs",
            Description = "Can be used to slightly lower the severity of many health conditions.",
            Tags = { ItemTagDefOf.Medical, ItemTagDefOf.Plant },
            SeverityReduction = 1f,
        },

        new ItemDef("NutSnack") {
            Label = "nut snack",
            Description = "Provides a good amount of nutrition.",
            Tags = { ItemTagDefOf.Food },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 2.5f,
        },

        new ItemDef("RawMeat")
        {
            Label = "raw meat",
            Description = "Fresh meat. Very nutritious, but eating it raw might not be the best idea.",
            Tags = { ItemTagDefOf.Food },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3.5f,
        },

        new ItemDef("Rope")
        {
            Label = "rope",
            Description = "Useful to tie things together or climbing.",
            Tags = { ItemTagDefOf.BuildingMaterial },
        },

        new ItemDef("Shovel")
        {
            Label = "shovel",
            Description = "Useful for digging.",
            Tags = { ItemTagDefOf.Digging },
        },

        new ItemDef("Trap")
        {
            Label = "trap",
            Description = "Can be placed in the evening to help with attacks during the night. May also catch something to provide resources.",
        },

        new ItemDef("WaterBottle")
        {
            Label = "bottle of water",
            Description = "Provides water for about 3 days.",
            Tags = { ItemTagDefOf.Drink },
            ConsumptionType = ConsumptionTypeDefOf.Drink,
            OnConsumptionHydration = 3f,
        },
    };
}