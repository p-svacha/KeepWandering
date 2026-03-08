using UnityEngine;

public class HC_CutWound : Wound
{
    public static float BLEED_PER_SEVERITY = 0.5f;
    protected override string GetUntendedEffectString() => "While untended, this wound causes bleeding.";

    protected override void OnEndDay(MorningReport morningReport)
    {
        base.OnEndDay(morningReport);

        if (!IsTended) Player.ApplyBloodLoss(BLEED_PER_SEVERITY);
    }
}
