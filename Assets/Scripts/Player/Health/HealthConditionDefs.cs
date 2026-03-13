using System.Collections.Generic;

public static class HealthConditionDefs
{
    public static List<HealthConditionDef> Defs => new List<HealthConditionDef>()
    {

        // Needs
        new HealthConditionDef("Hunger")
        {
            HealthConditionClass = typeof(HC_Hunger),
            IsNeed = true,
            InitialSeverity = 5,
            MaxSeverity = 17,
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
                        { StatDefOf.Intelligence, -2 },
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
                        { StatDefOf.Intelligence, -3 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            },
        },

        new HealthConditionDef("Thirst")
        {
            HealthConditionClass = typeof(HC_Thirst),
            IsNeed = true,
            InitialSeverity = 5,
            MaxSeverity = 13,
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
                        { StatDefOf.Agility, -2 },
                        { StatDefOf.Perception, -2 },
                        { StatDefOf.Dexterity, -2 },
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
                        { StatDefOf.Agility, -5 },
                        { StatDefOf.Perception, -5 },
                        { StatDefOf.Dexterity, -5 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            }
        },


        // Unique conditions
        new HealthConditionDef("BloodLoss")
        {
            HealthConditionClass = typeof(HC_BloodLoss),
            MaxInstances = 1,
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
                        { StatDefOf.Combat, -2 },
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Agility, -2 },
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
                        { StatDefOf.Combat, -5 },
                        { StatDefOf.Strength, -5 },
                        { StatDefOf.Agility, -5 },
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
                        { StatDefOf.Combat, -8 },
                        { StatDefOf.Strength, -8 },
                        { StatDefOf.Agility, -8 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                },
            }
        },

        // Fractures
        new HealthConditionDef("LegFracture")
        {
            HealthConditionClass = typeof(HC_LegFracture),
            MaxInstances = 2,
            MaxSeverity = 10,
            NaturalHealing = 0.5f,
            IsLethal = false,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Sprained Leg",
                    Description = "My leg is sprained. I can still walk, but it's painful.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -1 },
                        { StatDefOf.Agility, -3 },
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
                        { StatDefOf.Combat, -3 },
                        { StatDefOf.Agility, -5 },
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
                        { StatDefOf.Combat, -5 },
                        { StatDefOf.Agility, -10 },
                    },
                    Color = ResourceManager.Color_Text_ExtremelyNegative,
                }
            }
        },

        new HealthConditionDef("ArmFracture")
        {
            HealthConditionClass = typeof(HC_ArmFracture),
            MaxInstances = 2,
            MaxSeverity = 10,
            NaturalHealing = 0.5f,
            IsLethal = false,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "Sprained Arm",
                    Description = "My arm is sprained. It hurts moving it.",
                    SeverityThreshold = 0,
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -2 },
                        { StatDefOf.Dexterity, -2 },
                        { StatDefOf.Strength, -2 },
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
                        { StatDefOf.Combat, -4 },
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
                        { StatDefOf.Combat, -6 },
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
            Description = "A cut wound.",
            HealthConditionClass = typeof(HC_CutWound),
            MaxInstances = 5,
            IsWound = true,
            // Everything else handled by Wound class
        },

        new HealthConditionDef("Bruise")
        {
            Label = "bruise",
            Description = "A bruise wound.",
            HealthConditionClass = typeof(HC_BruiseWound),
            MaxInstances = 5,
            IsWound = true,
            // Everything else handled by Wound class
        },
    };
}
