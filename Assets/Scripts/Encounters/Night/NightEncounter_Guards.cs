using System.Collections.Generic;
using UnityEngine;

public class NightEncounter_Guards : NightEncounter
{
    // Intensity: 1 = lone guard, 2 = patrol of two, 3 = full patrol of three

    protected override string OnStart()
    {
        string text = "";

        if (Intensity == 1) text = "A flashlight sweeps over your camp. A guard from the fence walks up to you. 'Being this close to the fence at night is a quarantine violation.'";
        if (Intensity == 2) text = "Two guards from the fence enter your camp, flashlights raised. 'Quarantine violation. Nobody is allowed near the fence at night. Stay where you are!'";
        if (Intensity == 3) text = "Three guards in protective gear surround your camp. 'You are camping next to the fence. That is a quarantine violation. Hands where we can see them.'";

        if (Game.Camp.NumTrapsUsedToDefendNightAttack > 0) text += " It seems as your traps have weakened the attack.";

        return text;
    }

    protected override void RefreshSprites()
    {
        base.RefreshSprites();

        bool showGuards = !IsEncounterDone;

        SetObjectVisibility("Guard1", showGuards && Intensity >= 1);
        SetObjectVisibility("Guard2", showGuards && Intensity >= 2);
        SetObjectVisibility("Guard3", showGuards && Intensity >= 3);
    }

    protected override void GetOptions(List<EncounterOption> options)
    {
        options.Add(GetBluffOption());
        options.Add(GetBribeOption());
        options.Add(GetSneakAwayOption());
        options.Add(GetFightOption());
    }

    #region Options

    private EncounterOption GetBluffOption()
    {
        int difficulty = Intensity switch
        {
            1 => 30,
            2 => 50,
            3 => 70,
            _ => 70,
        };

        return new SkillCheckOption()
        {
            Text = "Bluff",
            Description = "Claim you are a lost survivor on your way to the evacuation point. They might believe you and leave you alone.",
            Action = Bluff,
            BaseDifficulty = difficulty,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 3 },
            },
            CanPartiallySucceed = false,
        };
    }
    private string Bluff(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyMorale(+1);
            Game.ModifyStatBaseValue(StatDefOf.Social, +1);
            return "They buy your story completely. One of them even tells you which direction to avoid before they leave.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            return "They look you over, then wave you off with a warning to stay away from the fence.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ModifyMorale(-1);
            Game.ApplyBruiseDamage(Intensity, source: "Guards");
            Game.RemoveRandomItemFromInventory();
            return "They don't believe a word. They push you around and confiscate some of your things.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyMorale(-2);
            Game.ApplyBruiseDamage(Intensity + 1, source: "Guards");
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            return "Your story falls apart. They beat you and strip your cart of everything that looks useful.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetBribeOption()
    {
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        for (int i = 0; i < Intensity; i++)
        {
            itemSlots.Add(new ItemSlot()
            {
                Tag = ItemTagDefOf.Bribe,
                IsRequired = true,
                IsDestroyingItem = true,
            });
        }

        return new FixedOutcomeOption()
        {
            Text = "Bribe",
            Description = $"Pay them to look the other way. Requires {Intensity} {"bribe item".Pluralize(Intensity)}, one for each guard.",
            Action = Bribe,
            ItemSlots = itemSlots,
        };
    }
    private string Bribe()
    {
        IsEncounterDone = true;
        Game.ModifyMorale(-1);
        return "They pocket your offer without a word and walk back towards the fence.";
    }


    private EncounterOption GetSneakAwayOption()
    {
        int difficulty = Intensity switch
        {
            1 => 35,
            2 => 50,
            3 => 70,
            _ => 70,
        };

        return new SkillCheckOption()
        {
            Text = "Sneak away",
            Description = "Slip away into the dark before they reach you. You will have to leave some of your things behind.",
            Action = SneakAway,
            BaseDifficulty = difficulty,
            BiomeDifficultyModifiers = new Dictionary<BiomeDef, int>()
            {
                { BiomeDefOf.Woods, -15 },
                { BiomeDefOf.Outskirts, +10 },
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
            return "You slip away unseen and circle back to your cart after they pass. They never notice you were there.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.RemoveRandomItemFromInventory();
            return "You slip away in the dark. The guards go through your camp, take something and leave.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ApplyBruiseWound(source: "Sneaking away from guards");
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            return "A flashlight catches you at the edge of the camp. They tackle you before you get far.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ApplyBruiseDamage(2f, source: "Sneaking away from guards");
            Game.ModifyMorale(-1);
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            return "They spot you immediately and drag you back. They confiscate a lot of your things.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyBruiseDamage(3f, source: "Sneaking away from guards");
            Game.ApplyCutWound(source: "Sneaking away from guards");
            Game.ModifyMorale(-2);
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            return "You run straight into a guard. They club you down and take what they want.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetFightOption()
    {
        int difficulty = Intensity switch
        {
            1 => 60,
            2 => 85,
            3 => 110,
            _ => 110,
        };

        return new SkillCheckOption()
        {
            Text = "Fight",
            Description = "They are armed and trained. This is very risky.",
            Action = Fight,
            BaseDifficulty = difficulty,
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
            },
            CanPartiallySucceed = false,
        };
    }
    private string Fight(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        string guards = "guard".Pluralize(Intensity);
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyStatBaseValue(StatDefOf.Strength, +1);
            Game.ModifyMorale(+1);
            LootTables.Bandit.AddItemToInventory();
            return $"You take the {guards} by surprise and put them down. You take something from their gear before they flee.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ApplyBruiseDamage(Intensity, source: "Fighting guards");
            return $"After a brutal scuffle, the {guards} retreat towards the fence.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ApplyBruiseDamage(Intensity + 2, source: "Fighting guards");
            Game.ModifyMorale(-2);
            for (int i = 0; i < Intensity; i++) Game.RemoveRandomItemFromInventory();
            return $"The {guards} overpower you easily, beat you and confiscate your things.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyBruiseDamage(Random.Range(2f, 3f), source: "Fighting guards");
            Game.ApplyCutDamage(Random.Range(2f, 3f), source: "Fighting guards");
            Game.ModifyMorale(-3);
            for (int i = 0; i < Intensity + 1; i++) Game.RemoveRandomItemFromInventory();
            return "They hit you hard and without mercy. You black out and wake up to a ransacked camp.";
        }
        throw new InvalidOutcomeException();
    }

    #endregion
}
