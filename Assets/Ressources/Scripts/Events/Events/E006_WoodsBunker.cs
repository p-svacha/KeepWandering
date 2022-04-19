using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E006_WoodsBunker : Event
{
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
        game.AddOrUpdateMission(MissionId.M002_FindWoodsBunker, "Find the Pams friends' bunker in the woods and bring them " + RequiredFood + " food and " + RequiredWater + " water");
    }

    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation != ResourceManager.Singleton.LOC_Woods) return 0;
        if (!E005_ParrowWomanReunion.SuccessfulReturn) return 0;
        return 2f;
    }

    public static E006_WoodsBunker GetEventInstance(Game game)
    {
        // Sprite
        ResourceManager.Singleton.E006_WoodsBunker.SetActive(true);

        // Event
        string eventText = "You come across the bunker that " + E004_ParrotWoman.WomanName + " told you about.";
        if (RequiredFood > 0 || RequiredWater > 0) eventText += " A voice from inside assures you that they will let you in if you give them enough food and water.";
        return new E006_WoodsBunker(GetInitialStep(game, eventText));
    }

    private static EventStep EnterBunker(Game game)
    {
        HasEnteredBunker = true;
        game.CheckGameOver();
        return null;
    }

    private static EventStep Continue(Game game)
    {
        return new EventStep("You walk past the bunker.", null, null, null, null);
    }

    private static EventStep GiveFood(Game game, Item item)
    {
        RequiredFood--;
        UpdateBunkerMission(game);
        EventStep nextStep = GetInitialStep(game, "You gave the " + item.Name + " to the bunker.");
        nextStep.RemovedItems = new List<Item>() { item };
        return nextStep;
    }

    private static EventStep GiveWater(Game game, Item item)
    {
        RequiredWater--;
        UpdateBunkerMission(game);
        EventStep nextStep = GetInitialStep(game, "You gave the " + item.Name + " to the bunker.");
        nextStep.RemovedItems = new List<Item>() { item };
        return nextStep;
    }

    private static EventStep GetInitialStep(Game game, string eventText)
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
        foreach(Item item in game.Inventory)
        {
            if(RequiredFood > 0 && item.IsEdible && !handledTypes.Contains(item.Type))
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

    public E006_WoodsBunker(EventStep initialStep) : base(
        EventType.E006_WoodsBunker,
        initialStep,
        new List<Item>() { },
        new List<GameObject>() { ResourceManager.Singleton.E006_WoodsBunker},
        itemActionsAllowed: true)
    { }
}
