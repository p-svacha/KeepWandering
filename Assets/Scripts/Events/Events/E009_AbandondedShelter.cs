using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class E009_AbandondedShelter : Encounter
{
    // Static
    public override int DefName => 9;
    protected override float BaseProbability => 5f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 1.2f},
        {LocationType.City, 0.5f},
        {LocationType.Woods, 1.2f},
    };

    private static int MIN_ITEMS = 1;
    private static int MAX_ITEMS = 4;
    private static LootTable ItemTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.WaterBottle, 9),
        new(ItemType.MedicalKit, 8)
        );

    private static Dictionary<int, float> WindowCutsTable = new Dictionary<int, float>()
    {
        {0, 45 },
        {1, 35 },
        {2, 20 }
    };

    private static float TRIGGER_TRAP_CHANCE = 0.2f;

    // Instance
    private bool IsTrapTriggered;
    public E009_AbandondedShelter(Game game) : base(game) { }
    public override Encounter GetEventInstance => new E009_AbandondedShelter(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager_Old.Singleton.E009_Shelter);
        ShowEventSprite(ResourceManager_Old.Singleton.E009_TrapOpen);
    }
    protected override EncounterStep GetInitialStep()
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Enter through window
        dialogueOptions.Add(new EventDialogueOption("Enter through broken window", EnterWindow));

        // Dialogue Option - Enter door
        dialogueOptions.Add(new EventDialogueOption("Enter through trapped door", EnterDoor));

        // Dialogue Option - Ignore
        dialogueOptions.Add(new EventDialogueOption("Ignore and move on", Ignore));

        // Event
        string eventText = "You stumble upon an abandoned shelter. It looks like you could scavenge it for supplies, but entering won't be easy.";
        return new EncounterStep(eventText, dialogueOptions, itemOptions);
    }

    // Steps
    private EncounterStep Ignore()
    {
        return new EncounterStep("You decide it's better to leave everything as it is and keep wandering.");
    }


    private EncounterStep EnterWindow()
    {
        int numCuts = HelperFunctions.GetWeightedRandomElement(WindowCutsTable);
        if (numCuts == 0)
        {
            string text = "You manage to enter through the window unscathed and gather everything you find.";
            return GetLeaveShelterStep(text);
        }
        else
        {
            CutOnWindows(numCuts);

            string twiceText = numCuts == 2 ? "twice " : "";
            string text = "You enter the shelter but cut yourself " + twiceText + "on the broken window. You gather everything you can find.";
            return GetLeaveShelterStep(text);
        }
    }

    private EncounterStep EnterDoor()
    {
        if (Random.value < TRIGGER_TRAP_CHANCE)
        {
            TriggerTrap();

            string text = "Even with great caution you touch the bear trap and it rips your leg right off. Almost dying of pain you still gather everything that's inside.";
            return GetLeaveShelterStep(text);
        }
        else
        {
            string text = "You elegantly avoid the bear trap in front of the door and gather everything you can find inside.";
            return GetLeaveShelterStep(text);
        }
    }

    private EncounterStep GetLeaveShelterStep(string text)
    {
        // Add items from shelter
        List<Item> items = GetLocationLootTable(ItemTable).AddItemsToInventory(MIN_ITEMS, MAX_ITEMS);

        // Hide character
        ResourceManager_Old.Singleton.PlayerCharacter.gameObject.SetActive(false);

        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Leave through window
        dialogueOptions.Add(new EventDialogueOption("Window", LeaveWindow));

        // Dialogue Option - Leave through door
        dialogueOptions.Add(new EventDialogueOption("Door", LeaveDoor));

        // Event
        text += "\nHow would you like to leave the shelter?";
        return new EncounterStep(text, dialogueOptions, itemOptions);
    }

    private EncounterStep LeaveWindow()
    {
        // Show character
        ResourceManager_Old.Singleton.PlayerCharacter.gameObject.SetActive(true);

        int numCuts = HelperFunctions.GetWeightedRandomElement(WindowCutsTable);
        if (numCuts == 0)
        {
            string text = "You manage to leave through the window unscathed and are ready to move on.";
            return new EncounterStep(text);
        }
        else
        {
            CutOnWindows(numCuts);

            string twiceText = numCuts == 2 ? "twice " : "";
            string text = "You leave the shelter but cut yourself " + twiceText + "on the broken window. You are ready to move on.";
            return new EncounterStep(text);
        }
    }
    private EncounterStep LeaveDoor()
    {
        // Show character
        ResourceManager_Old.Singleton.PlayerCharacter.gameObject.SetActive(true);

        if (IsTrapTriggered) return new EncounterStep("You walk past the already triggered trap and move on.");
        else
        {
            if (Random.value < TRIGGER_TRAP_CHANCE)
            {
                TriggerTrap();

                string text = "You are not careful enough and step right into the trap. It rips your leg right off. In awful pain you move on.";
                return new EncounterStep(text);
            }
            else
            {
                string text = "You elegantly avoid the bear trap in front of the door and are ready to move on.";
                return new EncounterStep(text);
            }
        }
    }

    private void CutOnWindows(int numCuts)
    {
        for (int i = 0; i < numCuts; i++) Game.AddCutWound();
        ShowEventSprite(ResourceManager_Old.Singleton.E009_WindowBlood);
    }

    private void TriggerTrap()
    {
        Game.AddBruiseWound();
        IsTrapTriggered = true;
        HideEventSprite(ResourceManager_Old.Singleton.E009_TrapOpen);
        ShowEventSprite(ResourceManager_Old.Singleton.E009_TrapClosed);
    }
}
*/