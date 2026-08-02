public class HC_Fracture : HealthCondition
{
    public bool IsRightSide { get; private set; }

    public void SetSide(bool isRightSide)
    {
        IsRightSide = isRightSide;
    }

    public override float GetNaturalHealing()
    {
        float baseHealing = 0.5f;

        // Reduce by 0.2 for each unbandaged bruise
        baseHealing -= 0.2f * Player.UnbandagedBruiseWounds.Count;
        if (baseHealing < 0f) baseHealing = 0f;
        return baseHealing;
    }

    public override string GetReportLabel()
    {
        return base.GetReportLabel() + (IsRightSide ? " (Right)" : " (Left)");
    }
}
