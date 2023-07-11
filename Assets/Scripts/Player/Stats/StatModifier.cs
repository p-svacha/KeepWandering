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
        if (Value == 0) return ResourceManager.Singleton.SE_Neutral;

        if (Value < -20) return ResourceManager.Singleton.SE_ExtremelyBad;
        if (Value < -10) return ResourceManager.Singleton.SE_VeryBad;
        if (Value < 0) return ResourceManager.Singleton.SE_Bad;

        if (Value > 20) return ResourceManager.Singleton.SE_ExtremelyGood;
        if (Value > 10) return ResourceManager.Singleton.SE_VeryGood;
        if (Value > 0) return ResourceManager.Singleton.SE_Good;

        throw new System.Exception("Value " + Value + " not handled.");
    }
}
