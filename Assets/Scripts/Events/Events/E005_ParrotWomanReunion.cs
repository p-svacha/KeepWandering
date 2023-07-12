using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E005_ParrotWomanReunion : Event
{
    // Static
    public override int Id => 5;
    private const int MinDaysForReunion = 3;

    private const int NUM_REWARDS = 3;
    private static LootTable RewardTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.WaterBottle, 10),
        new(ItemType.Coin, 10),
        new(ItemType.Bandage, 5),
        new(ItemType.Antibiotics, 5)
    );

    // Instance
    public E005_ParrotWomanReunion(Game game) : base(game) { }
    public override Event GetEventInstance => new E005_ParrotWomanReunion(Game);

    // Base
    public override float GetEventProbability()
    {
        if (HasOccuredAlready) return 0f;
        if (!Game.EventManager.HasEncounteredEvent(eventId: 004) || Game.EventManager.DaysSinceLastEventOccurence(eventId: 004) < MinDaysForReunion || !E004_ParrotWoman.HasAcceptedParrot) return 0f;
        if (Game.CurrentPosition.Location != E004_ParrotWoman.EncounterLocation) return 0f;
        else return 1f * (Game.EventManager.DaysSinceLastEventOccurence(eventId: 004) - MinDaysForReunion + 1);
    }
    protected override void OnEventStart()
    {
        // Sprite
        ShowEventSprite(ResourceManager.Singleton.E004_Woman);
    }
    protected override EventStep GetInitialStep()
    {
        // Options
        string eventText = "";
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        if (Game.Player.HasParrot)
        {
            eventText = "You encounter " + E004_ParrotWoman.WomanName + " again and she's overcome with joy to see that her parrot is doing well.";

            // Dialogue Option - Return parrot
            dialogueOptions.Add(new EventDialogueOption("Return the parrot", ReturnParrot));
        }

        else
        {
            eventText = "You encounter " + E004_ParrotWoman.WomanName + ". Upon realizing that her parrot is gone, she just stares into the void and doesn't interact with you anymore.";

            // Dialogue Option - Continue
            dialogueOptions.Add(new EventDialogueOption("Continue", Continue));
        }

        return new EventStep(eventText, dialogueOptions, itemOptions);
    }

    private EventStep ReturnParrot()
    {
        Game.RemoveParrot();
        ShowEventSprite(ResourceManager.Singleton.E004_Parrot);
        Game.RemoveMission(MissionId.E004);
        string text = E004_ParrotWoman.WomanName + " looks happy to be reunited with her parrot. As a thank you she hands you several items.";

        // Get reward
        GetLocationLootTable(RewardTable).AddItemsToInventory(NUM_REWARDS);

        return new EventStep(text);
    }
    private EventStep Continue()
    {
        string text = "You tell her you're sorry but there's nothing more you can do so you continue your journey.";
        return new EventStep(text);
    }


}
