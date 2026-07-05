using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
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
    /// 
    /// Needs are special kind of health condition this health condition is always present on the player, and the condition is updated in every UpdatePermanentHealthConditions(). It does not mean that it is always active though, and while inactive, it has no effects and is not visible to the player. This is mostly used for "needs" such as hunger and thirst.
    /// </summary>
    public bool IsNeed { get; init; } = false;

    /// <summary>
    /// The severity stages of the health condition. If the health condition is active, exactly one stage will be active, and the stage will determine the effects of the health condition on the player.
    /// </summary>
    public List<HealthConditionStage> Stages { get; init; } = new List<HealthConditionStage>();

    /// <summary>
    /// The severity that this condition has when it is applied to the player without a specific value.
    /// </summary>
    public float InitialSeverity { get; init; } = 1f;

    /// <summary>
    /// The maximum severity value this health condition can reach. It will never go above this value. If IsLethal is true, the player dies when the severity reaches this value.
    /// </summary>
    public float MaxSeverity { get; init; } = 100;

    /// <summary>
    /// If true, the player dies when the severity of this health condition reaches MaxSeverity.
    /// </summary>
    public bool IsLethal { get; init; } = false;

    /// <summary>
    /// The message that gets displayed in the game over screen when the player dies from this health condition.
    /// </summary>
    public string LethalityMessage { get; init; }

    /// <summary>
    /// The maximum amount of this health condition that can be active at the same time.
    /// </summary>
    public int MaxInstances { get; init; } = 1;

    /// <summary>
    /// How much the severity decreases during the night (or during the day if resting).
    /// </summary>
    public float NaturalHealing { get; init; } = 0f;

    /// <summary>
    /// Wounds must be marked with this flag. Used purely for validation.
    /// </summary>
    public bool IsWound { get; init; } = false;

    /// <summary>
    /// Fractures must be marked with this flag. Used purely for validation.
    /// </summary>
    public bool IsFracture { get; init; } = false;

    public HealthConditionDef(string defName) : base(defName) { }


    public override bool Validate()
    {
        if (IsNeed)
        {
            if (MaxInstances != 1) throw new System.Exception($"Needs must have MaxAmount of 1.");
            if (NaturalHealing != 0f) throw new System.Exception("Needs cannot have natural healing.");
        }

        if (MaxInstances < 1) throw new System.Exception($"Health condition {DefName} has a MaxAmount of {MaxInstances} which is less than 1.");
        if (!IsWound && !Stages.Any(stage => stage.SeverityThreshold == 0f)) throw new System.Exception($"Health condition {DefName} must have a stage with a severity threshold of 0.");
        if (NaturalHealing < 0) throw new System.Exception($"Health condition {DefName} has a natural healing value of {NaturalHealing} which is negative.");

        float prevThreshold = float.NegativeInfinity;
        foreach (HealthConditionStage stage in Stages)
        {
            if (stage.StatModifiers.Count > 0 && !stage.IsVisible) throw new System.Exception("Health condition stages with stat modifiers must be visible.");

            // Disallow same threshold as other stage
            if (Stages.Count(s => s.SeverityThreshold == stage.SeverityThreshold) > 1) throw new System.Exception($"Health condition {DefName} has multiple stages with the same severity threshold of {stage.SeverityThreshold}.");

            // Negative
            if(stage.SeverityThreshold < 0) throw new System.Exception($"Health condition {DefName} has a stage with a severity threshold of {stage.SeverityThreshold} which is negative.");

            // Above max
            if(stage.SeverityThreshold >= MaxSeverity) throw new System.Exception($"Health condition {DefName} has a stage with a severity threshold of {stage.SeverityThreshold} which is above or equal to the max severity of {MaxSeverity}.");

            // Check order
            if(stage.SeverityThreshold <= prevThreshold) throw new System.Exception($"Health condition {DefName} has stages that are not in order of severity threshold. Stage with threshold {stage.SeverityThreshold} comes after stage with threshold {prevThreshold}.");

            prevThreshold = stage.SeverityThreshold;
        }

        if (IsLethal && string.IsNullOrEmpty(LethalityMessage)) throw new System.Exception($"Health condition {DefName} is lethal but does not have a lethality message.");
        if (!IsLethal && !string.IsNullOrEmpty(LethalityMessage)) throw new System.Exception($"Health condition {DefName} has a lethality message but is not lethal.");

        // Wound subclass validation
        if (IsWound)
        {
            if (!typeof(Wound).IsAssignableFrom(HealthConditionClass)) throw new System.Exception($"Health condition {DefName} is marked as a wound but its class {HealthConditionClass} does not inherit from Wound.");
        }

        return base.Validate();
    }
}
