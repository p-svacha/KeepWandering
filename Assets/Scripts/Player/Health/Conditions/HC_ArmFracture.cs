public class HC_ArmFracture : HealthCondition
{
    public bool IsRightArm { get; private set; }
    public LimbRenderer Renderer { get; private set; }

    public void SetSide(bool isRightArm)
    {
        IsRightArm = isRightArm;
        if (IsRightArm) Renderer = PlayerRenderer.RightArm;

        if (Renderer != null) Renderer.Render(ActiveStageIndex);
    }

    protected override void OnActiveStageChanged()
    {
        if (Renderer != null) Renderer.Render(ActiveStageIndex);
    }

    public override float GetNaturalHealing()
    {
        float baseHealing = 0.5f;

        // Reduce by 0.2 for each untended bruise
        baseHealing -= 0.2f * Player.UnbandagedBruiseWounds.Count;
        if (baseHealing < 0f) baseHealing = 0f;
        return baseHealing;
    }

    public override string GetReportLabel()
    {
        return base.GetReportLabel() + (IsRightArm ? " (Right)" : " (Left)");
    }
}
