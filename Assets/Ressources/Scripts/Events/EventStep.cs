using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventStep
{
    public string Text;
    public List<EventOption> EventDialogueOptions;
    public List<EventItemOption> EventItemOptions;

    public EventStep(string text, List<EventOption> dialogueOptions, List<EventItemOption> itemOptions)
    {
        Text = text;
        EventDialogueOptions = dialogueOptions;
        EventItemOptions = itemOptions;
        if (EventItemOptions == null) EventItemOptions = new List<EventItemOption>();
    }
}
