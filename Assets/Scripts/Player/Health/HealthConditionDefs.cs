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
                    Description = "Some food would be nice."
                },
                new HealthConditionStage()
                {
                    Label = "very hungry",
                    Description = "I don't think I can go much longer without food."
                },
                new HealthConditionStage()
                {
                    Label = "starving",
                    Description = "I need to eat something immediately!"
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
                    Description = "I don't think I can go much longer without water."
                },
                new HealthConditionStage()
                {
                    Label = "dehydrated",
                    Description = "I need to drink something immediately!"
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
                    Label = "leg sprain",
                    Description = "My leg is sprained. I can still walk, but it's painful."
                },
                new HealthConditionStage()
                {
                    Label = "leg fracture",
                    Description = "My leg is fractured. My agility is severly reduced."
                },
                new HealthConditionStage()
                {
                    Label = "broken leg",
                    Description = "My leg is broken. I can't walk at all today."
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
                    Label = "arm sprain",
                    Description = "My arm is sprained. It hurts moving it."
                },
                new HealthConditionStage()
                {
                    Label = "arm fracture",
                    Description = "Oof ouch, my arms."
                },
                new HealthConditionStage()
                {
                    Label = "broken arm",
                    Description = "My arm is broken."
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
                },
                new HealthConditionStage()
                {
                    Label = "critical blood loss",
                    Description = "I'm losing blood very fast! I need to stop the bleeding immediately!",
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
            DefName = "CutWound",
            HealthConditionClass = typeof(HC_CutWound),
            IsPermanent = false,
        },
         new HealthConditionDef()
        {
            DefName = "BruiseWound",
            HealthConditionClass = typeof(HC_BruiseWound),
            IsPermanent = false,
         },
    };
}
