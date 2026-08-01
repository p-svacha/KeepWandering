using System.Collections.Generic;
using UnityEngine;

public class EveningEncounter : Encounter
{
    private Camp Camp => Camp.Instance;
    private CampRenderer CampRenderer;

    protected override void OnInitialize()
    {
        CampRenderer = Game.EncounterContainer.transform.Find($"{Def.DefName}/Camp").GetComponent<CampRenderer>();
    }

    protected override string OnStart()
    {
        return "How would you like to spend your evening?\n\nYou can set up your camp before deciding on an activity below. After choosing an activity, it will be too dark to make further changes.";
    }

    protected override void RefreshSprites()
    {
        CampRenderer.Refresh();
    }

    protected override void GetOptions(List<EncounterOption> options)
    {
        // Camp options (non-terminal, sprite-bound, any combination, done before the terminal choice below)
        if (!Camp.HasTent) options.Add(GetSetUpShelterOption());
        if (!Camp.HasBedroll) options.Add(GetSetUpSleepingSpotOption());
        if (!Camp.HasFire) options.Add(GetMakeFireOption());
        if (Camp.Trap1 == null) options.Add(GetSetTrapOption(1, CampRenderer.Trap1));
        if (Camp.Trap2 == null) options.Add(GetSetTrapOption(2, CampRenderer.Trap2));
        if (Camp.Trap3 == null) options.Add(GetSetTrapOption(3, CampRenderer.Trap3));
        if (Camp.HasFire) options.Add(GetCookOption());

        // Spend-the-evening options (terminal, dialogue list, exactly one ends the encounter)
        options.Add(GetRestEarlyOption());
        options.Add(GetScavengeOption());

        // Biome-specific options
        options.AddRange(GetBiomeSpecificOptions());
    }

    #region Camp Options

