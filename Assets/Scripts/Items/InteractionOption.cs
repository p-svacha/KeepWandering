using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionOption
{
    public string Text { get; private set; }
    public Action Action { get; private set; }
    public Action OnHoverStartAction { get; private set; }
    public Action OnHoverEndAction { get; private set; }


    public InteractionOption(string text, Action action, Action onHoverStartAction = null, Action onHoverEndAction = null)
    {
        Text = text;
        Action = action;
        OnHoverStartAction = onHoverStartAction;
        OnHoverEndAction = onHoverEndAction;
    }
}
