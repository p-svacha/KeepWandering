using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventOption
{
    public string Text;
    public Action Action;
    public EventStep NextStep;

    public EventOption(string text, Action action, EventStep nextStep)
    {
        Text = text;
        Action = action;
        NextStep = nextStep;
    }
}
