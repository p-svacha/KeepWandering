using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Dexterity : Stat
{
    public override StatId Id => StatId.Dexterity;
    public override string Name => "Dexterity";
    public override string Description => "How skillfully you can move your body to clear obstacles and avoid traps.";

    public Stat_Dexterity(Game game) : base(game) { }

    public override List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        return modifiers;
    }
}
