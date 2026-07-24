using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Biome encounters happen once per day in the evening. Players can't come back to a specific biome encounter, a new instance is created every evening based on the biome the player is in.
/// <br/>A biome encounter offers a set of options on how the player wants to spend the evening. Only one of these options can be chosen. Some options are standardized (like "Rest early", "Fortify" or "Scavenge") and available in multiple biomes.
/// </summary>
public abstract class BiomeEncounter : Encounter
{
    private const string SET_TRAP_TEXT = "Place Trap";
    protected const string SPEND_EVENING_TEXT = "\n\nHow would you like to spend the evening?";
    private string EveningAction;
    private bool IsEveningActionChosen = false;

    public override void OnOptionChosen(EncounterOption option)
    {
        base.OnOptionChosen(option);

        // Set the evening action if it hasn't been chosen yet.
        if (!IsEveningActionChosen && option.Text != SET_TRAP_TEXT)
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
            string eveningAction = EveningAction;
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

        // Place trap
        options.Add(GetPlaceTrapOption());

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
    protected virtual int GetScavengeDifficulty() => -1;
    protected bool IsScavengeAvailable() => GetScavengeDifficulty() >= 0;

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
        IsEncounterDone = true;

        return "You lie down early. The extra rest will help a little. Now is the last chance to use items before going to sleep.";
    }

    
    private EncounterOption GetPlaceTrapOption()
    {
        return new FixedOutcomeOption()
        {
            Text = SET_TRAP_TEXT,
            Description = "Set up a trap to catch something useful or defend against attacks in the night.\nPlacing a trap will still allow you to do something else.",
            Action = PlaceTrap,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Trap,
                    IsDestroyingItem = true,
                },
            }
        };
    }
    private string PlaceTrap()
    {
        Game.PlaceEveningTrap(ItemUsedInOption);
        return "You set up a trap for the night." + SPEND_EVENING_TEXT;
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
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Survival, 2 },
                { StatDefOf.Dexterity, 1 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.BuildingMaterial,
                },
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.BuildingMaterial,
                },
            }
        };
    }
    private string Fortify(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            // Morale boost
            Game.ModifyStatBaseValue(StatDefOf.Morale, 2);

            // Decrease danger level by 2
            Game.ModifyDangerLevel(-2);

            // 25% to set a trap for the night
            if (Random.value < 0.25f)
            {
                Game.PlaceEveningTrap(ItemUsedInOption); // todo: remove
                return "You build an excellent shelter and even manage to set up a trap that will help defend against attacks in the night, or maybe catch something useful.";
            }

            // 75% to imporve a skill
            else
            {
                Game.ModifyRandomStat(1, 2, StatDefOf.Strength, StatDefOf.Dexterity);
                return "You build an excellent shelter, improving your skills. You feel safe tonight.";
            }           
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            Game.ModifyDangerLevel(-1);
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
            Game.ApplyRandomWound(source: "Failed fortify attempt");

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
            Difficulty = GetScavengeDifficulty(),
            Action = Scavenge,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Survival, 2 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Scavenging,
                },
            }
        };
    }
    private string Scavenge(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

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
            Game.ApplyCutDamage(1f, source: "Failed scavenge attempt");
            return "You cut yourself on something sharp while digging through debris.";
        }

        throw new System.Exception($"Outcome {outcome.DefName} not handled");
    }

    #endregion
}
