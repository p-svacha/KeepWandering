using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventItemOption
{
    public ItemType RequiredItemType;
    public string Text;
    public Action<Game, Item> Action;
    public EventStep NextStep;

    public EventItemOption(ItemType requiredItem, string text, Action<Game, Item> action, EventStep nextStep)
    {
        RequiredItemType = requiredItem;
        Text = text;
        Action = action;
        NextStep = nextStep;
    }
}
