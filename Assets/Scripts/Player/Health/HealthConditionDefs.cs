using System.Collections.Generic;
using static HealthConditionDef;

public static class HealthConditionDefs
{
    public static List<HealthConditionDef> Defs => new List<HealthConditionDef>()
    {

        // Vitals
        new HealthConditionDef("Hunger")
        {
            Label = "Hunger",
            Interactions = "Eat food to reduce hunger.",
            HealthConditionClass = typeof(HC_Hunger),
            Category = HealthConditionCategoryDefOf.Vital,
            DefaultInitialSeverity = 5,
            MaxSeverity = 17,
            NaturalSeverityChange = 1,
            IsLethal = true,
            LethalityMessage = "You starved.",
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Well Fed",
                    Description = "I feel great! I have plenty of food in my stomach.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, +5 },
                        { StatDefOf.Strength, +3 },
                    },
                    Color = ResourceManager.Color_Text_Positive,
                },
                new HealthConditionStage()
                {
                    Label = "",
                    SeverityThreshold = 3,
                    IsVisible = false,
                },
                new HealthConditionStage()
                {
                    Label = "Hungry",
                    Description = "Some food would be nice.",
                    SeverityThreshold = 8,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, -2 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Very hungry",
                    Description = "I don't think I can go much longer without food.",
                    SeverityThreshold = 11,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, -5 },
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Dexterity, -2 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Starving",
                    Description = "I need to eat something immediately!",
                    SeverityThreshold = 14,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, -10 },
                        { StatDefOf.Strength, -5 },
                        { StatDefOf.Dexterity, -3 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            },
        },

        new HealthConditionDef("Thirst")
        {
            Label = "Thirst",
            Interactions = "Drink water to reduce thirst.",
            HealthConditionClass = typeof(HC_Thirst),
            Category = HealthConditionCategoryDefOf.Vital,
            DefaultInitialSeverity = 5,
            MaxSeverity = 13,
            NaturalSeverityChange = 1,
            IsLethal = true,
            LethalityMessage = "You died of dehydration.",
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Hydrated",
                    Description = "I feel refreshed.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, +3 },
                    },
                    Color = ResourceManager.Color_Text_Positive,
                },
                new HealthConditionStage()
                {
                    Label = "",
                    SeverityThreshold = 2,
                    IsVisible = false,
                },
                new HealthConditionStage()
                {
                    Label = "Thirsty",
                    Description = "I could use a drink.",
                    SeverityThreshold = 7,
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Very Thirsty",
                    Description = "I don't think I can go much longer without water.",
                    SeverityThreshold = 9,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -2 },
                        { StatDefOf.Survival, -2 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Dehydrated",
                    Description = "I need to drink something immediately!",
                    SeverityThreshold = 11,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -5 },
                        { StatDefOf.Survival, -5 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            }
        },

        new HealthConditionDef("BloodLoss")
        {
            Label = "Blood Loss",
            Interactions = $"{HEALS_NATURALLY}\nIncreased by bleeding (unbandaged) cut wounds.",
            HealthConditionClass = typeof(HC_BloodLoss),
            Category = HealthConditionCategoryDefOf.Vital,
            DefaultInitialSeverity = 0,
            MaxSeverity = 10,
            NaturalHealing = 0.5f,
            IsLethal = true,
            LethalityMessage = "You bled out.",
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage() // invisible at first
                {
                    SeverityThreshold = 0,
                    IsVisible = false,
                },
                new HealthConditionStage()
                {
                    Label = "Light Blood Loss",
                    Description = "I've been bleeding a little.",
                    SeverityThreshold = 2,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Dexterity, -2 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Heavy Blood Loss",
                    Description = "I've lost a lot of blood!",
                    SeverityThreshold = 5,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -5 },
                        { StatDefOf.Dexterity, -5 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Critical Blood Loss",
                    Description = "I'm losing blood very fast! I need to stop the bleeding immediately!",
                    SeverityThreshold = 8,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -8 },
                        { StatDefOf.Dexterity, -8 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            }
        },

        // Fractures
        new HealthConditionDef("LegFracture")
        {
            Label = "Leg Fracture",
            Interactions = $"{HEALS_NATURALLY}",
            HealthConditionClass = typeof(HC_LegFracture),
            Category = HealthConditionCategoryDefOf.Negative,
            MaxInstances = 2,
            MaxSeverity = 10,
            NaturalHealing = 0.5f,
            IsLethal = false,
            IsFracture = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Sprained Leg",
                    Description = "My leg is sprained. I can still walk, but it's painful.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -3 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Cracked Leg",
                    Description = "My leg is cracked.",
                    SeverityThreshold = 4,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -5 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Broken Leg",
                    Description = "My leg is broken. I can't walk at all today.",
                    SeverityThreshold = 8,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -10 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },

        new HealthConditionDef("ArmFracture")
        {
            Label = "Arm Fracture",
            Interactions = $"{HEALS_NATURALLY}",
            HealthConditionClass = typeof(HC_ArmFracture),
            Category = HealthConditionCategoryDefOf.Negative,
            MaxInstances = 2,
            MaxSeverity = 10,
            NaturalHealing = 0.5f,
            IsLethal = false,
            IsFracture = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Sprained Arm",
                    Description = "My arm is sprained. It hurts moving it.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Dexterity, -2 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Cracked Arm",
                    Description = "Oof ouch, my arms.",
                    SeverityThreshold = 4,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -4 },
                        { StatDefOf.Strength, -4 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Broken Arm",
                    Description = "My arm is broken.",
                    SeverityThreshold = 8,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -6 },
                        { StatDefOf.Strength, -6 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },


        // Wounds
        new HealthConditionDef("Cut")
        {
            Label = "cut",
            HealthConditionClass = typeof(HC_CutWound),
            Category = HealthConditionCategoryDefOf.Negative,
            MaxInstances = 5,
            IsWound = true,
            // Everything else handled by Wound class
        },

        new HealthConditionDef("Bruise")
        {
            HealthConditionClass = typeof(HC_BruiseWound),
            Category = HealthConditionCategoryDefOf.Negative,
            Label = "bruise",
            MaxInstances = 5,
            IsWound = true,
            // Everything else handled by Wound class
        },

        // Other injuries / ailments
        new HealthConditionDef("HeartArrhythmia")
        {
            Label = "Heart Arrhythmia",
            Interactions = $"Will not go away naturally. Needs to be treated at a pharmacy.",
            Category = HealthConditionCategoryDefOf.Negative,
            MaxInstances = 1,
            MaxSeverity = 1,
            DefaultInitialSeverity = 1,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Heart arrhythmia",
                    Description = "My heartbeat is irregular.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -1 },
                    },
                    SkillCheckModifier = (-10, 0.2f),
                    Color = ResourceManager.Color_Text_Negative,
                },
            }
        },

        new HealthConditionDef("Electrocution")
        {
            Label = "Electrocution",
            Interactions = $"{HEALS_NATURALLY}",
            Category = HealthConditionCategoryDefOf.Negative,
            MaxInstances = 1,
            MaxSeverity = 10,
            NaturalHealing = 1f,
            IsLethal = true,
            LethalityMessage = "You were electrocuted.",
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Sunned",
                    Description = "I've been electrocuted!",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -3 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Shocked",
                    Description = "I'm severely electrocuted!",
                    SeverityThreshold = 4,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -5 },
                    },
                    AppliedHealthConditions =
                    {
                        (HealthConditionDefOf.HeartArrhythmia, 0.2f),
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Electrocuted",
                    Description = "I'm critically electrocuted!",
                    SeverityThreshold = 7,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -8 },
                    },
                    AppliedHealthConditions =
                    {
                        (HealthConditionDefOf.HeartArrhythmia, 0.4f),
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },

        // Misc
        new HealthConditionDef("ChocolateHigh")
        {
            Label = "Chocolate High",
            Category = HealthConditionCategoryDefOf.Positive,
            MaxInstances = 1,
            DefaultInitialSeverity = 2,
            NaturalSeverityChange = -1,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Chocolate High",
                    Description = "That chocolate was amazing!",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Morale, +3 },
                    },
                    Color = ResourceManager.Color_Text_Positive,
                }
            }
        },

        new HealthConditionDef("Intoxication")
        {
            Category = HealthConditionCategoryDefOf.Neutral,
            Label = "Intoxication",
            NaturalHealing = 1,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Tipsy",
                    Description = "I feel a bit tipsy.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Social, +2 },
                        { StatDefOf.Dexterity, -2 },
                        { StatDefOf.Morale, +1 }
                    },
                    Color = ResourceManager.Color_Text_Default,
                },
                new HealthConditionStage()
                {
                    Label = "Drunk",
                    Description = "I'm drunk.",
                    SeverityThreshold = 3,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Social, +1 },
                        { StatDefOf.Dexterity, -4 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                }
            }
        },

        new HealthConditionDef("SteadyHands")
        {
            Label = "Steady Hands",
            Category = HealthConditionCategoryDefOf.Positive,
            NaturalSeverityChange = -1,
            DefaultInitialSeverity = 2,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Steady Hands",
                    Description = "My hands are steady.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, +2 },
                    },
                    Color = ResourceManager.Color_Text_Positive,
                }
            }
        }
    };
}

[DefOf]
public static class HealthConditionDefOf
{
    public static HealthConditionDef Hunger;
    public static HealthConditionDef Thirst;
    public static HealthConditionDef LegFracture;
    public static HealthConditionDef ArmFracture;
    public static HealthConditionDef BloodLoss;

    public static HealthConditionDef Bruise;
    public static HealthConditionDef Cut;

    public static HealthConditionDef HeartArrhythmia;
    public static HealthConditionDef Electrocution;

    public static HealthConditionDef ChocolateHigh;
    public static HealthConditionDef Intoxication;
    public static HealthConditionDef SteadyHands;
}