    private EncounterOption GetSetUpShelterOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Set up Shelter",
            Description = $"Pitch your tent. Decreases the chance of being attacked during the night and gives +{Camp.TENT_MORALE_BONUS} morale the next day.",
            Sprite = CampRenderer.TentSpot,
            Action = SetUpTent,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Tent,
                    IsDestroyingItem = true,
                }
            }
        };
    }
    private string SetUpTent()
    {
        Game.SetUpTent(ItemUsedInOption);
        return "You set up your tent for the night.";
    }

    private EncounterOption GetSetUpSleepingSpotOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Set up Sleeping Spot",
            Description = $"Lay out your bedroll. Improves how much you heal overnight and gives +{Camp.BEDROLL_MORALE_BONUS} morale for the next day.",
            Sprite = CampRenderer.BedrollSpot,
            Action = SetUpBedroll,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Bedroll,
                    IsDestroyingItem = true,
                }
            }
        };
    }
    private string SetUpBedroll()
    {
        Game.SetUpBedroll(ItemUsedInOption);
        return "You lay out your bedroll for the night.";
    }

    private EncounterOption GetMakeFireOption()
    {
        return new SkillCheckOption()
        {
            Text = "Make Fire",
            Description = $"Get a fire going. Enables cooking, keeps wildlife away for the night, and gives +{Camp.FIRE_MORALE_BONUS} morale for the next day.",
            Sprite = CampRenderer.Fire,
            Difficulty = 120,
            BiomeDifficultyModifiers = new Dictionary<BiomeDef, int>()
            {
                { BiomeDefOf.Woods, -10 },
            },
            Action = MakeFire,
            CanPartiallySucceed = false,
            CanCriticallySucceed = false,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Survival, 2 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Tag = ItemTagDefOf.Fuel,
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.FireStarter,
                }
            }
        };
    }
    private string MakeFire(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.Success)
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
            Description = "Traps help defending against attacks in the night, or may catch wildlife, providing resources.",
            Sprite = sprite,
            Action = () => SetTrap(slot),
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.Trap,
                    IsDestroyingItem = true,
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

    #region Spend-the-Evening Options (Base)

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
        return "You lie down early. The extra rest will help a little.";
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
                new ItemSlot() { Tag = ItemTagDefOf.LightSource }
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

    #endregion

    #region Spend-the-Evening Options (Biome specific)

    private List<EncounterOption> GetBiomeSpecificOptions()
    {
        if (Biome == BiomeDefOf.Woods) return GetWoodsOptions();
        else if (Biome == BiomeDefOf.Outskirts) return GetOutskirtsOptions();
        else if (Biome == BiomeDefOf.City) return GetCityOptions();
        else throw new System.Exception("No biome-specific options defined for biome " + Biome.DefName);
    }

    // Woods
    private List<EncounterOption> GetWoodsOptions()
    {
        return new List<EncounterOption>()
        {
            GetTrainSurvivalOption(),
        };
    }

    private EncounterOption GetTrainSurvivalOption()
    {
        int difficultyIncreaseFromExistingSkill = Game.Player.GetStatValue(StatDefOf.Survival) * 10;

        return new SkillCheckOption()
        {
            Text = "Train Survival Skill",
            Description = "Try studying the wilderness and your bushcraft skills.",
            Difficulty = 50,
            CanCriticallyFail = false,
            CanPartiallySucceed = false,
            FixedDifficultyModifiers =
            {
                new ("Current Survival Skill (x10)", difficultyIncreaseFromExistingSkill),
            },
            ItemSlots =
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.FieldGuide,
                }
            },
            Action = TrainSurvival,
        };
    }
    private string TrainSurvival(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyStatBaseValue(StatDefOf.Survival, +2);
            return "You have a breakthrough in your understanding of the wilderness!";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyStatBaseValue(StatDefOf.Survival, +1);
            return "You make some progress in your survival skills.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You fail to accomplish anything useful.";
        }
        throw new InvalidOutcomeException();
    }


    // Outskirts
    private List<EncounterOption> GetOutskirtsOptions()
    {
        return new List<EncounterOption>() { };
    }


    // City
    private List<EncounterOption> GetCityOptions()
    {
        return new List<EncounterOption>()
        {
            GetTrainSocialOption(),
            GetFindTraderOption(),
        };
    }

    private EncounterOption GetTrainSocialOption()
    {
        int difficultyIncreaseFromExistingSkill = Game.Player.GetStatValue(StatDefOf.Social) * 10;

        return new SkillCheckOption()
        {
            Text = "Train Social Skill",
            Description = "Try to meet and talk to people to improve your social skills.",
            Difficulty = 50,
            CanCriticallyFail = false,
            CanPartiallySucceed = false,
            FixedDifficultyModifiers =
            {
                new ("Current Social Skill (x10)", difficultyIncreaseFromExistingSkill),
            },
            Action = TrainSocial,
        };
    }
    private string TrainSocial(OptionOutcomeDef outcome)
    {
        IsEncounterDone = true;

        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            // 50% to receive a gift
            if (Random.value < 0.5f)
            {
                Game.ModifyStatBaseValue(StatDefOf.Social, +1);
                Game.AddNewItemToInventory(Game.GetRandomItemDef());
                return "You manage to find someone you connect with. They even give you a gift!";
            }

            // 50% for +2 social
            else
            {
                Game.ModifyStatBaseValue(StatDefOf.Social, +2);
                return "You manage to find great company and have a wonderful time socializing.";
            }
            
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyStatBaseValue(StatDefOf.Social, +1);
            return "You have a pleasant interaction and improve your social skills.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You fail to find anyone to interact with.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetFindTraderOption()
    {
        return new SkillCheckOption()
        {
            Text = "Find Trader",
            Description = "Seek out someone willing to trade.",
            Difficulty = 55,
            CanPartiallySucceed = false,
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

    protected override void OnTradingDone()
    {
        IsEncounterDone = true;
    }

    #endregion

}