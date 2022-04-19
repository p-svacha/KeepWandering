using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E004_ParrotWoman : Event
{
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

    public static E004_ParrotWoman GetEventInstance(Game game)
    {
        // Attributes
        HasEncountered = true;
        EncounterDay = game.Day;
        EncounterLocation = game.CurrentLocation;

        // Sprite
        ResourceManager.Singleton.E004_Woman.SetActive(true);
        ResourceManager.Singleton.E004_Parrot.SetActive(true);

        // Options
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Accept
        dialogueOptions.Add(new EventOption("Take the parrot", AcceptParrot));

        // Dialogie Option - Refuse
        dialogueOptions.Add(new EventOption("Refuse to take the parrot", RefuseParrot));

        // Event
        string eventText = "You encounter a woman called " + WomanName + " with a parrot on her shoulder. She asks you to take care of it for a while and then meet her again in the " + game.CurrentLocation.Name + ". She adds that the parrot is a very picky eater and will only accept nuts.";
        EventStep initialStep = new EventStep(eventText, dialogueOptions, itemOptions);
        return new E004_ParrotWoman(initialStep);
    }

    private static EventStep AcceptParrot(Game game)
    {
        HasAcceptedParrot = true;
        game.AddParrot();
        game.AddOrUpdateMission(MissionId.M001_CareParrot, "Take care of parrot until meeting " + WomanName + " again in the " + EncounterLocation.Name + ".");
        ResourceManager.Singleton.E004_Parrot.SetActive(false);
        return new EventStep("You promise " + WomanName + " to take care of the parrot. She asks you to take good care of him.", null, null);
    }

    private static EventStep RefuseParrot(Game game)
    {
        return new EventStep("You refuse to take care of the parrot.", null, null);
    }

    public E004_ParrotWoman(EventStep initialStep) : base(
        EventType.E004_ParrotWoman,
        initialStep,
        new List<Item>() { },
        new List<GameObject>() { ResourceManager.Singleton.E004_Woman, ResourceManager.Singleton.E004_Parrot },
        itemActionsAllowed: true)
    { }
}
