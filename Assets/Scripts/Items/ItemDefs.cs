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

        new ItemDef("Bedroll")
        {
            Label = "bedroll",
            Description = "Can be set up at your camp in the evening to provide a place to sleep, increasing healing during the night.",
            Value = 5,
            IsCampComponent = true,
            MinInitialDurability = 2,
            MaxInitialDurability = 8,
        },

        new ItemDef("Beer")
        {
            Label = "beer",
            Description = "A refreshing alcoholic beverage.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Drink,
                Nutrition = 0.5f,
                Hydration = 1.5f,
                AppliedHealthCondition = HealthConditionDefOf.Intoxication,
                AppliedHealthConditionSeverity = 2f,
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

        new ItemDef("Charcoal")
        {
            Label = "charcoal",
            Description = "The remains of burned wood, useful as fuel or for starting fires.",
            Value = 1,
            MaxInitialDurability = 2,
            Tags =
            {
                { ItemTagDefOf.Fuel, 5 },
            },
        },

        new ItemDef("Chocolate")
        {
            Label = "chocolate",
            Description = "A sweet treat that can boost morale.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 1f,
                AppliedHealthCondition = HealthConditionDefOf.ChocolateHigh,
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
                { ItemTagDefOf.Lockpicking, 1 },
                { ItemTagDefOf.PryingTool, 1 },
            },
        },

        new ItemDef("Lighter")
        {
            Label = "lighter",
            Description = "A small device that can be used to start fires.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.FireStarter, 5 },
                { ItemTagDefOf.LightSource, 2 },
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

        new ItemDef("Matchbox")
        {
            Label = "matchbox",
            Description = "A small carton box containing a few matches and a surface to strike them on.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.FireStarter, 4 },
                { ItemTagDefOf.LightSource, 1 },
                { ItemTagDefOf.Lockpicking, 1 },
            },
        },

        new ItemDef("MeatCooked")
        {
            Label = "cooked meat",
            Description = "A delicious, safe and very nutritious meal.",
            Value = 4,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 5f
            },
        },
        new ItemDef("MeatRaw")
        {
            Label = "raw meat",
            Description = "Fresh meat. Very nutritious, but eating it raw might not be the best idea.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 2.5f,
                AppliedHealthCondition = HealthConditionDefOf.Poisoning,
                AppliedHealthConditionChance = 0.5f,
            },
            CookResult = ItemDefOf.MeatCooked,
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

        new ItemDef("NimbleRoot")
        {
            Label = "nimble root",
            Description = "Chewing this numbs your fingertips into perfect stillness.",
            Value = 1,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Drug,
                Nutrition = 0.5f,
                AppliedHealthCondition = HealthConditionDefOf.SteadyHands,
            },
        },

        new ItemDef("NutSnack")
        {
            Label = "nut snack",
            Description = "Provides a good amount of nutrition.",
            Value = 1,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Food,
                Nutrition = 3f
            },
        },

        new ItemDef("OilLamp")
        {
            Label = "oil lamp",
            Description = "An old fashioned light source consisting out of a small container of flammable oil and a wick, and a mechanism to control the flame.",
            Value = 3,
            Tags =
            {
                { ItemTagDefOf.LightSource, 4 },
                { ItemTagDefOf.Fuel, 2 },
                { ItemTagDefOf.FireStarter, 1 },
            },
        },

        new ItemDef("Postcard")
        {
            Label = "postcard",
            Description = "A postcard from the area and a reminder of a better time and place.",
            Value = 1,
            PassiveStatChanges =
            {
                { StatDefOf.Morale, +1 }
            },
            Tags =
            {
                { ItemTagDefOf.FieldGuide, 1 },
            },
        },

        new ItemDef("ProteinShake")
        {
            Label = "protein shake",
            Description = "A drink that provides some nutrition and hydration, as well as increasing strength.",
            Value = 2,
            ConsumptionProperties = new ConsumptionProperties()
            {
                ConsumptionType = ConsumptionTypeDefOf.Drink,
                Nutrition = 1f,
                Hydration = 1f,
                StatChanges =
                {
                    { StatDefOf.Strength, +1 }
                }
            },
        },

        new ItemDef("Rope")
        {
            Label = "rope",
            Description = "Useful to tie things together or climbing.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.Climbing, 3 }
            },
        },

        new ItemDef("Screwdriver")
        {
            Label = "screwdriver",
            Description = "Useful for opening things big and small.",
            Value = 2,
            Tags =
            {
                { ItemTagDefOf.PryingTool, 3 },
                { ItemTagDefOf.Lockpicking, 2 },
                { ItemTagDefOf.Weapon, 1 },
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

        new ItemDef("SurvivalBook")
        {
            Label = "bush craft 101",
            Description = "A book that provides useful information about survival.",
            Value = 3,
            PassiveStatChanges =
            {
                { StatDefOf.Survival, +2 }
            },
            Tags =
            {
                { ItemTagDefOf.FieldGuide, 5 },
            },
        },

        new ItemDef("Tent")
        {
            Label = "packed tent",
            Description = "Can be set up at your camp in the evening to provide protection during the night.",
            Value = 5,
            MinInitialDurability = 2,
            MaxInitialDurability = 10,
            IsCampComponent = true,
        },

        new ItemDef("Trap")
        {
            Label = "trap",
            Description = "Can be placed in the evening to help with attacks during the night. May also catch something to provide resources.",
            Value = 3,
            MinInitialDurability = 1,
            MaxInitialDurability = 5,
            IsCampComponent = true,
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

        new ItemDef("Wood")
        {
            Label = "wood",
            Description = "Just an ordinary, dry piece of wood.",
            Value = 1,
            Tags =
            {
                { ItemTagDefOf.Fuel, 3 },
                { ItemTagDefOf.Weapon, 1 },
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
    public static ItemDef Bedroll;
    public static ItemDef Berries;
    public static ItemDef Bone;
    public static ItemDef Charcoal;
    public static ItemDef Coin;
    public static ItemDef Crowbar;
    public static ItemDef FenceCutter;
    public static ItemDef Knife;
    public static ItemDef Lighter;
    public static ItemDef Lockpick;
    public static ItemDef Matchbox;
    public static ItemDef MeatCooked;
    public static ItemDef MeatRaw;
    public static ItemDef MedicalKit;
    public static ItemDef MedicinalHerbs;
    public static ItemDef NutSnack;
    public static ItemDef OilLamp;
    public static ItemDef Shovel;
    public static ItemDef Rope;
    public static ItemDef Tent;
    public static ItemDef Trap;
    public static ItemDef WaterBottle;
    public static ItemDef Wood;
}