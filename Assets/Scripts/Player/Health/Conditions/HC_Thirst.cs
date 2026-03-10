using UnityEngine;

public class HC_Thirst : HealthCondition
{

    protected override void OnActiveStageChanged()
    {
        PlayerRenderer.SetActiveSprite(PlayerRenderer.DehydrationOverlay, ActiveStageIndex - 2);
    }

    protected override void OnEndDay(MorningReport morningReport)
    {
        // Increase thirst
        Player.ModifyThirst(PlayerCharacter.THIRST_INCREASE_PER_DAY);
    }
}
