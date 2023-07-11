using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Charisma : Stat
{
    public override StatId Id => StatId.Charisma;
    public override string Name => "Charisma";
    public override string Description => "How sympathetic you seem to other people and how easy it is for you to persuade and negotiate.";

    public Stat_Charisma(Game game) : base(game) { }

    public override List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        return modifiers;
    }
}
