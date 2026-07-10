using System.Collections.Generic;
using UnityEngine;

public enum SuccessLevel
{
    CriticalSuccess = 5,
    Success = 4,
    PartialSuccess = 3,
    Failure = 2,
    CriticalFailure = 1,
}

public class OptionOutcomeDef : Def
{
    public override string DefTypeLabel => "Option Outcome";

    /// <summary>
    /// The higher the number, the better the outcome. Used for sorting or logic that applies to multiple outcomes above/below a certain level.
    /// </summary>
    public SuccessLevel SuccessLevel { get; init; }

    /// <summary>
    /// If the player very generally achieves what they wanted with this option. True for critical success, success, and partial success. False for failure and critical failure.
    /// </summary>
    public bool IsSuccess { get; init; }

    public Color Color { get; init; }


    public OptionOutcomeDef(string defName) : base(defName) { }
}

public static class OptionOutcomeDefs
{
    public static List<OptionOutcomeDef> Defs => new List<OptionOutcomeDef>()
    {
        new OptionOutcomeDef("CriticalSuccess")
        {
            Label = "Critical Success",
            Description = "The player critically succeeds in the action they are trying to do. This is usually a better version of success, with an outcome that is even better than success.",
            Color = new Color(0.24f, 0.57f, 0f),
            SuccessLevel = SuccessLevel.CriticalSuccess,
            IsSuccess = true,
        },

        new OptionOutcomeDef("Success")
        {
            Label = "Success",
            Description = "The player fully succeeds in the action they are trying to do.",
            Color = new Color(0.54f, 1f, 0.21f),
            SuccessLevel = SuccessLevel.Success,
            IsSuccess = true,
        },

        new OptionOutcomeDef("PartialSuccess")
        {
            Label = "Partial Success",
            Description = "The player partially succeeds in the action they are trying to do. This is usually a middle ground between success and failure, with an outcome that is better than failure but worse than success.",
            Color = new Color(1f, 0.96f, 0f),
            SuccessLevel = SuccessLevel.PartialSuccess,
            IsSuccess = true,
        },

        new OptionOutcomeDef("Failure")
        {
            Label = "Failure",
            Description = "The player fails in the action they are trying to do.",
            Color = new Color(1f, 0.36f, 0f),
            SuccessLevel = SuccessLevel.Failure,
            IsSuccess = false,
        },

        new OptionOutcomeDef("CriticalFailure")
        {
            Label = "Critical Failure",
            Description = "The player critically fails in the action they are trying to do. This is usually a worse version of failure, with an outcome that is even worse than failure.",
            Color = new Color(0.67f, 0.03f, 0f),
            SuccessLevel = SuccessLevel.CriticalFailure,
            IsSuccess = false,
        }
    };
}

[DefOf]
public static class OptionOutcomeDefOf
{
    public static OptionOutcomeDef CriticalSuccess;
    public static OptionOutcomeDef Success;
    public static OptionOutcomeDef PartialSuccess;
    public static OptionOutcomeDef Failure;
    public static OptionOutcomeDef CriticalFailure;
}
