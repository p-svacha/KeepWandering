using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class E010_QuarantineFence : Encounter
{
    // Static
    public override int DefName => 10;

    private static float CLIMB_BASE_CHANCE = 0.05f;

    // Instance
    public E010_QuarantineFence(Game game) : base(game) { }
    public override Encounter GetEventInstance => new E010_QuarantineFence(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager_Old.Singleton.E010_FenceForeground);
        ShowEventSprite(ResourceManager_Old.Singleton.E010_FenceBackground);
    }
    protected override EncounterStep GetInitialStep()
    {
        string eventText = "You approach the fence that stands between you and the safety of the outside world.";
        return GetInitialStep(eventText, "Climb the fence");
    }

    // Steps
    private EncounterStep GetInitialStep(string eventText, string climbText)
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Climb
        dialogueOptions.Add(new EventDialogueOption(climbText, Climb));

        // Dialogue Option - Give up
        dialogueOptions.Add(new EventDialogueOption("Give up for today", GiveUp));

        // Event
        return new EncounterStep(eventText, dialogueOptions, itemOptions);
    }

    private EncounterStep Climb()
    {
        if(Random.value < CLIMB_BASE_CHANCE)
        {
            Game.SetPosition(Game.TargetPosition);
            return null;
        }
        else
        {
            Game.AddCutWound();
            return GetInitialStep("You cut yourself trying to climb it.", "Try again");
        }
    }

    private EncounterStep GiveUp()
    {
        return new EncounterStep("Today is not the day to escape. You decide to try again another time.");
    }
}
*/