using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public string Name;
    public string Description;
    public Color TextColor;
    public Color BackgroundColor;

    public StatusEffect(string name, string description, Color textColor, Color backgroundColor)
    {
        Name = name;
        Description = description;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
    }
}
