using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E010_QuarantineFence : Event
{
    // Static
    public override int Id => 10;

    private static float CLIMB_BASE_CHANCE = 0.05f;

    // Instance
    public E010_QuarantineFence(Game game) : base(game) { }
    public override Event GetEventInstance => new E010_QuarantineFence(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E010_FenceForeground);
        ShowEventSprite(ResourceManager.Singleton.E010_FenceBackground);
    }
    protected override EventStep GetInitialStep()
    {
        string eventText = "You approach the fence that stands between you and the safety of the outside world.";
        return GetInitialStep(eventText, "Climb the fence");
    }

    // Steps
    private EventStep GetInitialStep(string eventText, string climbText)
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Climb
        dialogueOptions.Add(new EventDialogueOption(climbText, Climb));

        // Dialogue Option - Give up
        dialogueOptions.Add(new EventDialogueOption("Give up for today", GiveUp));

        // Event
        return new EventStep(eventText, dialogueOptions, itemOptions);
    }

    private EventStep Climb()
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

    private EventStep GiveUp()
    {
        return new EventStep("Today is not the day to escape. You decide to try again another time.");
    }
}
