using System.Collections.Generic;
using UnityEngine;

public static class OptionOutcomeDefs
{
    public static List<OptionOutcomeDef> Defs => new List<OptionOutcomeDef>()
    {
        new OptionOutcomeDef()
        {
            DefName = "CriticalSuccess",
            Label = "Critical Success",
            Description = "The player critically succeeds in the action they are trying to do. This is usually a better version of success, with an outcome that is even better than success.",
            Color = new Color(0.24f, 0.57f, 0f),
            SuccessLevel = 5,
            IsSuccess = true,
        },

        new OptionOutcomeDef()
        {
            DefName = "Success",
            Label = "Success",
            Description = "The player fully succeeds in the action they are trying to do.",
            Color = new Color(0.54f, 1f, 0.21f),
            SuccessLevel = 4,
            IsSuccess = true,
        },

        new OptionOutcomeDef()
        {
            DefName = "PartialSuccess",
            Label = "Partial Success",
            Description = "The player partially succeeds in the action they are trying to do. This is usually a middle ground between success and failure, with an outcome that is better than failure but worse than success.",
            Color = new Color(1f, 0.96f, 0f),
            SuccessLevel = 3,
            IsSuccess = true,
        },

        new OptionOutcomeDef()
        {
            DefName = "Failure",
            Label = "Failure",
            Description = "The player fails in the action they are trying to do.",
            Color = new Color(1f, 0.36f, 0f),
            SuccessLevel = 2,
            IsSuccess = false,
        },

        new OptionOutcomeDef()
        {
            DefName = "CriticalFailure",
            Label = "Critical Failure",
            Description = "The player critically fails in the action they are trying to do. This is usually a worse version of failure, with an outcome that is even worse than failure.",
            Color = new Color(0.67f, 0.03f, 0f),
            SuccessLevel = 1,
            IsSuccess = false,
        }
    };
}
