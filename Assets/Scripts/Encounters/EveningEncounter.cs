using System.Collections.Generic;
using UnityEngine;

public class EveningEncounter : Encounter
{
    private SpriteRenderer TentSprite;
    private SpriteRenderer BedrollSprite;
    private SpriteRenderer FireSprite;
    private SpriteRenderer Trap1Sprite;
    private SpriteRenderer Trap2Sprite;
    private SpriteRenderer Trap3Sprite;

    protected override void OnInitialize()
    {
        TentSprite = GetSprite("Tent");
        BedrollSprite = GetSprite("Bedroll");
        FireSprite = GetSprite("Fire");
        Trap1Sprite = GetSprite("Trap1");
        Trap2Sprite = GetSprite("Trap2");
        Trap3Sprite = GetSprite("Trap3");
    }

    protected override string OnStart()
    {
        return "How would you like to spend your evening?";
    }

    protected override void RefreshSprites()
    {
        SetSprite(TentSprite, Camp.Instance.HasTent ? "Tent" : "TentSpot");
        SetSprite(BedrollSprite, Camp.Instance.HasBedroll ? "Bedroll" : "BedrollSpot");
        SetSprite(FireSprite, Camp.Instance.HasFire ? "Fire" : "FireSpot");
        SetSprite(Trap1Sprite, Camp.Instance.Trap1 != null ? "Trap" : "TrapSpot");
        SetSprite(Trap2Sprite, Camp.Instance.Trap2 != null ? "Trap" : "TrapSpot");
        SetSprite(Trap3Sprite, Camp.Instance.Trap3 != null ? "Trap" : "TrapSpot");
    }

    protected override bool IsMoveOnOptionAvailable() => false;

    /// <summary>
    /// "Find Trader" hands off to the base class's trading system, which doesn't naturally end the
    /// encounter on its own. We only want the evening to end once the player is actually done trading.
    /// </summary>
    public override void OnOptionChosen(EncounterOption option)
    {
        base.OnOptionChosen(option);
        if (option.Text == "Done trading") IsEncounterDone = true;
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        // Camp options - non-terminal, sprite-bound, any combination, done before the terminal choice below
        if (!Camp.Instance.HasTent) options.Add(GetSetUpShelterOption());
        if (!Camp.Instance.HasBedroll) options.Add(GetSetUpSleepingSpotOption());
        if (!Camp.Instance.HasFire) options.Add(GetMakeFireOption());
        if (Camp.Instance.Trap1 == null) options.Add(GetSetTrapOption(1, Trap1Sprite));
        if (Camp.Instance.Trap2 == null) options.Add(GetSetTrapOption(2, Trap2Sprite));
        if (Camp.Instance.Trap3 == null) options.Add(GetSetTrapOption(3, Trap3Sprite));

        // Spend-the-evening options - terminal, dialogue list, exactly one ends the encounter
        options.Add(GetRestEarlyOption());
        options.Add(GetScavengeOption());
        options.AddRange(GetTrainSkillOptions());
        if (Biome == BiomeDefOf.City) options.Add(GetFindTraderOption());

        return options;
    }

    #region Camp Options

