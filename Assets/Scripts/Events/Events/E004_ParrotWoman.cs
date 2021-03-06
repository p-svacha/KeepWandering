using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E004_ParrotWoman : Event
{
    // Static
    public const string WomanName = "Pam";

    public static bool HasEncountered;
    public static int EncounterDay;
    public static Location EncounterLocation;
    public static bool HasAcceptedParrot;

    public static float GetProbability(Game game)
    {
        if (HasEncountered) return 0f;
        return 2f;
    }

    // Instance
    public E004_ParrotWoman(Game game) : base(game, EventType.E004_ParrotWoman) { }

    public override void InitEvent()
    {
        // Attributes
        ItemActionsAllowed = true;
        HasEncountered = true;
        EncounterDay = Game.Day;
        EncounterLocation = Game.CurrentLocation;

        // Sprite
        ResourceManager.Singleton.E004_Woman.SetActive(true);
        ResourceManager.Singleton.E004_Parrot.SetActive(true);

        // Init Event
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Accept
        dialogueOptions.Add(new EventOption("Take the parrot", AcceptParrot));

        // Dialogie Option - Refuse
        dialogueOptions.Add(new EventOption("Refuse to take the parrot", RefuseParrot));

        // Event
        string eventText = "You encounter a woman called " + WomanName + " with a parrot on her shoulder. She asks you to take care of it for a while and then meet her again in the " + Game.CurrentLocation.Name + ". She adds that the parrot is a very picky eater and will only accept nuts.";
        InitialStep = new EventStep(eventText, null, null, dialogueOptions, itemOptions);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E004_Woman.SetActive(false);
        ResourceManager.Singleton.E004_Parrot.SetActive(false);
    }

    private EventStep AcceptParrot()
    {
        HasAcceptedParrot = true;
        Game.AddParrot();
        Game.AddOrUpdateMission(MissionId.M001_CareParrot, "Take care of parrot until meeting " + WomanName + " again in the " + EncounterLocation.Name + ".");
        ResourceManager.Singleton.E004_Parrot.SetActive(false);
        string text = "You promise " + WomanName + " to take care of the parrot. She asks you to take good care of him.";
        return new EventStep(text, null, null, null, null);
    }
    private EventStep RefuseParrot()
    {
        return new EventStep("You refuse to take care of the parrot.", null, null, null, null);
    }


}
