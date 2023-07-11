using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Moving : Stat
{
    public override StatId Id => StatId.Moving;
    public override string Name => "Moving";
    public override string Description => "How fast you are at moving. Higher moving increases likelyhood to reach a destination at the end of the day.";

    public Stat_Moving(Game game) : base(game) { }

    public override List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        return modifiers;
    }
}