    private EncounterOption GetSetUpShelterOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Set up Shelter",
            Description = "Pitch your tent. Makes tonight safer, but only for tonight.",
            Sprite = TentSprite,
            Action = SetUpShelter,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Tent,
                    IsDestroyingItem = true, // see system note below
                }
            }
        };
    }
    private string SetUpShelter()
    {
        Game.SetUpTent(ItemUsedInOption);
        return "You set up your tent for the night.";
    }

    private EncounterOption GetSetUpSleepingSpotOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Set up Sleeping Spot",
            Description = "Lay out your bedroll. Improves how much you heal overnight.",
            Sprite = BedrollSprite,
            Action = SetUpSleepingSpot,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Bedroll,
                    IsDestroyingItem = true, // see system note below
                }
            }
        };
    }
    private string SetUpSleepingSpot()
    {
        Game.SetUpBedroll(ItemUsedInOption);
        return "You lay out your bedroll for the night.";
    }

    private EncounterOption GetMakeFireOption()
    {
        return new SkillCheckOption()
        {
            Text = "Make Fire",
            Description = "Get a fire going. Enables cooking and keeps wildlife away for the night.",
            Difficulty = 40,
            Action = MakeFire,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Survival, 2 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Fuel, // new item def needed, see notes below
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.FireStarter, // new tag def needed, see notes below
                }
            }
        };
    }
    private string MakeFire(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.Success || outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.MakeFire();
            return "You get a fire going.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You can't get the fire to catch. The fuel is wasted.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);
            return "You fumble and singe your fingers. The fuel is wasted.";
        }
        throw new InvalidOutcomeException();
    }

    private EncounterOption GetSetTrapOption(int slot, SpriteRenderer sprite)
    {
        return new FixedOutcomeOption()
        {
            Text = "Set Trap",
            Description = "Set up a trap. May help defend against a night attack, or catch something useful if unused.",
            Sprite = sprite,
            Action = () => SetTrap(slot),
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Trap,
                    IsDestroyingItem = true, // see system note below
                }
            }
        };
    }
    private string SetTrap(int slot)
    {
        Game.PlaceEveningTrap(ItemUsedInOption, slot);
        return "You set up a trap.";
    }

    #endregion

    #region Spend-the-Evening Options

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

    private int GetScavengeDifficulty()
    {
        if (Biome == BiomeDefOf.Woods) return 45;
        if (Biome == BiomeDefOf.Outskirts) return 40;
        if (Biome == BiomeDefOf.City) return 40;
        throw new System.Exception("Scavenge difficulty not defined for biome " + Biome.DefName);
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
                new ItemSlot() { Tag = ItemTagDefOf.Scavenging }
            }
        };
    }
    private string Scavenge(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            BiomeLootTable.AddItemToInventory();
            BiomeLootTable.AddItemToInventory();
            Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
            return "You find a hidden stash someone left behind.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            BiomeLootTable.AddItemToInventory();
            return "You find some useful supplies.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            LootTables.Trash.AddItemToInventory();
            return "Slim pickings, but you find something.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "Nothing useful. Wasted effort.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyCutDamage(1f, source: "Failed scavenge attempt");
            return "You cut yourself on something sharp while digging through debris.";
        }
        throw new InvalidOutcomeException();
    }

    private int GetTrainSkillDifficulty(StatDef stat)
    {
        int currentValue = Game.Player.GetStatValue(stat);
        return Mathf.Clamp(20 + currentValue * 4, 10, 95);
    }
    private List<EncounterOption> GetTrainSkillOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (Biome == BiomeDefOf.Woods) options.Add(GetTrainSkillOption(StatDefOf.Survival, ItemTagDefOf.Scavenging, "Track and forage with purpose, sharpening your survival instincts."));
        if (Biome == BiomeDefOf.Outskirts) options.Add(GetTrainSkillOption(StatDefOf.Strength, ItemTagDefOf.Digging, "Put in some hard, honest labor to build your strength."));
        if (Biome == BiomeDefOf.City) options.Add(GetTrainSkillOption(StatDefOf.Social, ItemTagDefOf.Charm, "Practice reading and talking to people to sharpen your social skills."));
        if (Camp.Instance.HasFire) options.Add(GetTrainSkillOption(StatDefOf.Dexterity, ItemTagDefOf.Lockpicking, "Tinker with your gear by the firelight to steady your hands."));

        // todo: replace these

        return options;
    }
    private EncounterOption GetTrainSkillOption(StatDef stat, ItemTagDef tag, string description)
    {
        return new SkillCheckOption()
        {
            Text = $"Train {stat.LabelCapWord}",
            Description = description,
            Difficulty = GetTrainSkillDifficulty(stat),
            Action = (outcome) => TrainSkill(outcome, stat),
            CanCriticallyFail = false,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot() { Tag = tag }
            }
        };
    }
    private string TrainSkill(OptionOutcomeDef outcome, StatDef stat)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess || outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyStatBaseValue(stat, +1);
            return $"You spend the evening deliberately practicing, and your {stat.LabelCapWord.ToLower()} improves.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess || outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You practice, but it doesn't seem to have paid off tonight.";
        }
        throw new InvalidOutcomeException();
    }

    private EncounterOption GetFindTraderOption()
    {
        return new SkillCheckOption()
        {
            Text = "Find Trader",
            Description = "Seek out someone willing to trade. There's no guarantee of luck.",
            Difficulty = 55,
            Action = FindTrader,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 2 },
            },
        };
    }
    private string FindTrader(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            List<ItemDef> offeredItems = BiomeLootTable.ResolveMultiple(3);
            return InitiateTrade("You track down a trader eager to do business, and they seem to have good stock.", offeredItems, canBuyRumour: true);
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            List<ItemDef> offeredItems = BiomeLootTable.ResolveMultiple(2);
            return InitiateTrade("You track down a trader willing to do business.", offeredItems, canBuyRumour: true);
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            IsEncounterDone = true;
            Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
            return "You don't find a trader, but you enjoy a quiet walk through the streets.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            IsEncounterDone = true;
            return "You search but can't find anyone willing to trade tonight.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            IsEncounterDone = true;
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);
            return "You wander into the wrong part of town and have to hurry back, rattled.";
        }
        throw new InvalidOutcomeException();
    }

    #endregion
}