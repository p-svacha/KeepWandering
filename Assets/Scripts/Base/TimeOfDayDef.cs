using UnityEngine;

public class TimeOfDayDef : Def
{
    public override string DefTypeLabel => "Time of Day";

    public Color SkyColor { get; init; }
    public Color LightingAmbienceOverlayColor { get; init; }
}
