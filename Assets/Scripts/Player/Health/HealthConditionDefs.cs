using System.Collections.Generic;
using static HealthConditionDef;

public static class HealthConditionDefs
{

    public static List<HealthConditionDef> Defs => new List<HealthConditionDef>()
    {

        #region Vitals

        new HealthConditionDef("Hunger")
        {
            Label = "Hunger",
            Interactions = "Eat food to reduce hunger.",
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

        #endregion

        #region Fractures

        new HealthConditionDef("LegFracture")
        {
            Label = "Leg Fracture",
            BaseSpriteName = "Fracture",
            Interactions = $"{HEALS_NATURALLY}",
            HealthConditionClass = typeof(HC_Fracture),
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
            BaseSpriteName = "Fracture",
            Interactions = $"{HEALS_NATURALLY}",
            HealthConditionClass = typeof(HC_Fracture),
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

        #endregion

        #region Wounds

        new HealthConditionDef("Cut")
        {
            Label = "cut",
            HealthConditionClass = typeof(HC_CutWound),
            Category = HealthConditionCategoryDefOf.Negative,
            IsWound = true,
            // Everything else handled by Wound class
        },

        new HealthConditionDef("Bruise")
        {
            HealthConditionClass = typeof(HC_BruiseWound),
            Category = HealthConditionCategoryDefOf.Negative,
            Label = "bruise",
            IsWound = true,
            // Everything else handled by Wound class
        },

        new HealthConditionDef("Burn")
        {
            HealthConditionClass = typeof(HC_BurnWound),
            Category = HealthConditionCategoryDefOf.Negative,
            Label = "burn",
            IsWound = true,
            // Everything else handled by Wound class
        },

        #endregion

        #region Misc. Ailments

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
                    Label = "Buzzed",
                    Description = "I've been electrically shocked, this feels weird.",
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
                    Description = "My whole body is tingling from the electric shock.",
                    SeverityThreshold = 4,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -5 },
                    },
                    _AppliedHealthConditions =
                    {
                        ("HeartArrhythmia", 0.2f),
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Electrocuted",
                    Description = "I can't move my body properly from the electric shock, everything feels numb and twitchy.",
                    SeverityThreshold = 7,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Dexterity, -8 },
                    },
                    _AppliedHealthConditions =
                    {
                        ("HeartArrhythmia", 0.4f),
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },

        new HealthConditionDef("Poisoning")
        {
            Label = "Poisoning",
            Interactions = $"Can be reduced with poison treatment items, with higher star items having a greater effect.",
            Category = HealthConditionCategoryDefOf.Negative,
            NaturalHealing = 0.2f,
            NaturalSeverityChange = 0.4f,
            MaxInstances = 1,
            MaxSeverity = 12,
            DefaultInitialSeverity = 2,
            IsLethal = true,
            LethalityMessage = "The poison killed you.",
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Queasy",
                    Description = "I feel a bit sick.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Survival, -1 },
                    },
                    Color = ResourceManager.Color_Text_Negative,
                },
                new HealthConditionStage()
                {
                    Label = "Nauseous",
                    Description = "I'm feeling sick and can't keep anything down.",
                    SeverityThreshold = 4,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -4 },
                        { StatDefOf.Survival, -2 },
                        { StatDefOf.Dexterity, -2 },
                    },
                    _EndOfDayVitalChanges = new Dictionary<string, float>()
                    {
                        { "Hunger", +1 },
                        { "Thirst", +1 },
                    },
                    Color = ResourceManager.Color_Text_VeryNegative,
                },
                new HealthConditionStage()
                {
                    Label = "Critically Poisoned",
                    Description = "If I can't find an antidote, this poison will kill me!",
                    SeverityThreshold = 8,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -6 },
                        { StatDefOf.Survival, -4 },
                        { StatDefOf.Dexterity, -3 },
                        { StatDefOf.Social, -2 },
                    },
                    _EndOfDayVitalChanges = new Dictionary<string, float>()
                    {
                        { "Hunger", +1 },
                        { "Thirst", +1 },
                    },
                    SkillCheckModifier = (-15, 0.4f),
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },

        #endregion

        #region Buffs

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
        },

        new HealthConditionDef("WellRested")
        {
            Label = "Well Rested",
            Category = HealthConditionCategoryDefOf.Positive,
            HealthConditionClass = typeof(HC_WellRested),
            NaturalSeverityChange = -1,
            DefaultInitialSeverity = 1,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Well Rested",
                    Description = "I feel well rested from having a nice camp last night.",
                    SeverityThreshold = 0,
                    Color = ResourceManager.Color_Text_Positive,
                }
            }
        },

        new HealthConditionDef("FullOfBeans")
        {
            Label = "Full of Beans",
            Category = HealthConditionCategoryDefOf.Positive,
            NaturalSeverityChange = -1,
            DefaultInitialSeverity = 2,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Full of Beans",
                    Description = "I feel full of energy and enthusiasm.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Survival, +2 },
                        { StatDefOf.Strength, +2 },
                        { StatDefOf.Dexterity, +2 },
                        { StatDefOf.Social, +2 },
                    },
                    Color = ResourceManager.Color_Text_Positive,
                }
            }
        },

        #endregion

        #region Misc / Neutral

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

    #endregion

    };
}

[DefOf]
public static class HealthConditionDefOf
{
    // Vitals
    public static HealthConditionDef Hunger;
    public static HealthConditionDef Thirst;
    public static HealthConditionDef BloodLoss;

    // Fractures
    public static HealthConditionDef LegFracture;
    public static HealthConditionDef ArmFracture;

    // Wounds
    public static HealthConditionDef Bruise;
    public static HealthConditionDef Cut;
    public static HealthConditionDef Burn;

    // Misc Negative
    public static HealthConditionDef HeartArrhythmia;
    public static HealthConditionDef Electrocution;
    public static HealthConditionDef Poisoning;

    // Positive
    public static HealthConditionDef ChocolateHigh;
    public static HealthConditionDef FullOfBeans;
    public static HealthConditionDef SteadyHands;
    public static HealthConditionDef WellRested;

    // Neutral
    public static HealthConditionDef Intoxication;
}
