/// <summary>
/// Represents a possible outcome of a skill check, including its label, the roll range that leads to it, and the chance of it occurring.
/// </summary>
public class SkillCheckOutcomeChance
{
    public OptionOutcomeDef Outcome { get; private set; }
    public string Label { get; private set; }
    public int MinRoll { get; private set; }
    public int MaxRoll { get; private set; }
    public float Chance { get; private set; }

    public SkillCheckOutcomeChance(OptionOutcomeDef outcome, int minRoll, int maxRoll)
    {
        Outcome = outcome;
        Label = outcome.Label;
        MinRoll = minRoll;
        MaxRoll = maxRoll;
        Chance = (maxRoll - minRoll + 1) / 100f;
    }
}
