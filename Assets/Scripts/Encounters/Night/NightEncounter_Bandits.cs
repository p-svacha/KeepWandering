using System.Collections.Generic;
using UnityEngine;

public class NightEncounter_Bandits : NightEncounter
{
    // Intensity: 1 = lone bandit, 2 = pair, 3 = small group

    private bool IsPlayerCaughtHiding;

    protected override void OnInitialize() { }

    protected override string OnStart()
    {
        string text = "";

        if (Intensity == 1) text = "You wake to a sound. A figure stands over your cart, rummaging through your things.";
        if (Intensity == 2) text = "Voices wake you. Two figures are going through your cart. One seems to notice you're awake.";
        if (Intensity == 3) text = "You're kicked awake. Three people surround your camp.";

        if (Game.Camp.NumTrapsUsedToDefendNightAttack > 0) text += " It seems as your traps have weakened the attack.";

        return text;
    }

    protected override void RefreshSprites()
    {
        bool showBandits = !IsEncounterDone;

        SetObjectVisibility("Bandit1", showBandits && Intensity >= 1);
        SetObjectVisibility("Bandit2", showBandits && Intensity >= 2);
        SetObjectVisibility("Bandit3", showBandits && Intensity >= 3);
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (IsPlayerCaughtHiding)
        {
            options.Add(GetFightOption());
            options.Add(GetBegOption());
        }

        else
        {
            options.Add(GetFightOption());
            options.Add(GetSneakAwayOption());
            options.Add(GetIntimidateOption());

            if (Intensity <= 2) options.Add(GetHideOption());
        }

        return options;
    }

    #region Options

