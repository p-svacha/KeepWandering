using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventStep
{
    public string Text;

    public List<EventDialogueOption> EventDialogueOptions;
    public List<EventItemOption> EventItemOptions;
    public bool ItemsAllowed;

    public EventStep(string text, List<EventDialogueOption> dialogueOptions = null, List<EventItemOption> itemOptions = null, bool allowItems = true)
    {
        Text = text;
        EventDialogueOptions = dialogueOptions;
        EventItemOptions = itemOptions;
        ItemsAllowed = allowItems;
        if (EventDialogueOptions == null) EventDialogueOptions = new List<EventDialogueOption>();
        if (EventItemOptions == null) EventItemOptions = new List<EventItemOption>();
    }

    /// <summary>
    /// If this is the final step of the event, meaning that there are no more interaction options.
    /// </summary>
    public bool IsFinalStep => EventDialogueOptions.Count == 0;
}
