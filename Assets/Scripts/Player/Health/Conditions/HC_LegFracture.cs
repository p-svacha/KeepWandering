public class HC_LegFracture : HealthCondition
{
    public bool IsRightLeg { get; private set; }
    public LimbRenderer Renderer { get; private set; }

    public void SetSide(bool isRightLeg)
    {
        IsRightLeg = isRightLeg;
        if (IsRightLeg) Renderer = PlayerRenderer.LegFront;
        else Renderer = PlayerRenderer.LegBack;

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
        baseHealing -= 0.2f * Player.UntendedBruiseWounds.Count;
        if (baseHealing < 0f) baseHealing = 0f;
        return baseHealing;
    }

    public override string GetReportLabel()
    {
        return base.GetReportLabel() + (IsRightLeg ? " (Right)" : " (Left)");
    }
}
