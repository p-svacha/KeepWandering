using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class BiomeEncounter_City : BiomeEncounter
{
    enum SettingType
    {
        AbandondedApartment,
        ParkingGarage,
        BoardedUpShop,
        Alleyway
    }
    private Dictionary<SettingType, float> SettingWeights = new Dictionary<SettingType, float>()
    {
        { SettingType.AbandondedApartment, 0.35f },
        { SettingType.ParkingGarage, 0.25f },
        { SettingType.BoardedUpShop, 0.30f },
        { SettingType.Alleyway, 0.10f }
    };
    private SettingType Setting;

    protected override int GetFortifyDifficulty()
    {
        return Setting switch
        {
            SettingType.BoardedUpShop => 25,
            SettingType.AbandondedApartment => 35,
            SettingType.ParkingGarage => 50,
            SettingType.Alleyway => 65,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    protected override int GetScavengeDifficulty()
    {
        return Setting switch
        {
            SettingType.BoardedUpShop => 35,
            SettingType.AbandondedApartment => 30,
            SettingType.ParkingGarage => 45,
            SettingType.Alleyway => 60,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    private int GetKeepWatchDifficulty()
    {
        return Setting switch
        {
            SettingType.BoardedUpShop => 45,
            SettingType.AbandondedApartment => 50,
            SettingType.ParkingGarage => 25,
            SettingType.Alleyway => 35,
            _ => throw new System.Exception("Invalid setting")
        };
    }
    private bool IsEavesdropAvailable;

    protected override void OnInitialize()
    {
        Setting = SettingWeights.GetWeightedRandomElement();

        IsEavesdropAvailable = Setting switch
        {
            SettingType.BoardedUpShop => Random.value < 0.5f,
            SettingType.AbandondedApartment => true,
            SettingType.ParkingGarage => true,
            SettingType.Alleyway => false,
            _ => throw new System.Exception("Invalid setting")
        };
    }

    protected override string OnStart()
    {
        string text = "";
        if (Setting == SettingType.AbandondedApartment) text = "You find an apartment building with the front door kicked in. Most of the units have been ransacked, but the walls are solid.";
        if (Setting == SettingType.ParkingGarage) text = "You take shelter in a parking garage. The concrete echoes every sound, but you can see anyone coming from far away.";
        if (Setting == SettingType.BoardedUpShop) text = "You squeeze into a boarded-up shop through a gap in the planks. The shelves are mostly empty, but it's dry and hidden.";
        if (Setting == SettingType.Alleyway) text = "You duck into a narrow alley between two buildings. It's cramped and smells bad, but nobody's likely to look here.";
        text += SPEND_EVENING_TEXT;
        return text;
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("BackgroundSurvivors", IsEavesdropAvailable);
        SetEncounterSpriteVisibility("AbandondedApartment", Setting == SettingType.AbandondedApartment);
        SetEncounterSpriteVisibility("BoardedUpShop", Setting == SettingType.BoardedUpShop);
        SetEncounterSpriteVisibility("ParkingGarage", Setting == SettingType.ParkingGarage);
        SetEncounterSpriteVisibility("Alleyway", Setting == SettingType.Alleyway);
    }

    protected override bool IsRestEarlyAvailable() => false;
    protected override List<EncounterOption> GetAdditionalInitialOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        options.Add(GetKeepWatchOption());
        if (IsEavesdropAvailable) options.Add(GetEavesdropOption());

        return options;
    }

    protected override List<EncounterOption> GetFollowUpOptions()
    {
        return new List<EncounterOption>();
    }

    #region Options

    private EncounterOption GetKeepWatchOption()
    {
        return new SkillCheckOption()
        {
            Text = "Keep watch",
            Description = "Stay alert and observe your surroundings until late in the evening. You might spot threats early or notice something useful.",
            Action = KeepWatch,
            Difficulty = GetKeepWatchDifficulty(),
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Perception, 2 },
                { StatDefOf.Combat, 2 }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Weapon,
                    DifficultyReduction = 15,
                }
            },
        };
    }
    private string KeepWatch(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyDangerLevel(-1);
            BiomeLootTable.AddItemToInventory();
            return "You spot movement in the distance and identify a potential threat long before it arrives. You also notice a stash someone hid nearby.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyDangerLevel(-1);
            return "You keep a sharp eye out all evening. You know what's around you.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
            return "You stay up watching, but it's hard to make out much in the dark. At least you feel a bit more prepared.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            // todo: add exhaustion debuff for next day
            return "You try to stay alert, but exhaustion takes over. You keep dozing off and do not notice much.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyDangerLevel(+1);
            Game.ModifyStatBaseValue(StatDefOf.Morale, -2);
            return "You hear something and panic, knocking things over and making a lot of noise.";
        }
        throw new System.Exception("Invalid success level");
    }


    private EncounterOption GetEavesdropOption()
    {
        return new SkillCheckOption()
        {
            Text = "Eavesdrop",
            Description = "Other survivors are nearby. Stay quiet and listen in on their conversations. You might learn something useful.",
            Action = Eavesdrop,
            Difficulty = 55,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Perception, 1 },
                { StatDefOf.Charisma, 2 }
            },
        };
    }
    private string Eavesdrop(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            // todo: add supply stash encounter to nearby empty tile on world map and reveal (max radius 4)
            return "You overhear a detailed conversation about a supply stash nearby. This could be very useful.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.RevealRandomNearbyLocationEncounter();
            Game.RevealRandomNearbyLocationEncounter();
            return "You catch fragments of a conversation. Enough to piece together some useful information.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
            return "You hear people talking, but can't make out much. At least you know you're not alone out here.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You strain to listen, but hear nothing useful. Just the city's ambient noise.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ApplyBruiseDamage(2f);
            return "They noticed you listening. A rock comes flying from the darkness.";
        }
        throw new System.Exception("Invalid success level");
    }

#endregion
}
