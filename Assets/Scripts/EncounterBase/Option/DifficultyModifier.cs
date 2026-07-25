/// <summary>
/// Simple wrapper for a difficulty modifier that can be applied to a skill check option.
/// </summary>
public class DifficultyModifier
{
    public string Label { get; private set; }
    public int Value { get; private set; }

    public DifficultyModifier(string label, int value)
    {
        Label = label;
        Value = value;
    }
}
