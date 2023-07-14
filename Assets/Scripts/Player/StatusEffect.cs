using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public string Name;
    public string Description;
    public Color TextColor;
    public Color BackgroundColor;

    public UI_StatusEffect UI;

    public StatusEffect(string name, string description, Color textColor, Color? backgroundColor = null)
    {
        Name = name;
        Description = description;
        TextColor = textColor;
        BackgroundColor = backgroundColor ?? Color.clear;
    }

    public StatusEffect(StatusEffect copyTemplate)
    {
        Name = copyTemplate.Name;
        Description = copyTemplate.Description;
        TextColor = copyTemplate.TextColor;
        BackgroundColor = copyTemplate.BackgroundColor;
    }
}
