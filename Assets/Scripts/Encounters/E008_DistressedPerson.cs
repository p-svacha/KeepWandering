using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class E008_DistressedPerson : Encounter
{
    // Static
    public override int DefName => 8;

    protected override float BaseProbability => 5f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.2f},
        {LocationType.City, 1f},
        {LocationType.Woods, 0.2f},
    };

    private static float NO_REACTION_CHANCE = 0.2f;

    // Instance
    public E008_DistressedPerson(Game game) : base(game) { }
    public override Encounter GetEventInstance => new E008_DistressedPerson(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager_Old.Singleton.E008_DistressedPerson);
    }
    protected override EncounterStep GetInitialStep()
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Ask what's wrong
        dialogueOptions.Add(new EventDialogueOption("Ask what's wrong", Ask));

        // Dialogue Option - Ignore
        dialogueOptions.Add(new EventDialogueOption("Ignore", () => Ignore("You don't want to interact with him and move on.")));

        // Event
        string eventText = "You see a very distressed person who is flailing their arms around.";
        return new EncounterStep(eventText, dialogueOptions, itemOptions);
    }

    // Steps
    private EncounterStep Ask()
    {
        if(Random.value < NO_REACTION_CHANCE) return new EncounterStep("The person doesn't react. There's appearently nothing you can do.");

        // Requested item
        Item requestedItem = Game.RandomInventoryItem;

        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Don't give item
        dialogueOptions.Add(new EventDialogueOption("Ignore", () => Ignore("You don't want to give him your " + requestedItem.Name + " so you move on.")));

        // Item Option - Give item
        itemOptions.Add(new EventItemOption(requestedItem.Type, "Give", GiveItem));

        return new EncounterStep("He tells you that he is dire need of a " + requestedItem.Name + ".", dialogueOptions, itemOptions, allowDefaultItemInteractions: false);
    }

    private EncounterStep Ignore(string text)
    {
        return new EncounterStep(text);
    }

    private EncounterStep GiveItem(Item item)
    {
        Game.DestroyOwnedItem(item);

        return new EncounterStep("He thanks you vigourously and adds that he will come back to you if he'll meet you again.");
    }

}
*/