using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General class representing an option for the player to interact with. Used for item context menus.
/// </summary>
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