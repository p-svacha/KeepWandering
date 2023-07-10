using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E004_ParrotWoman : Event
{
    // Static
    public override int Id => 4;
    protected override float BaseProbability => 2f;
    protected override bool CanOnlyOccurOnce => true;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.8f},
        {LocationType.City, 1f},
        {LocationType.Woods, 0.8f},
    };

    public const string WomanName = "Pam";

    public static Location EncounterLocation;
    public static bool HasAcceptedParrot;

    // Instance
    public E004_ParrotWoman(Game game) : base(game) { }
    public override Event GetEventInstance => new E004_ParrotWoman(Game);

    // Base
    protected override void OnEventStart()
    {
        // Attributes
        EncounterLocation = Game.CurrentPosition.Location;

        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E004_Woman);
        ShowEventSprite(ResourceManager.Singleton.E004_Parrot);
    }
    protected override EventStep GetInitialStep()
    {
        // Dialogue Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        

        // Dialogue Option - Accept
        dialogueOptions.Add(new EventDialogueOption("Take the parrot", AcceptParrot));

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();
        dialogueOptions.Add(new EventDialogueOption("Refuse to take the parrot", RefuseParrot));

        // Event
        string eventText = "You encounter a woman called " + WomanName + " with a parrot on her shoulder. She asks you to take care of it for a while and then meet her again in the " + Game.CurrentPosition.Location.Name + ". She adds that the parrot is a very picky eater and will only accept nuts.";
        return new EventStep(eventText, dialogueOptions, itemOptions);
    }

    private EventStep AcceptParrot()
    {
        HasAcceptedParrot = true;
        Game.AddParrot();
        Game.AddOrUpdateMission(MissionId.M001_CareParrot, "Take care of parrot until meeting " + WomanName + " again in the " + EncounterLocation.Name + ".");
        HideEventSprite(ResourceManager.Singleton.E004_Parrot);
        string text = "You promise " + WomanName + " to take care of the parrot. She asks you to take good care of him.";
        return new EventStep(text);
    }
    private EventStep RefuseParrot()
    {
        return new EventStep("You refuse to take care of the parrot.");
    }


}
