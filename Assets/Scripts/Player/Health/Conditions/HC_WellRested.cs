using System.Collections.Generic;
using UnityEngine;

public class HC_WellRested : HealthCondition
{
    public int MoraleBonus { get; private set; }

    public void Init(bool hasTent, bool hasBedroll, bool hasFire)
    {
        if (!hasTent && !hasBedroll && !hasFire) throw new System.Exception("HC_WellRested requires at least one of the following: tent, bedroll, or fire.");

        MoraleBonus = 0;
        if (hasTent) MoraleBonus += Camp.TENT_MORALE_BONUS;
        if (hasBedroll) MoraleBonus += Camp.BEDROLL_MORALE_BONUS;
        if (hasFire) MoraleBonus += Camp.FIRE_MORALE_BONUS;
    }

    public override Dictionary<StatDef, int> GetStatCurrentModifiers()
    {
        return new Dictionary<StatDef, int>()
        {
            { StatDefOf.Morale, MoraleBonus }
        };
    }
}
