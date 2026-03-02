using UnityEngine;

public class HC_Thirst : HealthCondition
{
    public float Hydration { get; private set; }

    protected override void OnInit()
    {
        Hydration = 6.5f;
    }

    public override void OnUpdate()
    {
        if (Hydration <= 1f) SetActiveStage(2);
        else if (Hydration <= 2f) SetActiveStage(1);
        else if (Hydration <= 4f) SetActiveStage(0);
        else SetActiveStage(null);
    }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        Player.ModifyHydration(-PlayerCharacter.BASE_HYDRATION_DROP_PER_DAY);
    }

    public override string IsFatal()
    {
        if (Hydration <= 0f) return "You died of dehydration";
        return "";
    }

    public void ModifyHydration(float value)
    {
        Hydration += value;
    }
}
