using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Biome encounters happen once per day in the evening. Players can't come back to a specific biome encounter, a new instance is created every evening based on the biome the player is in.
/// <br/>A biome encounter offers a set of options on how the player wants to spend the evening. Only one of these options can be chosen. Some options are standardized (like "Rest early", "Fortify" or "Scavenge") and available in multiple biomes.
/// </summary>
public abstract class BiomeEncounter : Encounter
{
    private string EveningAction;
    private bool IsEveningActionChosen = false;

    public override void OnOptionChosen(EncounterOption option)
    {
        base.OnOptionChosen(option);

        // Set the evening action if it hasn't been chosen yet.
        if (!IsEveningActionChosen)
        {
            IsEveningActionChosen = true;
            EveningAction = option.Text;
        }
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        if (IsEveningActionChosen) // Evening action is chosen, options depend on the action
        {
            // Default options do not have follow up --> end encounter
            if (EveningAction == "Rest early") return options;
            if (EveningAction == "Fortify") return options;
            if (EveningAction == "Scavenge") return options;

            // Biome-specific options
            return GetFollowUpOptions();
        }
        else // Initial options where the player chooses how to spend the evening
        {
            options.AddRange(GetInitialOptions());
        }
        return options;
    }

    /// <summary>
    /// Returns a list of all options of how the player wants to spend the evening. Only one of these can be chosen.
    /// </summary>
    private List<EncounterOption> GetInitialOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        // Rest early
        if (IsRestEarlyAvailable()) options.Add(GetRestEarlyOption());

        // Fortify
        if (IsFortifyAvailable()) options.Add(GetFortifyOption());

        // Scavenge
        if (IsScavengeAvailable()) options.Add(GetScavengeOption());

        // Biome-specific options
        options.AddRange(GetAdditionalInitialOptions());

        return options;
    }

    /// <summary>
    /// Returns a list of biome-specific additional options of how the player can spend the evening.
    /// </summary>
    protected abstract List<EncounterOption> GetAdditionalInitialOptions();

    /// <summary>
    /// Returns a list of options that are available after the player has chosen how to spend the evening. Handles biome-specific follow up options.
    /// </summary>
    protected abstract List<EncounterOption> GetFollowUpOptions();

    protected override bool IsMoveOnOptionAvailable() => false;
    protected virtual bool IsRestEarlyAvailable() => true;
    protected virtual int GetFortifyDifficulty() => -1;
    private bool IsFortifyAvailable() => GetFortifyDifficulty() >= 0;
    protected virtual bool IsScavengeAvailable() => false;

    #region Options

    private EncounterOption GetRestEarlyOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Rest early",
            Description = "Turn in early. Get some extra rest to help with recovery.",
            Action = RestEarly,
        };
    }
    private string RestEarly()
    {
        Game.IsEarlyResting = true;
        return "You lie down early. The extra rest will help a little. Now is the last chance to use items before going to sleep.";
    }


    private EncounterOption GetFortifyOption()
    {
        return new SkillCheckOption()
        {
            Text = "Fortify",
            Description = "Spend the evening reinforcing your sleeping spot. Should make the night safer and maybe improves some of your skills.",
            Difficulty = GetFortifyDifficulty(),
            Action = Fortify,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Strength, 1f },
                { StatDefOf.Dexterity, 1f },
                { StatDefOf.Intelligence, 1f },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    ItemTags = new List<ItemTagDef>() { ItemTagDefOf.BuildingMaterial },
                    DifficultyReduction = 30,
                    DestructionChance = 0.5f,
                },
                new ItemSlot()
                {
                    ItemTags = new List<ItemTagDef>() { ItemTagDefOf.Tool },
                    DifficultyReduction = 20,
                    DestructionChance = 0.1f,
                },
            }
        };
    }
    private string Fortify(OptionOutcomeDef outcome)
    {
        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            // Improve stats
            List<StatDef> improvableStats = new List<StatDef>() { StatDefOf.Strength, StatDefOf.Dexterity, StatDefOf.Intelligence };
            StatDef statToImprove = improvableStats.RandomElement();
            int improvementAmount = Random.Range(1, 3 + 1);
            Game.ModifyStatBaseValue(statToImprove, improvementAmount);

            // Morale boost
            Game.ModifyStatBaseValue(StatDefOf.Morale, 2);

            // todo: Danger Level decrease by 2

            return "You build an excellent shelter, improving your skills. You feel safe tonight.";
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            // todo: Danger Level decrease by 1
            return "You set up some decent cover and noise traps.";
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);

            return "You try to stack some debris, but it keeps falling apart. Not much help.";
        }
        if(outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, -2);
            Game.ApplyRandomWound();

            return "You completely fail at building anything remotely helpful and hurt yourself in the process.";
        }
        throw new System.Exception("Invalid outcome for Fortify option: " + outcome);
    }


    private EncounterOption GetScavengeOption()
    {
        return new SkillCheckOption()
        {
            Text = "Scavenge",
            Description = "Search the area for anything useful.",
            Difficulty = 45,
            Action = Scavenge,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Dexterity, 1f },
                { StatDefOf.Perception, 1f },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    ItemTags = new List<ItemTagDef>() { ItemTagDefOf.Scavenging },
                    DifficultyReduction = 15,
                    DestructionChance = 0.1f,
                },
            }
        };
    }
    private string Scavenge(OptionOutcomeDef outcome)
    {
        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            BiomeLootTable.AddItemToInventory();
            BiomeLootTable.AddItemToInventory();

            Game.ModifyStatBaseValue(StatDefOf.Morale, 1);

            return "You find a hidden stash someone left behind.";
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            BiomeLootTable.AddItemToInventory();

            return "You find some useful supplies.";
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            LootTables.Trash.AddItemToInventory();

            return "Slim pickings, but you find something.";
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            return "Nothing useful. Wasted effort.";
        }
        if(outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            Game.ApplyCutDamage(1f);
            return "You cut yourself on something sharp while digging through debris.";
        }

        throw new System.Exception($"Outcome {outcome.DefName} not handled");
    }

    #endregion
}
