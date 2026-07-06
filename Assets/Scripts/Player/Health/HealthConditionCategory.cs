using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General category/classification of a health condition.
/// </summary>
public class HealthConditionCategoryDef : Def
{
    public override string DefTypeLabel => "Health Condition Category";
    public HealthConditionCategoryDef(string defName) : base(defName) { }
}

public static class HealthConditionCategoryDefs
{
    public static List<HealthConditionCategoryDef> Defs => new List<HealthConditionCategoryDef>()
    {
        new HealthConditionCategoryDef("Need")
        {
            Label = "Need",
            Description = "Needs are special kind of health condition this health condition is always present on the player, and the condition is updated in every UpdatePermanentHealthConditions(). It does not mean that it is always active though, and while inactive, it has no effects and is not visible to the player. This is mostly used for needs such as hunger and thirst."
        },
        new HealthConditionCategoryDef("Negative")
        {
            Label = "Negative",
            Description = "Health conditions that have a detrimental effect on the player."
        },
        new HealthConditionCategoryDef("Positive")
        {
            Label = "Positive",
            Description = "Health conditions that have a beneficial effect on the player."
        },
        new HealthConditionCategoryDef("Neutral")
        {
            Label = "Neutral",
            Description = "Health conditions that can't be classified as strictly positive or negative."
        }
    };
}

[DefOf]
public static class HealthConditionCategoryDefOf
{
    public static HealthConditionCategoryDef Need;
    public static HealthConditionCategoryDef Negative;
    public static HealthConditionCategoryDef Positive;
    public static HealthConditionCategoryDef Neutral;
}
