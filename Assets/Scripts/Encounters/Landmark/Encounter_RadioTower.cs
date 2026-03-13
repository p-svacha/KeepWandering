using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acts as the source for the quest leading to a tile where the fence can be cut.
/// </summary>
public class Encounter_RadioTower : LocationEncounter
{
    enum PlayerPosition
    {
        Outside,
        Inside,
        OnTop
    }

    // Flag if this is quest relevant for the "Go to unpowered fence" quest and finding R
    // Only one radio tower will have this
    public bool HasNoteOnDoor;
    
    private PlayerPosition CurrentPlayerPosition;
    private bool IsNoteTaken;
    private bool IsDoorOpen;
    private bool HasBeenOnTop;
    private List<Item> ItemsInside;

    protected override void OnInitialize()
    {
        ItemsInside = new List<Item>();
        ItemsInside.Add(Game.CreateItem(Game.GetRandomItemDef(), hidden: true));
        ItemsInside.Add(Game.CreateItem(Game.GetRandomItemDef(), hidden: true));
    }

    protected override string OnStart()
    {
        string text = "You approach the radio tower.";

        if (!IsDoorOpen) text += " The door is locked.";
        if (IsDoorOpen) text += " The door is open.";

        if (!Game.HasQuestStarted(QuestDefOf.GoToUnpoweredFence) && HasNoteOnDoor) text += " There is a static sound in the air.";
        if (HasNoteOnDoor && !IsNoteTaken) text += " A note is taped to the door.";


        CurrentPlayerPosition = PlayerPosition.Outside;

        return text;
    }

    protected override List<EncounterOption> GetOptions()
    {
        // Note has to be taken and read before anything else for narrative clarity
        if (HasNoteOnDoor && !IsNoteTaken) return new List<EncounterOption>() { GetTakeNoteOption() };

        List<EncounterOption> options = new List<EncounterOption>();

        switch (CurrentPlayerPosition)
        {
            case PlayerPosition.Outside:
                if (!Game.HasQuestStarted(QuestDefOf.GoToUnpoweredFence) && HasNoteOnDoor) options.Add(GetListenOption());
                if (!IsDoorOpen) options.Add(GetForceDoorOption());
                if (!HasBeenOnTop) options.Add(GetClimbTowerOption());
                break;
            case PlayerPosition.Inside:
                // Player can't be permanently inside atm
                break;
            case PlayerPosition.OnTop:
                options.Add(GetClimbDownOption());
                break;
        }

        return options;
    }
    protected override bool IsMoveOnOptionAvailable() => CurrentPlayerPosition == PlayerPosition.Outside && IsNoteTaken;

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Tower", true);
        SetEncounterSpriteVisibility("Note", HasNoteOnDoor && !IsNoteTaken);
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
        string text = $"The note reads:\n\"Still transmitting. If you can hear this, the fence has a weak point. Find me in {StoryManager.CityOfR.Name}. - R'\"";
        if (Game.QuestStates[QuestDefOf.FindR] == QuestState.Completed)
        {
            text += "\n\nYou have already found R, so you know all about it.";
        }


        Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
        if (!Game.HasQuestStarted(QuestDefOf.FindR))
        {
            Game.StartQuest(new Quest(QuestDefOf.FindR, $"Find R in {StoryManager.CityOfR.Name}", area: StoryManager.CityOfR));
        }
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
            OncePerDay = true,
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
        string text = "";

        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = "You understand everything! The voice tells you the exact coordinates of a fence segment that is unpowered and could be cut through with a fence cutter.";
            Game.ModifyStatBaseValue(StatDefOf.Morale, +2);
            Game.StartQuest(new Quest(QuestDefOf.GoToUnpoweredFence, $"The fence at {StoryManager.CuttableFenceTile.Coordinates} is unpowered and can be cut with a fence cutter.", location: StoryManager.CuttableFenceTile));
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            text = $"You understand parts of the message. The voice mentions a fence cutter and {StoryManager.ClosestAreaOfCuttableFence.Name}.";
            Game.StartQuest(new Quest(QuestDefOf.GoToUnpoweredFence, $"The radio voice mentioned {StoryManager.ClosestAreaOfCuttableFence.Name} and a fence cutter.", area: StoryManager.ClosestAreaOfCuttableFence));
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "You can't make out anything useful from the static. The transmissions stops, maybe try again another day.";
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
            OncePerDay = true,
            Difficulty = 80,
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
            CanCriticallyFail = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Strength, 4f }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = false,
                    Item = ItemDefOf.Crowbar,
                    DifficultyReduction = 40,
                    DestructionChance = 0.1f,
                }
            }
        };
    }
    private string ForceDoor(OptionOutcomeDef outcome)
    {
        string text = "";
        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = "The door gives way. Inside: dusty equipment, a cot, and some supplies. You take them.";
            TakeItemsInside();
            IsDoorOpen = true;
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "The door holds. You sprain your shoulder.";
            Game.ApplyArmFracture(2f);
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
            OncePerDay = true,
            Difficulty = 70,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Agility, 2f },
                { StatDefOf.Strength, 2f },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = false,
                    Item = ItemDefOf.Rope,
                    DifficultyReduction = 30,
                }
            }
        };
    }
    private string ClimbTower(OptionOutcomeDef outcome)
    {
        string text = "";

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
            Game.ApplyBruiseDamage(2f);
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            text = "You slip while climbing and fall to the ground, hurting yourself badly.";
            Game.ApplyBruiseDamage(5f);
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
