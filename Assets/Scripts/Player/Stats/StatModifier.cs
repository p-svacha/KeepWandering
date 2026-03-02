using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier
{
    public string Name { get; private set; }
    public int Value { get; private set; }

    public StatModifier(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public Color GetValueColor()
    {
        if (Value == 0) return ResourceManager.Color_Text_Default;

        if (Value <= -10) return ResourceManager.Color_Text_ExtremelyNegative;
        if (Value <= -5) return ResourceManager.Color_Text_VeryNegative;
        if (Value < 0) return ResourceManager.Color_Text_Negative;

        if (Value >= 10) return ResourceManager.Color_Text_ExtremelyPositive;
        if (Value >= 5) return ResourceManager.Color_Text_VeryPositive;
        if (Value > 0) return ResourceManager.Color_Text_Positive;
        throw new System.Exception("Value " + Value + " not handled.");
    }
}
