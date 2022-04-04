using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public EventType Type;
    public EventStep InitialStep;

    public List<GameObject> EventSprites;
    public List<Item> EventItems;

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
