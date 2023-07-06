using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueOption
{
    public string Text;
    public Func<EventStep> Action; // Action that gets executed when chosing this option. Must return the next EventStep in the Event.

    public EventDialogueOption(string text, Func<EventStep> action)
    {
        Text = text;
        Action = action;
    }
}
