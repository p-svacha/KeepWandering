using System.Collections.Generic;
using UnityEngine;

public class HC_BurnWound : Wound
{
    public static float UNBANDAGED_THIRST_RATE = 0.5f;
    protected override string GetUnbandagedEffectString() => "While unbandaged, this wound increases thirst rate.";

    public override Dictionary<HealthConditionDef, float> GetCurrentEndOfDayVitalChanges()
    {
        Dictionary<HealthConditionDef, float> vitalChanges = new(ActiveStage.EndOfDayVitalChanges); // Copy to avoid modifying the original
        if (!IsBandaged) vitalChanges.Increment(HealthConditionDefOf.Thirst, UNBANDAGED_THIRST_RATE);
        return vitalChanges;
    }
}
