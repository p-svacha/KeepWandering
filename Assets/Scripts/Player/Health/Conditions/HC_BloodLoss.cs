using System.Collections.Generic;
using UnityEngine;

public class HC_BloodLoss : HealthCondition
{

    private List<Color> StageColors = new List<Color>()
    {
        Color.white,
        new Color(1f, 0.9f, 0.9f),
        new Color(1f, 0.75f, 0.75f),
        new Color(1f, 0.6f, 0.6f),
    };


    protected override void OnActiveStageChanged()
    {
        PlayerRenderer.SetCharacterColor(StageColors[ActiveStageIndex]);
    }
    public override void OnRemoved()
    {
        PlayerRenderer.SetCharacterColor(StageColors[0]);
    }
}
