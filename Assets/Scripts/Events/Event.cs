using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public EventType Type;
    public EventStep InitialStep;

    /// <summary>
    /// Sprites that are used for this event. These will get disabled when the event is finished.
    /// </summary>
    public List<GameObject> EventSprites { get; protected set; }

    /// <summary>
    /// Items that are used for this event and do not belong to the player. These will get destroyed when the event is finished.
    /// </summary>
    public List<Item> EventItems { get; protected set; }

    public bool ItemActionsAllowed; // If false, the player cannot do default item interactions during the event (i.e. eat, drink)

    public Event(EventType type, EventStep initialStep, List<Item> eventItems, List<GameObject> usedSprites, bool itemActionsAllowed)
    {
        Type = type;
        InitialStep = initialStep;
        EventItems = eventItems;
        EventSprites = usedSprites;
        ItemActionsAllowed = itemActionsAllowed;
    }
}
