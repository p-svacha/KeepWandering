using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E005_ParrowWomanReunion : Event
{
    // Static
    private const int MinDaysForReunion = 3;

    public static bool HasEncountered;
    public static bool SuccessfulReturn;

    // Instance
    public E005_ParrowWomanReunion(Game game) : base(game) { }
    public override Event GetEventInstance => new E005_ParrowWomanReunion(Game);

    public override float GetEventProbability()
    {
        if (HasEncountered) return 0f;
        if (!E004_ParrotWoman.HasEncountered || Game.Day < E004_ParrotWoman.EncounterDay + MinDaysForReunion || !E004_ParrotWoman.HasAcceptedParrot) return 0f;
        if (Game.CurrentLocation != E004_ParrotWoman.EncounterLocation) return 0f;
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
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        if (Game.Player.HasParrot)
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

        return new EventStep(eventText, null, null, dialogueOptions, itemOptions);
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
        E006_WoodsBunker.SetRandomRequirements();
        E006_WoodsBunker.UpdateBunkerMission(Game);
        SuccessfulReturn = true;
        string text = E004_ParrotWoman.WomanName + " thanks you thoroughly. She adds that she has some friends in a safe bunker in the woods that will let you join them if you bring them food and water.";
        return new EventStep(text, null, null, null, null);
    }
    private EventStep Continue()
    {
        string text = "You tell her you're sorry but there's nothing more you can do so you continue your journey.";
        return new EventStep(text, null, null, null, null);
    }


}
