using NUnit.Framework.Interfaces;
using System.Collections.Generic;

/// <summary>
/// Acts as the source for the quest leading to a hole in the fence.
/// </summary>
public class Encounter_RadioTower : LocationEncounter
{
    private WorldMapTile HomeOfR;
    private Area CityOfR => HomeOfR.City;

    private WorldMapTile FenceHoleTile;
    private Area ClosestAreaOfHole;

    enum PlayerPosition
    {
        Outside,
        Inside,
        OnTop
    }
    
    private PlayerPosition CurrentPlayerPosition;
    private bool IsNoteTaken;
    private bool TriedToListen;
    private bool IsDoorOpen;
    private bool TriedForcingDoor;
    private bool HasBeenOnTop;
    private bool TriedClimbing;
    private List<Item> ItemsInside;

    protected override void OnInitialize()
    {
        HomeOfR = WorldMap.GetRandomTile(biome: BiomeDefOf.City);
        FenceHoleTile = WorldMap.GetRandomTile(mustBorderFence: true);
        ClosestAreaOfHole = FenceHoleTile.GetClosestArea();

        ItemsInside = new List<Item>();
        ItemsInside.Add(Game.CreateItem(Game.GetRandomItemDef(), hidden: true));
        ItemsInside.Add(Game.CreateItem(Game.GetRandomItemDef(), hidden: true));
    }

    protected override string OnStart()
    {
        string text = "You approach the radio tower. The door is locked. There is a static sound in the air. A note is taped to the door.";

        TriedToListen = false;
        TriedClimbing = false;
        TriedForcingDoor = false;
        CurrentPlayerPosition = PlayerPosition.Outside;

        return text;
    }

    protected override void OnEnd() { }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        switch (CurrentPlayerPosition)
        {
            case PlayerPosition.Outside:
                if (!IsNoteTaken) options.Add(GetTakeNoteOption());
                if (!TriedToListen && !Game.HasMission(MissionId.GoToFenceHole) && !Game.HasMission(MissionId.GoToFenceHoleArea)) options.Add(GetListenOption());
                if (IsNoteTaken && !IsDoorOpen && !TriedForcingDoor) options.Add(GetForceDoorOption());
                if (!HasBeenOnTop && !TriedClimbing) options.Add(GetClimbTowerOption());
                break;
            case PlayerPosition.Inside:
                // No options for now, maybe add some later?
                break;
            case PlayerPosition.OnTop:
                options.Add(GetClimbDownOption());
                break;
        }

        return options;
    }
    protected override bool IsMoveOnOptionAvailable() => CurrentPlayerPosition == PlayerPosition.Outside;

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Tower", true);
        SetEncounterSpriteVisibility("Note", !IsNoteTaken);
        SetEncounterSpriteVisibility("DoorClosed", !IsDoorOpen);
        SetEncounterSpriteVisibility("DoorOpen", IsDoorOpen);
        ShowPlayerCharacter(CurrentPlayerPosition == PlayerPosition.Outside);
    }

    private void TakeItemsInside()
    {
        foreach (var item in ItemsInside)
        {
            Game.AddExistingItemToInventory(item);
        }
        ItemsInside.Clear();
    }

    

    // Options

    private EncounterOption GetTakeNoteOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Read note",
            Description = "Read the note on the door.",
            Action = ReadNote
        };
    }
    private string ReadNote()
    {
        string text = $"The note reads:\n\"Still transmitting. If you can hear this, the fence has a weak point. Find me in {CityOfR.Name}. - R'\"";
        Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
        Game.AddMission(new Mission(MissionId.FindRadioTowerR, $"Find R in {CityOfR.Name}", area: CityOfR));
        IsNoteTaken = true;

        return text;
    }


    private EncounterOption GetListenOption()
    {
        return new SkillCheckOption()
        {
            Text = "Listen",
            Description = "You think to hear a faint voice in the static. Try understanding it.",
            Action = Listen,
            Difficulty = 90,
            CanCriticallySucceed = false,
            CanCriticallyFail = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Perception, 3f }
             },
        };
    }
    private string Listen(OptionOutcomeDef outcome)
    {
        TriedToListen = true;
        string text = "";

        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = "You understand everything! The voice tells you the exact coordinates of a fence in the hole. You write it down immediately.";
            Game.ModifyStatBaseValue(StatDefOf.Morale, +2);
            Game.AddMission(new Mission(MissionId.GoToFenceHole, $"There is hole in the fence at {FenceHoleTile.Coordinates}.", location: FenceHoleTile));
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            text = $"You understand parts of the message. The voice mentions {ClosestAreaOfHole.Name}.";
            Game.AddMission(new Mission(MissionId.GoToFenceHoleArea, $"The radio voice mentioned {ClosestAreaOfHole.Name}.", area: ClosestAreaOfHole));
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "You can't make out anything useful from the static.";
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);
        }

        return text;
    }


    private EncounterOption GetForceDoorOption()
    {
        return new SkillCheckOption()
        {
            Text = "Force door",
            Description = "Try to force the door open.",
            Action = ForceDoor,
            Difficulty = 80,
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
            CanCriticallyFail = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Strength, 4f }
             },
        };
    }
    private string ForceDoor(OptionOutcomeDef outcome)
    {
        TriedForcingDoor = true;
        string text = "";
        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = "The door gives way. Inside: dusty equipment, a cot, and some supplies. You take them.";
            TakeItemsInside();
            IsDoorOpen = true;
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "The door holds. You strain your shoulder.";
            Game.DecreaseArmBoneHealth(0.2f);
        }
        return text;
    }


    private EncounterOption GetClimbTowerOption()
    {
        return new SkillCheckOption()
        {
            Text = "Climb tower",
            Description = "Climb the radio tower to get a better view of the surroundings.",
            Action = ClimbTower,
            Difficulty = 70,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Agility, 2f },
                { StatDefOf.Strength, 2f },
            }
        };
    }
    private string ClimbTower(OptionOutcomeDef outcome)
    {
        string text = "";
        TriedClimbing = true;

        if (outcome.SuccessLevel >= SuccessLevel.Success)
        {
            text = "You climb the tower and get a good view of the surroundings. The air up here is fresh, it clears your mind.";
            CurrentPlayerPosition = PlayerPosition.OnTop;
            HasBeenOnTop = true;
            Game.RevealLocationEncountersAround(Game.CurrentPosition);
            Game.ModifyStatBaseValue(StatDefOf.Intelligence, +1);
            Game.ModifyStatBaseValue(StatDefOf.Perception, +1);

            if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
            {
                ItemDef foundItem = Game.GetRandomItemDefWithTag(ItemTagDefOf.Tool);
                text += $" You also find a hidden a {foundItem.Label} and take it.";
                Game.AddNewItemToInventory(foundItem);
            }
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "You slip while climbing and fall to the ground, hurting yourself.";
            Game.ApplyBruiseDamage(0.2f);
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            text = "You slip while climbing and fall to the ground, hurting yourself badly.";
            Game.ApplyBruiseDamage(0.2f);
            Game.ApplyBruiseDamage(0.2f);
        }
        return text;
    }

    private EncounterOption GetClimbDownOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Climb down",
            Description = "Climb down from the tower.",
            Action = ClimbDown
        };
    }
    private string ClimbDown()
    {
        CurrentPlayerPosition = PlayerPosition.Outside;
        return "You climb down from the tower.";
    }
}
