using UnityEngine;

public class HC_BloodLoss : HealthCondition
{
    public float BloodAmount { get; private set; } // [0-1] how much blood you have, 1 = healthy, 0 = dead

    protected override void OnInit()
    {
        BloodAmount = 1f;
    }

    public override void OnUpdate()
    {
        if (BloodAmount <= 0.2f) SetActiveStage(2);
        else if (BloodAmount <= 0.5f) SetActiveStage(1);
        else if (BloodAmount <= 0.9f) SetActiveStage(0);
        else SetActiveStage(null);
    }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        float bloodChange = PlayerCharacter.BASE_BLOOD_REGEN_PER_DAY;
        if (Player.HasUntendedCutWound)
        {
            bloodChange = 0f;
            foreach (HC_CutWound wound in Player.UntendedCutWounds) bloodChange -= PlayerCharacter.CUT_WOUND_BLEED_PER_DAY;
        }
        Player.ModifyBloodAmount(bloodChange);
    }

    public override string IsFatal()
    {
        if (BloodAmount <= 0f) return "You died of blood loss.";
        return "";
    }

    public void ModifyBloodAmount(float value)
    {
        BloodAmount += value;
        if (BloodAmount > 1f) BloodAmount = 1f;
    }
}
