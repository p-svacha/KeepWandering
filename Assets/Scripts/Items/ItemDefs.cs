using System.Collections.Generic;
using UnityEngine;

public static class ItemDefs
{
    public static List<ItemDef> Defs => new List<ItemDef>()
    {

        new ItemDef("Antibiotics")
        {
            Label = "antibiotics",
            Description = "Pills that are very effective at treating infections.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.InfectionTreatment, 5 }
            },
        },

        new ItemDef("Antidote")
        {
            Label = "antidote",
            Description = "An injectable solution that counteracts poisoning.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.PoisonTreatment, 5 }
            },
        },

        new ItemDef("Bandage")
        {
            Label = "bandage",
            Description = "An effective way to tend all kinds of wounds.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.WoundBandaging, 5 }
            },
        },

        new ItemDef("Beans")
        {
            Label = "can of beans",
            Description = "Provides a good amount of nutrition and a small amount of hydration.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties() {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 2.5f,
                Hydration = 1f
            },
        },

        new ItemDef("Berries")
        {
            Label = "berries",
            Description = "Provides a small amount of nutrition and hydration.",
            Value = 1,
            ConsumptionProperties = new ConsumptionProperties() {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 1f,
                Hydration = 1f
            },
        },

        new ItemDef("Bone")
        {
            Label = "bone",
            Description = "A bone that can be used as a weapon or tool.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Weapon, 1 },
                { ItemTagDefOf.DogToy, 4 },
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
                { ItemTagDefOf.PryingTool, 4 },
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
                { ItemTagDefOf.Cutting, 3 },
                { ItemTagDefOf.Scavenging, 1 },
                { ItemTagDefOf.Lockpicking, 1 },
                { ItemTagDefOf.PryingTool, 1 },
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
            MaxInitialDurability = 2,
            Tags =
            {
                { ItemTagDefOf.WoundBandaging, 4 },
                { ItemTagDefOf.InfectionTreatment, 4 },
                { ItemTagDefOf.PoisonTreatment, 4 }
            },
        },

        new ItemDef("MedicinalHerbs")
        {
            Label = "medicinal herbs",
            Description = "A natural remedy that may help with ailments, but is not very effective.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.InfectionTreatment, 1 },
                { ItemTagDefOf.PoisonTreatment, 1 }
            },
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Drug,
                Nutrition = 0.2f,
                Hydration = 0.2f,
                SeverityReduction = 1f
            },
        },

        new ItemDef("NutSnack") {
            Label = "nut snack",
            Description = "Provides a good amount of nutrition.",
            Value = 1,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 3f
            },
        },

        new ItemDef("RawMeat")
        {
            Label = "raw meat",
            Description = "Fresh meat. Very nutritious, but eating it raw might not be the best idea.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 3.5f
            },
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
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Drink,
                Hydration = 3f
            },
        },
    };
}

[DefOf]
public static class ItemDefOf
{
    public static ItemDef Antibiotics;
    public static ItemDef Antidote;
    public static ItemDef Bandage;
    public static ItemDef Beans;
    public static ItemDef Berries;
    public static ItemDef Bone;
    public static ItemDef Coin;
    public static ItemDef Crowbar;
    public static ItemDef FenceCutter;
    public static ItemDef Knife;
    public static ItemDef Lockpick;
    public static ItemDef MedicalKit;
    public static ItemDef MedicinalHerbs;
    public static ItemDef NutSnack;
    public static ItemDef RawMeat;
    public static ItemDef Shovel;
    public static ItemDef Rope;
    public static ItemDef Trap;
    public static ItemDef WaterBottle;

}