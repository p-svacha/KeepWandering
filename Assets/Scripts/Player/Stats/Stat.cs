using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    private const int BASE_VALUE = 0;

    protected Game Game;
    public StatDef Def { get; private set; }

    public string Label => Def.Label;
    public string Description => Def.Description;

    public Stat(Game game, StatDef def)
    {
        Game = game;
        Def = def;
    }

    public int GetValue()
    {
        int value = BASE_VALUE;
        foreach(StatModifier mod in GetModifiers()) value += mod.Value;
        return value;
    }

    /// <summary>
    /// Returns the deviation of the stat value from the base value.
    /// </summary>
    public int GetRelativeValue()
    {
        return GetValue() - BASE_VALUE;
    }

    public List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();



        return modifiers;
    }

    public Color GetValueColor()
    {
        int value = GetValue();

        if (value == 0) return ResourceManager.Color_Text_Default;

        if (value < -20) return ResourceManager.Color_Text_ExtremelyNegative;
        if (value < -10) return ResourceManager.Color_Text_VeryNegative;
        if (value < 0) return ResourceManager.Color_Text_Negative;

        if (value > 20) return ResourceManager.Color_Text_ExtremelyPositive;
        if (value > 10) return ResourceManager.Color_Text_VeryPositive;
        if (value > 0) return ResourceManager.Color_Text_Positive;
        throw new System.Exception("Value " + value + " not handled.");
    }
}
