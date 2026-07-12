using UnityEngine;

public class HC_Thirst : HealthCondition
{
    protected override void OnActiveStageChanged()
    {
        PlayerRenderer.SetActiveSprite(PlayerRenderer.DehydrationOverlay, ActiveStageIndex - 2);
    }
}
