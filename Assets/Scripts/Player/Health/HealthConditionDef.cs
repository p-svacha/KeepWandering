using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthConditionDef : Def
{
    override public string DefTypeLabel => "Health Condition";
    public override Sprite Sprite => ResourceManager.LoadSpriteFromSheet("HealthConditions", BaseSpriteName ?? DefName); // used for outcome notes, in wounds additionally on body
    public const float DEFAULT_INITIAL_SEVERITY = 1f;
    public const float DEFAULT_MAX_SEVERITY = 10f;
    public const string HEALS_NATURALLY = "Heals naturally";


    /// <summary>
    /// The class of the health condition. This class will be instantiated and added to the player when the health condition is applied to the player. It must be a subclass of HealthCondition.
    /// </summary>
    public System.Type HealthConditionClass { get; init; } = typeof(HealthCondition);
    public bool IsVital => Category == HealthConditionCategoryDefOf.Vital;

    /// <summary>
    /// Can be set to override the default sprite for this health condition.
    /// </summary>
    public string BaseSpriteName { get; init; } = null;

    /// <summary>
    /// Describes what the player can do to affect this health condition.
    /// </summary>
    public string Interactions { get; init; } = null;

    /// <summary>
    /// The category of the health condition. This is used to classify health conditions into different types, such as vitals, negative conditions, positive conditions, etc.
    /// </summary>
    public HealthConditionCategoryDef Category { get; init; } = null;

    /// <summary>
    /// The severity stages of the health condition. If the health condition is active, exactly one stage will be active, and the stage will determine the effects of the health condition on the player.
    /// </summary>
    public List<HealthConditionStage> Stages { get; init; } = new List<HealthConditionStage>();

    /// <summary>
    /// The severity that this condition has when it is applied to the player without a specific value. If <= 0, an initial severity must be provided when applying the condition. If > 0, this value will be used as the initial severity when applying the condition without a specific value.
    /// </summary>
    public float DefaultInitialSeverity { get; init; } = DEFAULT_INITIAL_SEVERITY;

    /// <summary>
    /// The maximum severity value this health condition can reach. It will never go above this value. If IsLethal is true, the player dies when the severity reaches this value.
    /// </summary>
    public float MaxSeverity { get; init; } = DEFAULT_MAX_SEVERITY;

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
    /// How much the severity decreases when natural healing is applied (during the night or if resting).
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

    /// <summary>
    /// How much the severity of this health condition naturally changes per day, unaffected by any other factors. For example, if set to -1, severity can be used as remaining duration.
    /// </summary>
    public float NaturalSeverityChange { get; init; } = 0f;

    public HealthConditionDef(string defName) : base(defName) { }


    public override bool Validate()
    {
        if (Category == null) ThrowValidationError($"Health condition {DefName} does not have a category.");
        if (Sprite == null) ThrowValidationError($"Health condition {DefName} does not have a sprite.");

        if (!string.IsNullOrEmpty(Description)) ThrowValidationError($"Health condition {DefName} has a description. Health conditions should not have descriptions, as the description should come from stages.");
        if (!IsVital && DefaultInitialSeverity <= 0) ThrowValidationError($"Health condition {DefName} must have a default initial severity greater than 0.");
        if (DefaultInitialSeverity > MaxSeverity) ThrowValidationError($"Health condition {DefName} has a default initial severity of {DefaultInitialSeverity} which is greater than the max severity of {MaxSeverity}.");

        if (Category == HealthConditionCategoryDefOf.Negative && string.IsNullOrEmpty(Interactions) && !IsWound) ThrowValidationError($"Health condition {DefName} is negative but does not have any interactions defined. Negative health conditions should have interactions define, so the player knows how to deal with them.");

        if (IsVital)
        {
            if (MaxInstances != 1) ThrowValidationError($"Vitals must have MaxAmount of 1.");
        }

        if (MaxInstances < 1) ThrowValidationError($"Health condition {DefName} has a MaxAmount of {MaxInstances} which is less than 1.");
        if (!IsWound && !Stages.Any(stage => stage.SeverityThreshold == 0f)) ThrowValidationError($"Health condition {DefName} must have a stage with a severity threshold of 0.");
        if (NaturalHealing < 0) ThrowValidationError($"Health condition {DefName} has a natural healing value of {NaturalHealing} which is negative.");

        // Stage validation
        if (Stages.Count == 0 && !IsWound) ThrowValidationError($"Health condition {DefName} does not have any stages. This is only allowed for wounds, as they define their own stages in the Wound class.");
        float prevThreshold = float.NegativeInfinity;
        foreach (HealthConditionStage stage in Stages)
        {
            if (stage == null) ThrowValidationError($"Health condition {DefName} has a null stage.");
            if (stage.StatModifiers == null) ThrowValidationError($"Health condition {DefName} stage {stage.Label} has null stat modifiers.");

            if (stage.StatModifiers.Count > 0 && !stage.IsVisible) ThrowValidationError($"Health condition {DefName} stage {stage.Label} has stat modifiers and must be visible.");

            // Vital changes
            foreach (var vitalChange in stage._EndOfDayVitalChanges)
            {
                if (vitalChange.Value == 0) ThrowValidationError($"Health condition {DefName} has a stage with an end of day vital change for {vitalChange.Key}, which is 0. This is not allowed, as it has no effect.");
            }

            // End of day health conditions
            foreach (var appliedCondition in stage._AppliedHealthConditions)
            {
                if (appliedCondition.Chance == 0) ThrowValidationError($"Health condition {DefName} has a stage with an applied health condition {appliedCondition.Condition}, which has a chance of 0. This is not allowed, as it has no effect.");
                if (appliedCondition.Chance > 1) ThrowValidationError($"Health condition {DefName} has a stage with an applied health condition {appliedCondition.Condition}, which has a chance of {appliedCondition.Chance}. This is not allowed, as it must be between 0 and 1.");
            }

            // Disallow same threshold as other stage
            if (Stages.Count(s => s.SeverityThreshold == stage.SeverityThreshold) > 1) ThrowValidationError($"Health condition {DefName} has multiple stages with the same severity threshold of {stage.SeverityThreshold}.");

            // Negative
            if(stage.SeverityThreshold < 0) ThrowValidationError($"Health condition {DefName} has a stage with a severity threshold of {stage.SeverityThreshold} which is negative.");

            // Above max
            if(stage.SeverityThreshold >= MaxSeverity) ThrowValidationError($"Health condition {DefName} has a stage with a severity threshold of {stage.SeverityThreshold} which is above or equal to the max severity of {MaxSeverity}.");

            // Check order
            if(stage.SeverityThreshold <= prevThreshold) ThrowValidationError($"Health condition {DefName} has stages that are not in order of severity threshold. Stage with threshold {stage.SeverityThreshold} comes after stage with threshold {prevThreshold}.");

            prevThreshold = stage.SeverityThreshold;
        }

        if (IsLethal && string.IsNullOrEmpty(LethalityMessage)) ThrowValidationError($"Health condition {DefName} is lethal but does not have a lethality message.");
        if (!IsLethal && !string.IsNullOrEmpty(LethalityMessage)) ThrowValidationError($"Health condition {DefName} has a lethality message but is not lethal.");

        // Wound subclass validation
        if (IsWound)
        {
            if (!typeof(Wound).IsAssignableFrom(HealthConditionClass)) ThrowValidationError($"Health condition {DefName} is marked as a wound but its class {HealthConditionClass} does not inherit from Wound.");
        }

        

        return base.Validate();
    }

    public override void ResolveReferences()
    {
        foreach(var stage in Stages)
        {
            stage.ResolveReferences(this);
        }
    }
}
