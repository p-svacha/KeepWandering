using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventOption
{
    public string Text;
    public Func<Game, EventStep> Action; // Action that gets executed when chosing this option. Must return the next EventStep in the Event.

    public EventOption(string text, Func<Game, EventStep> action)
    {
        Text = text;
        Action = action;
    }
}
