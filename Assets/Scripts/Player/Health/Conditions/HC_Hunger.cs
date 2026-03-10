using UnityEngine;
using UnityEngine.TextCore.Text;

public class HC_Hunger : HealthCondition
{
    protected override void OnActiveStageChanged()
    {
        PlayerRenderer.SetActiveSprite(PlayerRenderer.Torso, ActiveStageIndex);
    }

    protected override void OnEndDay(MorningReport morningReport)
    {
        // Increase hunger
        Player.ModifyHunger(PlayerCharacter.HUNGER_INCREASE_PER_DAY);
    }
}
