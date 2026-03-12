using System.Collections.Generic;
using UnityEngine;

public class BiomeEncounter_Woods : BiomeEncounter
{
    enum SettingType
    {
        DenseThicket,
        ForestClearing,
        FallenTree,
        StreamBank
    }
    private Dictionary<SettingType, float> SettingWeights = new Dictionary<SettingType, float>()
    {
        { SettingType.DenseThicket, 30 },
        { SettingType.ForestClearing, 35 },
        { SettingType.FallenTree, 25 },
        { SettingType.StreamBank, 10 }
    };
    private SettingType Setting;

    protected override int GetFortifyDifficulty()
    {
        return Setting switch
        {
            SettingType.DenseThicket => 30,
            SettingType.ForestClearing => 60,
            SettingType.FallenTree => 20,
            SettingType.StreamBank => 45,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    protected override int GetScavengeDifficulty()
    {
        return Setting switch
        {
            SettingType.DenseThicket => 55,
            SettingType.ForestClearing => 45,
            SettingType.FallenTree => 35,
            SettingType.StreamBank => 30,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    private int GetForageDifficulty()
    {
        return Setting switch
        {
            SettingType.DenseThicket => 60,
            SettingType.ForestClearing => 40,
            SettingType.FallenTree => 50,
            SettingType.StreamBank => 30,
            _ => throw new System.Exception("Invalid setting")
        };
    }

    protected override void OnInitialize()
    {
        Setting = SettingWeights.GetWeightedRandomElement();
        if (Setting == SettingType.ForestClearing) SetBackground("Woods_Clearing");
    }

    protected override string OnStart()
    {
        string text = "";
        if (Setting == SettingType.DenseThicket) text = "You arrive at some heavy undergrowth. The dense foliage makes it difficult to see far.";
        if (Setting == SettingType.ForestClearing) text = "You find a small clearing in the forest. It's a bit more open, but still surrounded by trees.";
        if (Setting == SettingType.FallenTree) text = "You come across a fallen tree. It provides some cover and a place to rest.";
        if (Setting == SettingType.StreamBank)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
            text = "You reach the bank of a small stream. The sound of flowing water is soothing.";
        }
        text += SPEND_EVENING_TEXT;
        return text;
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("DenseThicket", Setting == SettingType.DenseThicket);
        SetEncounterSpriteVisibility("FallenTree", Setting == SettingType.FallenTree);
        SetEncounterSpriteVisibility("Stream", Setting == SettingType.StreamBank);
    }

    protected override List<EncounterOption> GetAdditionalInitialOptions()
    {
        return new List<EncounterOption>()
        {
            GetAssembleTrapOption(),
            GetForageOption()
        };
    }

    protected override List<EncounterOption> GetFollowUpOptions()
    {
        return new List<EncounterOption>();
    }

    #region Options

    private EncounterOption GetAssembleTrapOption()
    {
        return new SkillCheckOption()
        {
            Text = "Assemble a trap",
            Description = "Rig a trap near your camp using branches, vines and whatever else is available. It will help during attacks or maybe provide food. Could even be reusable.",
            Difficulty = 50,
            Action = AssembleTrap,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Intelligence, 2 },
                { StatDefOf.Dexterity, 2 }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    SpecificItems = new List<ItemDef>() { ItemDefOf.Rope },
                    DifficultyReduction = 25,
                    DestructionChance = 0.3f,
                },
                new ItemSlot()
                {
                    ItemTags = new List<ItemTagDef>() { ItemTagDefOf.Weapon },
                    DifficultyReduction = 15,
                    DestructionChance = 0.5f,
                }
            }
        };
    }
    private string AssembleTrap(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            // Improve stat
            Game.ModifyRandomStat(1, 2, StatDefOf.Intelligence, StatDefOf.Dexterity, StatDefOf.Perception);

            // Set trap
            Game.PlaceEveningTrap();

            return "You build an ingenious trap, improving your skills. You feel confident about tonight.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.PlaceEveningTrap();

            return "You successfully rig a decent trap.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);
            return "The trap falls apart before you even finish. Wasted effort.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyRandomWound();
            return "The trap snaps shut on your hand.";
        }
        throw new System.Exception();
    }


    private EncounterOption GetForageOption()
    {
        return new SkillCheckOption()
        {
            Text = "Forage",
            Description = "Search for edible plants and medicinal herbs in the undergrowth.",
            Difficulty = GetForageDifficulty(),
            Action = Forage,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Perception, 2f },
                { StatDefOf.Intelligence, 2f }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    SpecificItems = new List<ItemDef>() { ItemDefOf.Knife },
                    DifficultyReduction = 15,
                }
            }
        };
    }
    private string Forage(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.AddNewItemToInventory(ItemDefOf.MedicinalHerbs);
            Game.AddNewItemToInventory(ItemDefOf.Berries);
            Game.ModifyRandomStat(1, 2, StatDefOf.Perception, StatDefOf.Intelligence);
            return "You find both a medicinal herb and some berries. Your scavenging skills improve";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            LootTables.Plants.AddItemToInventory();
            return "You find some useful plants.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ModifyHunger(-0.5f);
            return "You found some scattered small berries. Not worth to take with you, but eating them has reduced your hunger a little.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You search for a while but find nothing you recognize as safe.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            // todo: poison
            Game.ModifyHunger(+0.5f);
            return "You eat something you shouldn't have.";
        }
        throw new System.Exception();
    }

    #endregion
}
