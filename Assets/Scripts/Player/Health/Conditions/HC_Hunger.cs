using UnityEngine;
using UnityEngine.TextCore.Text;

public class HC_Hunger : HealthCondition
{
    protected override void OnActiveStageChanged()
    {
        PlayerRenderer.SetActiveSprite(PlayerRenderer.Torso, ActiveStageIndex);
    }
}
