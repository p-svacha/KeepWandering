using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class Encounter_ParrotWoman : Encounter
{
    // Static
    public override int DefName => 4;
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
    public Encounter_ParrotWoman(Game game) : base(game) { }
    public override Encounter GetEventInstance => new Encounter_ParrotWoman(Game);

    // Base
    protected override void OnEventStart()
    {
        // Attributes
        EncounterLocation = Game.CurrentPosition.Location;

        // Sprites
        ShowEventSprite(ResourceManager_Old.Singleton.E004_Woman);
        ShowEventSprite(ResourceManager_Old.Singleton.E004_Parrot);
    }
    protected override EncounterStep GetInitialStep()
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
        return new EncounterStep(eventText, dialogueOptions, itemOptions);
    }

    private EncounterStep AcceptParrot()
    {
        HasAcceptedParrot = true;
        Game.AddParrot();
        Game.AddMission(new Mission(MissionId.E004, "Take care of parrot until meeting " + WomanName + " again in the " + EncounterLocation.Name + "."));
        HideEventSprite(ResourceManager_Old.Singleton.E004_Parrot);
        string text = "You promise " + WomanName + " to take care of the parrot. She asks you to take good care of him.";
        return new EncounterStep(text);
    }
    private EncounterStep RefuseParrot()
    {
        return new EncounterStep("You refuse to take care of the parrot.");
    }
}
*/
