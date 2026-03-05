using System.Collections.Generic;
using UnityEngine;

public class HealthConditionDef : Def
{
    override public string DefTypeLabel => "Health Condition";
    public override Sprite Sprite => ResourceManager.LoadSprite($"HealthConditions/{DefName}");

    /// <summary>
    /// The class of the health condition. This class will be instantiated and added to the player when the health condition is applied to the player. It must be a subclass of HealthCondition.
    /// </summary>
    public System.Type HealthConditionClass { get; init; } = typeof(HealthCondition);

    /// <summary>
    /// If true, exactly one instance of this health condition is always present on the player, and the condition is updated in every UpdatePermanentHealthConditions(). It does not mean that it is always active though, and while inactive, it has no effects and is not visible to the player.
    /// </summary>
    public bool IsPermanent { get; init; } = false;

    /// <summary>
    /// The severity stages of the health condition. If the health condition is active, exactly one stage will be active, and the stage will determine the effects of the health condition on the player.
    /// </summary>
    public List<HealthConditionStage> Stages { get; init; } = new List<HealthConditionStage>();

    /// <summary>
    /// The maximum amount of this health condition that can be active at the same time.
    /// </summary>
    public int MaxAmount { get; init; } = 1;

    public override bool Validate()
    {
        if (IsPermanent && MaxAmount != 1) throw new System.Exception($"Permanent health condition {DefName} must have MaxAmount of 1.");

        return base.Validate();
    }
}
