using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E008_DistressedPerson : Event
{
    // Static
    public override int Id => 8;

    protected override float BaseProbability => 5f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.2f},
        {LocationType.City, 1f},
        {LocationType.Woods, 0.2f},
    };

    // Instance
    public E008_DistressedPerson(Game game) : base(game) { }
    public override Event GetEventInstance => new E008_DistressedPerson(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E008_DistressedPerson);
    }
    protected override EventStep GetInitialStep()
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

    // Steps
    private EventStep Ask()
    {
        return new EventStep("The person doesn't react. There's appearently nothing you can do.", null, null);
    }


}
