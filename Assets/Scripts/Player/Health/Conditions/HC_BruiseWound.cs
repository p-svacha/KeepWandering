using UnityEngine;

public class HC_BruiseWound : Wound
{
    protected override string GetUnbandagedEffectString() => "While untended, this slows the healing process of fractures.";
}
