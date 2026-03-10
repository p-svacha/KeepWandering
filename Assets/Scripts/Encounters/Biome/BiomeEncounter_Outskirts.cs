using System.Collections.Generic;
using UnityEngine;

public class BiomeEncounter_Outskirts : BiomeEncounter
{
    enum SettingType
    {
        AbandonedFarmstead,
        RoadsideDitch,
        CrumblingWall,
        OldShed
    }
    private Dictionary<SettingType, float> SettingWeights = new Dictionary<SettingType, float>()
    {
        { SettingType.AbandonedFarmstead, 0.4f },
        { SettingType.RoadsideDitch, 0.3f },
        { SettingType.CrumblingWall, 0.2f },
        { SettingType.OldShed, 0.1f }
    };
    private SettingType Setting;


    private bool IsPasserbyOptionAvailable;

    private bool IsTradingWithPasserby;
    private ItemDef OfferedTradeItem;

    protected override int GetFortifyDifficulty()
    {
        return Setting switch
        {
            SettingType.AbandonedFarmstead => 25,
            SettingType.RoadsideDitch => 60,
            SettingType.CrumblingWall => 45,
            SettingType.OldShed => 35,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    protected override int GetScavengeDifficulty()
    {
        return Setting switch
        {
            SettingType.AbandonedFarmstead => 35,
            SettingType.RoadsideDitch => 55,
            SettingType.CrumblingWall => 55,
            SettingType.OldShed => 30,
            _ => throw new System.Exception("Invalid setting")
        };
    }

    protected int GetFlagDownPasserbyDifficulty()
    {
        return Setting switch
        {
            SettingType.AbandonedFarmstead => 70,
            SettingType.RoadsideDitch => 40,
            SettingType.CrumblingWall => 50,
            SettingType.OldShed => 60,
            _ => throw new System.Exception("Invalid setting")
        };
    }

    protected override void OnInitialize()
    {
        Setting = SettingWeights.GetWeightedRandomElement();

        if (Setting == SettingType.RoadsideDitch) IsPasserbyOptionAvailable = true;
        else IsPasserbyOptionAvailable = Random.value < 0.5f;
    }

    protected override string OnStart()
    {
        string text = "";
        if (Setting == SettingType.AbandonedFarmstead) text = "You settle near an abandoned farmstead. The barn roof is half-collapsed, but the walls block the wind.";
        if (Setting == SettingType.RoadsideDitch) text = "You make camp in a ditch alongside a cracked road. Not comfortable, but it'll do.";
        if (Setting == SettingType.CrumblingWall) text = "You find the remains of an old stone wall. Good enough to lean against for the night.";
        if (Setting == SettingType.OldShed) text = "A small shed stands crooked in the tall grass. The door hangs open.";
        text += "\n\nHow would you like to spend the evening?";
        return text;
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Path", IsPasserbyOptionAvailable);
        SetEncounterSpriteVisibility("DistantPerson", IsPasserbyOptionAvailable && !IsTradingWithPasserby);
        SetEncounterSpriteVisibility("Farmhouse", Setting == SettingType.AbandonedFarmstead);
        SetEncounterSpriteVisibility("Ditch", Setting == SettingType.RoadsideDitch);
        SetEncounterSpriteVisibility("Wall", Setting == SettingType.CrumblingWall);
        SetEncounterSpriteVisibility("Shed", Setting == SettingType.OldShed);
        SetEncounterSpriteVisibility("Passerby", IsTradingWithPasserby);
    }

    protected override void OnMoveOn()
    {
        IsTradingWithPasserby = false;
    }



    #region Options

    protected override bool IsMoveOnOptionAvailable() => IsTradingWithPasserby;
    protected override List<EncounterOption> GetAdditionalInitialOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (IsPasserbyOptionAvailable) options.Add(GetFlagDownPasserbyOption());

        return options;
    }
    protected override List<EncounterOption> GetFollowUpOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        if (IsTradingWithPasserby)
        {
            options.Add(GetBuyItemOption());
            options.Add(GetBuyInformationOption());
        }
        return options;
    }


    private EncounterOption GetFlagDownPasserbyOption()
    {
        return new SkillCheckOption()
        {
            Text = "Flag down passerby",
            Description = "You see someone in the distance. Maybe you can get their attention in order to trade or get information.",
            Action = FlagDownPasserby,
            Difficulty = GetFlagDownPasserbyDifficulty(),
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Charisma, 3f }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    SpecificItems = new List<ItemDef>() { ItemDefOf.Coin },
                    DifficultyReduction = 25,
                    DestructionChance = 1f
                },
            },
        };
    }
    private string FlagDownPasserby(OptionOutcomeDef outcome)
    {
        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            BiomeLootTable.AddItemToInventory();
            Game.ModifyStatBaseValue(StatDefOf.Morale, 1);
            // todo: add a rumour reveal

            return "A friendly traveler stops and shares generously, before continuing on their way.";
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            IsTradingWithPasserby = true;
            OfferedTradeItem = BiomeLootTable.Resolve();
            return "A traveler stops. You exchange a few words. They offer you a trade.";
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            LootTable table = new LootTable
            {
                { LootTables.Trash, 10 },
                { ItemDefOf.Coin, 10 }
            };
            table.AddItemToInventory();

            return "Someone passes but doesn't want to stop. They think you are someone in need and toss you something.";
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            return "Nobody comes by, or they ignore you.";
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            Game.ApplyBruiseWound();
            Game.RemoveRandomItemFromInventory();
            return "A hostile stranger. They shove you down and grab something from your cart before running off.";
        }
        throw new System.Exception("Invalid outcome");
    }



    private EncounterOption GetBuyItemOption()
    {
        return new FixedOutcomeOption()
        {
            Text = $"Buy {OfferedTradeItem.Label} for 1 coin.",
            Action = BuyItem,
            OncePerDay = true,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    SpecificItems = new List<ItemDef>() { ItemDefOf.Coin },
                    IsRequired = true,
                    DestructionChance = 1f
                }
            }
        };
    }
    private string BuyItem()
    {
        Game.AddNewItemToInventory(OfferedTradeItem);
        return $"You trade a coin for {OfferedTradeItem.Label}.";
    }

    private EncounterOption GetBuyInformationOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Buy information for 1 coin.",
            Description = "The traveler offers to share some information for a price.",
            Action = BuyInformation,
            OncePerDay = true,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    SpecificItems = new List<ItemDef>() { ItemDefOf.Coin },
                    IsRequired = true,
                    DestructionChance = 1f
                }
            }
        };
    }
    private string BuyInformation()
    {
        // todo: add a rumour reveal
        return $"You trade a coin for a piece of information.";
    }

    #endregion
}
