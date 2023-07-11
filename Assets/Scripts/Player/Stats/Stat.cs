using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stat
{
    private const int BASE_VALUE = 100;

    protected Game Game;
    public abstract StatId Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }

    public Stat(Game game)
    {
        Game = game;
    }

    public int GetValue()
    {
        int value = BASE_VALUE;
        foreach(StatModifier mod in GetModifiers()) value += mod.Value;
        return value;
    }

    public abstract List<StatModifier> GetModifiers();

    public Color GetValueColor()
    {
        int value = GetValue();

        if (value == 100) return ResourceManager.Singleton.SE_Neutral;

        if (value < 50) return ResourceManager.Singleton.SE_ExtremelyBad;
        if (value < 75) return ResourceManager.Singleton.SE_VeryBad;
        if (value < 100) return ResourceManager.Singleton.SE_Bad;

        if (value > 150) return ResourceManager.Singleton.SE_ExtremelyGood;
        if (value > 125) return ResourceManager.Singleton.SE_VeryGood;
        if (value > 100) return ResourceManager.Singleton.SE_Good;

        throw new System.Exception("Value " + value + " not handled.");
    }
}
