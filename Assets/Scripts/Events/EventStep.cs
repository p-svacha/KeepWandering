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

    public List<EventDialogueOption> EventDialogueOptions;
    public List<EventItemOption> EventItemOptions;
    public bool ItemsAllowed;

    public EventStep(string text, List<Item> addedItems, List<Item> removedItems, List<EventDialogueOption> dialogueOptions, List<EventItemOption> itemOptions, bool allowItems = true)
    {
        Text = text;
        AddedItems = addedItems;
        RemovedItems = removedItems;
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
