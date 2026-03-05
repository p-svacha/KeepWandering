using System.Collections.Generic;
using UnityEngine;

public static class HealthConditionDefs
{
    public static List<HealthConditionDef> Defs => new List<HealthConditionDef>()
    {
        new HealthConditionDef()
        {
            DefName = "Hunger",
            HealthConditionClass = typeof(HC_Hunger),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "hungry",
                    Description = "Some food would be nice.",
                },
                new HealthConditionStage()
                {
                    Label = "very hungry",
                    Description = "I don't think I can go much longer without food.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Morale, -2 },
                        { StatDefOf.Intelligence, -2 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "starving",
                    Description = "I need to eat something immediately!",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Strength, -5 },
                        { StatDefOf.Morale, -5 },
                        { StatDefOf.Intelligence, -5 },
                    },
                },
            },
        },

        new HealthConditionDef()
        {
            DefName = "Thirst",
            HealthConditionClass = typeof(HC_Thirst),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "thirsty",
                    Description = "I could use a drink."
                },
                new HealthConditionStage()
                {
                    Label = "very thirsty",
                    Description = "I don't think I can go much longer without water.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Agility, -2 },
                        { StatDefOf.Perception, -2 },
                        { StatDefOf.Dexterity, -2 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "dehydrated",
                    Description = "I need to drink something immediately!",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Agility, -5 },
                        { StatDefOf.Perception, -5 },
                        { StatDefOf.Dexterity, -5 },
                    },
                },
            }
        },

        new HealthConditionDef()
        {
            DefName = "LegFracture",
            HealthConditionClass = typeof(HC_LegFracture),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "sprained leg",
                    Description = "My leg is sprained. I can still walk, but it's painful.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -1 },
                        { StatDefOf.Agility, -2 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "cracked leg",
                    Description = "My leg is cracked.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -2 },
                        { StatDefOf.Agility, -4 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "broken leg",
                    Description = "My leg is broken. I can't walk at all today.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -3 },
                        { StatDefOf.Agility, -6 },
                    },
                }
            }
        },

        new HealthConditionDef()
        {
            DefName = "ArmFracture",
            HealthConditionClass = typeof(HC_ArmFracture),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "sprained arm",
                    Description = "My arm is sprained. It hurts moving it.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -1 },
                        { StatDefOf.Dexterity, -1 },
                        { StatDefOf.Strength, -1 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "cracked arm",
                    Description = "Oof ouch, my arms.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -3 },
                        { StatDefOf.Dexterity, -3 },
                        { StatDefOf.Strength, -3 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "broken arm",
                    Description = "My arm is broken.",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -5 },
                        { StatDefOf.Dexterity, -5 },
                        { StatDefOf.Strength, -5 },
                    },
                }
            }
        },

        new HealthConditionDef()
        {
            DefName = "BloodLoss",
            HealthConditionClass = typeof(HC_BloodLoss),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "light blood loss",
                    Description = "I've been bleeding a little.",
                },
                new HealthConditionStage()
                {
                    Label = "heavy blood loss",
                    Description = "I've lost a lot of blood!",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -2 },
                        { StatDefOf.Strength, -2 },
                        { StatDefOf.Agility, -2 },
                    },
                },
                new HealthConditionStage()
                {
                    Label = "critical blood loss",
                    Description = "I'm losing blood very fast! I need to stop the bleeding immediately!",
                    StatModifiers = new Dictionary<StatDef, int>()
                    {
                        { StatDefOf.Combat, -5 },
                        { StatDefOf.Strength, -5 },
                        { StatDefOf.Agility, -5 },
                    },
                },
            }
        },

        new HealthConditionDef()
        {
            DefName = "Poison",
            HealthConditionClass = typeof(HC_Poison),
            IsPermanent = true,
            Stages = new List<HealthConditionStage>()
            {
                new HealthConditionStage()
                {
                    Label = "poisoned (minor)",
                    Description = "I feel sick. I think I'm poisoned.",
                },
                new HealthConditionStage()
                {
                    Label = "poisoned (major)",
                    Description = "I feel really sick. The poison is getting worse!",
                },
                new HealthConditionStage()
                {
                    Label = "poisoned (critical)",
                    Description = "I feel awful. The poison is taking over my body! I need an antidote immediately!",
                },
            }
        },

        new HealthConditionDef()
        {
            DefName = "Cut",
            Label = "cut",
            HealthConditionClass = typeof(HC_CutWound),
            IsPermanent = false,
            MaxAmount = 5,
        },
         new HealthConditionDef()
        {
            DefName = "Bruise",
            Label = "bruise",
            HealthConditionClass = typeof(HC_BruiseWound),
            IsPermanent = false,
            MaxAmount = 5,
         },
    };
}
