using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventItemOption
{
    public ItemType RequiredItemType;
    public string Text;
    public Func<Item, EventStep> Action;

    public EventItemOption(ItemType requiredItem, string text, Func<Item, EventStep> action)
    {
        RequiredItemType = requiredItem;
        Text = text;
        Action = action;
    }
}
