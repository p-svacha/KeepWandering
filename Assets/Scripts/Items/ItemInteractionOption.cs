using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractionOption
{
    public string Text;
    public Action Action;
    public Action OnHoverStartAction;
    public Action OnHoverEndAction;


    public ItemInteractionOption(string text, Action action, Action onHoverStartAction = null, Action onHoverEndAction = null)
    {
        Text = text;
        Action = action;
        OnHoverStartAction = onHoverStartAction;
        OnHoverEndAction = onHoverEndAction;
    }
}
