using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E005_ParrowWomanReunion : Event
{
    private const int MinDaysForReunion = 3;

    public static bool HasEncountered;
    public static bool SuccessfulReturn;

    public static float GetProbability(Game game)
    {
        if (HasEncountered) return 0f;
        if (!E004_ParrotWoman.HasEncountered || game.Day < E004_ParrotWoman.EncounterDay + MinDaysForReunion || !E004_ParrotWoman.HasAcceptedParrot) return 0f;
        if (game.CurrentLocation != E004_ParrotWoman.EncounterLocation) return 0f;
        else return 1f * ((game.Day - MinDaysForReunion) - E004_ParrotWoman.EncounterDay);
    }

    public static E005_ParrowWomanReunion GetEventInstance(Game game)
    {
        // Attributes
        HasEncountered = true;

        // Sprite
        ResourceManager.Singleton.E004_Woman.SetActive(true);

        // Options
        string eventText = "";
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        if(game.Player.HasParrot)
        {
            eventText = "You encounter " + E004_ParrotWoman.WomanName + " again and she's overcome with joy to see that her parrot is doing well.";

            // Dialogue Option - Return parrot
            dialogueOptions.Add(new EventOption("Return the parrot", ReturnParrot));
        }

        else
        {
            eventText = "You encounter " + E004_ParrotWoman.WomanName + ". Upon realizing that her parrot is gone, she just stares into the void and doesn't interact with you anymore.";

            // Dialogue Option - Continue
            dialogueOptions.Add(new EventOption("Continue", Continue));
        }

        // Event
        EventStep initialStep = new EventStep(eventText, dialogueOptions, itemOptions);
        return new E005_ParrowWomanReunion(initialStep);
    }

    private static EventStep ReturnParrot(Game game)
    {
        game.RemoveParrot();
        game.RemoveMission(MissionId.M001_CareParrot);
        E006_WoodsBunker.SetRandomRequirements();
        E006_WoodsBunker.UpdateBunkerMission(game);
        SuccessfulReturn = true;
        return new EventStep(E004_ParrotWoman.WomanName + " thanks you thoroughly. She adds that she has some friends in a safe bunker in the woods that will let you join them if you bring them food and water.", null, null);
    }

    private static EventStep Continue(Game game)
    {
        return new EventStep("You tell her you're sorry but there's nothing more you can do so you continue your journey.", null, null);
    }

    public E005_ParrowWomanReunion(EventStep initialStep) : base(
    EventType.E005_ParrotWomanReunion,
    initialStep,
    new List<Item>() { },
    new List<GameObject>() { ResourceManager.Singleton.E004_Woman, ResourceManager.Singleton.E004_Parrot },
    itemActionsAllowed: true)
    { }
}
