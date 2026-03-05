using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    protected Game Game;
    public PlayerCharacter Player {  get; private set; }
    public StatDef Def { get; private set; }

    public string Label => Def.Label;
    public string Description => Def.Description;

    public int BaseValue {  get; private set; }

    public Stat(Game game, PlayerCharacter player, StatDef def)
    {
        Game = game;
        Player = player;
        Def = def;
    }

    public int GetValue()
    {
        int value = BaseValue;
        foreach(StatModifier mod in GetModifiers()) value += mod.Value;
        return value;
    }

    public void ModifyBaseValue(int amount)
    {
        BaseValue += amount;
    }

    public List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        // Health conditions
        foreach (HealthCondition condition in Player.ActiveHealthConditions)
        {
            int modifierValue = condition.GetStatModifierFor(Def);
            if (modifierValue != 0) modifiers.Add(new StatModifier(condition.GetReportLabel(), modifierValue));
        }

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
