using UnityEngine;

public class HC_LegFracture : HealthCondition
{
    public float BoneHealth { get; private set; } // [0-1] how fractures the bones are, 1 = healthy, 0 = dead

    protected override void OnInit()
    {
        BoneHealth = 1f;
    }

    public override void OnUpdate()
    {
        if (BoneHealth <= 0.2f) SetActiveStage(2);
        else if (BoneHealth <= 0.5f) SetActiveStage(1);
        else if (BoneHealth <= 0.9f) SetActiveStage(0);
        else SetActiveStage(null);
    }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        bool canRegenBone = !Player.HasUntendedBruiseWound;
        if (canRegenBone) Player.ModifyLegBoneHealth(PlayerCharacter.BASE_BONE_REGEN_PER_DAY);
    }

    public override string IsFatal()
    {
        if (BoneHealth <= 0f) return "You died due to exreme fractures.";
        return "";
    }

    public void ModifyBoneHealth(float value)
    {
        BoneHealth += value;
        if (BoneHealth > 1f) BoneHealth = 1f;
    }
}
