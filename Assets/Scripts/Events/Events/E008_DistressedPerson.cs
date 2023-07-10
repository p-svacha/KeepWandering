using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E008_DistressedPerson : Event
{
    // Static
    private const float BaseProbability = 4f;

    // Instance
    public E008_DistressedPerson(Game game) : base(game) { }
    public override Event GetEventInstance => new E008_DistressedPerson(Game);

    // Base
    public override float GetEventProbability()
    {
        return BaseProbability;
    }
    public override void OnEventStart()
    {
        // Sprites
        ResourceManager.Singleton.E008_DistressedPerson.SetActive(true);
    }
    public override EventStep GetInitialStep()
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Ask what's wrong
        dialogueOptions.Add(new EventDialogueOption("Ask what's wrong", Ask));

        // Event
        string eventText = "You see a very distressed person who is flailing their arms around.";
        return new EventStep(eventText, dialogueOptions, itemOptions);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E008_DistressedPerson.SetActive(false);
    }

    // Steps
    private EventStep Ask()
    {
        return new EventStep("The person doesn't react. There's appearently nothing you can do.", null, null);
    }


}
