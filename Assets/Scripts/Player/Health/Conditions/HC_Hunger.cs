using UnityEngine;

public class HC_Hunger : HealthCondition
{
    public float Nutrition { get; private set; }

    protected override void OnInit()
    {
        Nutrition = 7.5f;
    }

    public override void OnUpdate()
    {
        if (Nutrition <= 1f) SetActiveStage(2);
        else if (Nutrition <= 2.5f) SetActiveStage(1);
        else if (Nutrition <= 5f) SetActiveStage(0);
        else SetActiveStage(null);
    }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        Player.ModifyNutrition(-PlayerCharacter.BASE_NUTRITION_DROP_PER_DAY);
    }

    public override string IsFatal()
    {
        if (Nutrition <= 0f) return "You starved";
        return "";
    }

    public void ModifyNutrition(float value)
    {
        Nutrition += value;
    }
}
