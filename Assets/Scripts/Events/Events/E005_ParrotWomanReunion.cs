using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E005_ParrotWomanReunion : Event
{
    // Static
    private const int MinDaysForReunion = 3;
    public static bool HasEncountered;

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

    public override float GetEventProbability()
    {
        if (HasEncountered) return 0f;
        if (!E004_ParrotWoman.HasEncountered || Game.Day < E004_ParrotWoman.EncounterDay + MinDaysForReunion || !E004_ParrotWoman.HasAcceptedParrot) return 0f;
        if (Game.CurrentPosition.Location != E004_ParrotWoman.EncounterLocation) return 0f;
        else return 1f * ((Game.Day - MinDaysForReunion) - E004_ParrotWoman.EncounterDay);
    }
    public override void OnEventStart()
    {
        // Attributes
        HasEncountered = true;

        // Sprite
        ResourceManager.Singleton.E004_Woman.SetActive(true);
    }
    public override EventStep GetInitialStep()
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
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E004_Woman.SetActive(false);
        ResourceManager.Singleton.E004_Parrot.SetActive(false);
    }

    private EventStep ReturnParrot()
    {
        Game.RemoveParrot();
        ResourceManager.Singleton.E004_Parrot.SetActive(true);
        Game.RemoveMission(MissionId.M001_CareParrot);
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