    private EncounterOption GetFightOption()
    {
        string description = Intensity switch
        {
            1 => "Take on the intruder.",
            2 => "Fight them both off. It won't be easy.",
            3 => "Fight your way out. You're outnumbered."
        };
        int difficulty = Intensity switch
        {
            1 => 40,
            2 => 65,
            3 => 90
        };

        return new SkillCheckOption()
        {
            Text = "Fight",
            Description = description,
            Action = Fight,
            Difficulty = difficulty,
            FixedDifficultyModifiers = new Dictionary<string, int>()
            {
                { "Caught hiding", IsPlayerCaughtHiding ? +15 : 0  },
            },
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Strength, 3 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Weapon,
                },
            }
        };
    }
    private string Fight(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        string bandits = "bandit".Pluralize(Intensity);
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            if (Intensity > 1) Game.ModifyStatBaseValue(StatDefOf.Strength, +1);
            LootTables.Bandit.AddItemToInventory();

            return $"You completely overwhelm the {bandits} and drive them off. They drop something in their panic.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            return $"You successfully drive off the {bandits}.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ApplyBruiseDamage(Intensity, source: "Fighting bandits");
            return $"You drive off the {bandits} but take some hits.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ApplyBruiseDamage(Intensity + 1, source: "Fighting bandits");
            Game.RemoveRandomItemFromInventory();

            string beat = Intensity == 1 ? "beats" : "beat";
            return $"You are overpowered. The {bandits} {beat} you and take some of your stuff.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyCutDamage(Random.Range(2f, 3f), source: "Fighting bandits");
            Game.ApplyBruiseDamage(Random.Range(2f, 3f), source: "Fighting bandits");

            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();

            return $"You get completely overpowered and badly beaten.";
        }

        throw new InvalidOutcomeException();
    }


    private EncounterOption GetSneakAwayOption()
    {
        int difficulty = Intensity switch
        {
            1 => 30,
            2 => 45,
            3 => 65
        };

        return new SkillCheckOption()
        {
            Text = "Sneak away",
            Description = "Try to slip past them in the dark to save your skin. They seem focused on the loot.",
            Action = SneakAway,
            Difficulty = difficulty,
            BiomeDifficultyModifiers = new Dictionary<BiomeDef, int>()
            {
                { BiomeDefOf.Woods, -15 },
                { BiomeDefOf.Outskirts, +15 },
            },
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Dexterity, 3 },
            },
        };
    }
    private string SneakAway(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyRandomStat(1, StatDefOf.Dexterity);

            return "You slip away like a shadow, somehow even taking your whole cart without them noticing.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();

            return "You slip away into the darkness. They don't notice you and just take a few items from your cart.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ApplyBruiseWound(source: "Sneaking away from bandits");
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();

            return "You almost make it, but step on something. They spot you and shove you down before you can get far.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            Game.ApplyBruiseDamage(2f, source: "Sneaking away from bandits");
            Game.ModifyMorale(-1);

            return $"They see you trying to run. One of them trips you.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            Game.ApplyBruiseDamage(3f, source: "Sneaking away from bandits");
            Game.ApplyCutWound(source: "Sneaking away from bandits");
            Game.ModifyMorale(-1);

            return "You stumble right into them. They don't take kindly to that.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetIntimidateOption()
    {
        string description = Intensity switch
        {
            1 => "Stand up and make it clear they picked the wrong person.",
            2 => "Try to convince them you're more trouble than you're worth.",
            3 => "They have the numbers. But maybe you can make them doubt it."
        };
        int difficulty = Intensity switch
        {
            1 => 35,
            2 => 55,
            3 => 80
        };

        return new SkillCheckOption()
        {
            Text = "Intimidate",
            Description = description,
            Action = Intimidate,
            Difficulty = difficulty,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 2 },
                { StatDefOf.Strength, 1 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Weapon,
                },
            },
            CanCriticallyFail = false,
        };
    }
    private string Intimidate(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyMorale(+1);
            LootTables.Bandit.AddItemToInventory();

            return "They back off immediately and drop something as they scramble away.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyMorale(+1);
            return "They exchange glances and back off. Not worth the risk.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.RemoveRandomItemFromInventory();
            return "They hesitate and quickly grab something from your cart before scrambling away.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            Game.ModifyMorale(-1);
            Game.ApplyBruiseWound(source: "Intimidating bandits");

            return "They're not impressed. They shove you aside and help themselves.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetHideOption()
    {
        int difficulty = Intensity switch
        {
            1 => 30,
            2 => 55,
        };

        return new SkillCheckOption()
        {
            Text = "Hide",
            Description = "Hold your breath and don't move. Maybe they'll take what they want and leave.",
            Action = Hide,
            Difficulty = difficulty,
            BiomeDifficultyModifiers = new Dictionary<BiomeDef, int>()
            {
                { BiomeDefOf.Woods, -15 }
            },
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Dexterity, 2 },
                { StatDefOf.Survival, 1 },
            },
            CanPartiallySucceed = false,
        };
    }
    private string Hide(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            IsEncounterDone = true;
            for (int i = 0; i < Intensity - 1; i++) Game.RemoveRandomItemFromInventory();
            return "You stay perfectly still. They rummage through your cart briefly, but miss most of your things and leave.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            IsEncounterDone = true;
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            return "They don't see you. They go through your cart and leave.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            IsPlayerCaughtHiding = true;
            return "They find you and drag you out.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            IsPlayerCaughtHiding = true;
            Game.ModifyMorale(-1);
            return "You panic and give yourself away. They drag you out.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetBegOption()
    {
        int difficulty = Intensity switch
        {
            1 => 40,
            2 => 60,
        };

        return new SkillCheckOption()
        {
            Text = "Beg",
            Description = "Plead with them to leave you something.",
            Action = Beg,
            Difficulty = difficulty,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 3 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsDestroyingItem = true,
                },
            },
            CanPartiallySucceed = false,
            CanCriticallySucceed = false,
        };
    }
    private string Beg(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            return "They look at you with pity. They take a few things and leave.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ApplyBruiseWound(source: "Begging bandits");
            Game.ModifyMorale(-1);
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();

            return "They don't care.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyBruiseDamage(2f, source: "Begging bandits");
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            Game.ModifyMorale(-3);
            return "They laugh at you.";
        }
        throw new InvalidOutcomeException();
    }

    #endregion
}
