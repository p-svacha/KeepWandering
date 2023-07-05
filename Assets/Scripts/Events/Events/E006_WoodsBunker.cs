using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E006_WoodsBunker : Event
{
    // Static
    public static int RequiredFood;
    public static int RequiredWater;

    public static bool HasEnteredBunker;

    public static void SetRandomRequirements()
    {
        RequiredFood = Random.Range(1, 4);
        RequiredWater = Random.Range(1, 4);
    }

    public static void UpdateBunkerMission(Game game)
    {
        game.AddOrUpdateMission(MissionId.M002_FindWoodsBunker, "Find Pams friends' bunker in the woods and bring them food (" + RequiredFood + ") and water (" + RequiredWater + ").");
    }

    // Instance
    public E006_WoodsBunker(Game game) : base(game) { }
    public override Event GetEventInstance => new E006_WoodsBunker(Game);

    public override float GetEventProbability()
    {
        if (Game.CurrentLocation != ResourceManager.Singleton.LOC_Woods) return 0;
        if (!E005_ParrowWomanReunion.SuccessfulReturn) return 0;
        return 2f;
    }
    public override void OnEventStart()
    {
        // Sprites
        ResourceManager.Singleton.E006_WoodsBunker.SetActive(true);
    }
    public override EventStep GetInitialStep()
    {
        string eventText = "You come across the bunker that " + E004_ParrotWoman.WomanName + " told you about.";
        if (RequiredFood > 0 || RequiredWater > 0) eventText += " A voice from inside assures you that they will let you in if you give them enough food and water.";
        return GetInitialStep(eventText);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E006_WoodsBunker.SetActive(false);
    }

    private EventStep GetInitialStep(string eventText)
    {
        // Options
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Enter Bunker
        if (RequiredFood == 0 && RequiredWater == 0)
        {
            dialogueOptions.Add(new EventOption("Enter Bunker", EnterBunker));
        }

        // Dialogue Option - Continue
        dialogueOptions.Add(new EventOption("Ignore Bunker", Continue));

        // Item Option - Give Food/Water
        List<ItemType> handledTypes = new List<ItemType>();
        foreach (Item item in Game.Inventory)
        {
            if (RequiredFood > 0 && item.IsEdible && !handledTypes.Contains(item.Type))
            {
                handledTypes.Add(item.Type);
                itemOptions.Add(new EventItemOption(item.Type, "Give to bunker", GiveFood));
            }
            if (RequiredWater > 0 && item.IsDrinkable && !handledTypes.Contains(item.Type))
            {
                handledTypes.Add(item.Type);
                itemOptions.Add(new EventItemOption(item.Type, "Give to bunker", GiveWater));
            }
        }

        // Event
        return new EventStep(eventText, null, null, dialogueOptions, itemOptions);
    }
    private EventStep EnterBunker()
    {
        HasEnteredBunker = true;
        Game.CheckGameOver();
        return null;
    }
    private EventStep Continue()
    {
        return new EventStep("You walk past the bunker.", null, null, null, null);
    }
    private EventStep GiveFood(Item item)
    {
        Game.DestroyOwnedItem(item);
        RequiredFood--;
        UpdateBunkerMission(Game);
        EventStep nextStep = GetInitialStep("You gave the " + item.Name + " to the bunker.");
        nextStep.RemovedItems = new List<Item>() { item };
        return nextStep;
    }
    private EventStep GiveWater(Item item)
    {
        Game.DestroyOwnedItem(item);
        RequiredWater--;
        UpdateBunkerMission(Game);
        EventStep nextStep = GetInitialStep("You gave the " + item.Name + " to the bunker.");
        nextStep.RemovedItems = new List<Item>() { item };
        return nextStep;
    }
}
