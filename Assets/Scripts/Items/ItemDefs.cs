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
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.Medical, 3 }
            },
            CanHealInfections = true,
        },

        new ItemDef("Antidote")
        {
            Label = "antidote",
            Description = "Heals poisoning.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.Medical, 3 }
            },
            CanHealPoisoning = true,
        },

        new ItemDef("Bandage")
        {
            Label = "bandage",
            Description = "An effective way to tend all kinds of wounds.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Medical, 2 }
            },
            CanTendWounds = true,
        },

        new ItemDef("Beans")
        {
            Label = "can of beans",
            Description = "Provides a good amount of nutrition and a small amount of hydration.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Food, 3 }
            },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3f,
            OnConsumptionHydration = 1f,
        },

        new ItemDef("Berries")
        {
            Label = "berries",
            Description = "Provides a small amount of nutrition and hydration.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Food, 1 },
                { ItemTagDefOf.Plant, 1 }
            },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 1f,
            OnConsumptionHydration = 0.5f,
        },

        new ItemDef("Bone")
        {
            Label = "bone",
            Description = "A bone that can be used as a weapon or tool.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Weapon, 1 },
                { ItemTagDefOf.ForDogs, 4 },
                { ItemTagDefOf.Trash, 1 }
            },
        },

        new ItemDef("Coin")
        {
            Label = "coin",
            Description = "Commonly accepted as currency.",
            Value = 1,
        },

        new ItemDef("Crowbar")
        {
            Label = "crowbar",
            Description = "Very useful for opening things that were not meant to be opened.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.Tool, 3 },
                { ItemTagDefOf.Weapon, 2 },
                { ItemTagDefOf.Digging, 1 }
            },
        },

        new ItemDef("FenceCutter")
        {
            Label = "fence cutter",
            Description = "There's a specific type of fence this could be very useful for.",
            Value = 5,
            Tags =
            {
                { ItemTagDefOf.Tool, 3 },
                { ItemTagDefOf.Cutting, 3 },
                { ItemTagDefOf.Weapon, 1 },
            },
            IsQuestItem = true,
        },

        new ItemDef("Knife")
        {
            Label = "knife",
            Description = "Very useful multi-purpose tool.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Weapon, 2 },
                { ItemTagDefOf.Tool, 2 },
                { ItemTagDefOf.Cutting, 3 },
                { ItemTagDefOf.Scavenging, 1 },
                { ItemTagDefOf.Lockpicking, 1 },
            },
        },

        new ItemDef("Lockpick")
        {
            Label = "lockpick",
            Description = "Useful for opening locked containers.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.Lockpicking, 5 }
            },
        },

        new ItemDef("MedicalKit")
        {
            Label = "medical kit",
            Description = "Can be used to tend or heal a variety of medical issues.",
            Value = 4,
            Tags =
            {
                { ItemTagDefOf.Medical, 4 }
            },
            CanTendWounds = true,
            CanHealInfections = true,
            CanHealPoisoning = true,
        },

        new ItemDef("MedicinalHerbs")
        {
            Label = "medicinal herbs",
            Description = "Can be used to slightly lower the severity of many health conditions.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Medical, 2 },
                { ItemTagDefOf.Plant, 2 }
            },
            SeverityReduction = 1f,
        },

        new ItemDef("NutSnack") {
            Label = "nut snack",
            Description = "Provides a good amount of nutrition.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Food, 2 }
            },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 2.5f,
        },

        new ItemDef("RawMeat")
        {
            Label = "raw meat",
            Description = "Fresh meat. Very nutritious, but eating it raw might not be the best idea.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Food, 2 }
            },
            ConsumptionType = ConsumptionTypeDefOf.Eat,
            OnConsumptionNutrition = 3.5f,
        },

        new ItemDef("Rope")
        {
            Label = "rope",
            Description = "Useful to tie things together or climbing.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.BuildingMaterial, 3 }
            },
        },

        new ItemDef("Shovel")
        {
            Label = "shovel",
            Description = "Particularly useful for digging.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Tool, 2 },
                { ItemTagDefOf.Digging, 4 },
                { ItemTagDefOf.Weapon, 1 },
            },
        },

        new ItemDef("Trap")
        {
            Label = "trap",
            Description = "Can be placed in the evening to help with attacks during the night. May also catch something to provide resources.",
            Value = 3,
        },

        new ItemDef("WaterBottle")
        {
            Label = "bottle of water",
            Description = "Provides water for about 3 days.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Drink, 3 }
            },
            ConsumptionType = ConsumptionTypeDefOf.Drink,
            OnConsumptionHydration = 3f,
        },
    };
}