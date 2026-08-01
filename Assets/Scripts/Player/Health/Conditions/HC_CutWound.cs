using System.Collections.Generic;
using UnityEngine;

public class HC_CutWound : Wound
{
    public static float UNBANDAGED_BLOOD_LOSS = 0.5f;
    protected override string GetUnbandagedEffectString() => "While unbandaged, this wound causes bleeding.";

    public override Dictionary<HealthConditionDef, float> GetCurrentEndOfDayVitalChanges()
    {
        Dictionary<HealthConditionDef, float> vitalChanges = new(ActiveStage.EndOfDayVitalChanges); // Copy to avoid modifying the original
        if (!IsBandaged) vitalChanges.Increment(HealthConditionDefOf.BloodLoss, UNBANDAGED_BLOOD_LOSS);
        return vitalChanges;
    }
}
