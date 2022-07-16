using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E008_DistressedPerson : Event
{
    // Static
    private const float BaseProbability = 4f;

    public static float GetProbability(Game game)
    {
        return BaseProbability;
    }

    // Instance
    public E008_DistressedPerson(Game game) : base(game, EventType.E008_DistressedPerson) { }

    public override void InitEvent()
    {
        // Attributes
        ItemActionsAllowed = false;

        // Sprites
        ResourceManager.Singleton.E008_DistressedPerson.SetActive(true);

        // Options
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Ask what's wrong
        dialogueOptions.Add(new EventOption("Ask what's wrong", Ask));

        // Event
        string eventText = "You see a very distressed person who is flailing their arms around.";
        InitialStep = new EventStep(eventText, null, null, dialogueOptions, itemOptions);
    }

    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E008_DistressedPerson.SetActive(false);
    }

    private EventStep Ask()
    {
        return new EventStep("The person doesn't react. There's appearently nothing you can do.", null, null, null, null);
    }


}
