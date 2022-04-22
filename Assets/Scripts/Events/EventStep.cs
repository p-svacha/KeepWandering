using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventStep
{
    public string Text;

    /// <summary>
    /// Items that were added to the inventory with this step
    /// </summary>
    public List<Item> AddedItems { get; set; }

    /// <summary>
    /// Items that were removed from the inventory with this step
    /// </summary>
    public List<Item> RemovedItems { get; set; }

    public List<EventOption> EventDialogueOptions;
    public List<EventItemOption> EventItemOptions;

    public EventStep(string text, List<Item> addedItems, List<Item> removedItems, List<EventOption> dialogueOptions, List<EventItemOption> itemOptions)
    {
        Text = text;
        AddedItems = addedItems;
        RemovedItems = removedItems;
        EventDialogueOptions = dialogueOptions;
        EventItemOptions = itemOptions;
        if (EventItemOptions == null) EventItemOptions = new List<EventItemOption>();
    }
}
