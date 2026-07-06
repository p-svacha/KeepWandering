using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HealthCondition
{
    private const float NATURAL_HEALING_RANDOM_OFFSET = 0.1f; // in %, both directions

    public Game Game => Player.Game;
    public PlayerCharacter Player { get; private set; }
    public HealthConditionDef Def { get; private set; }
    public bool IsSingleStageCondition { get; private set; } // Single stage conditions are not affected by severity and always have the same effect.
    public virtual List<HealthConditionStage> Stages => Def.Stages;
    public HealthConditionStage ActiveStage { get; private set; }
    public int OriginDay { get; private set; }
    public int ActiveStageIndex => Stages.IndexOf(ActiveStage);

    // Def overrides
    public virtual float GetNaturalHealing() => Def.NaturalHealing;
    public virtual float InitialSeverity => Def.InitialSeverity;
    public virtual float MaxSeverity => Def.MaxSeverity;
    public virtual bool IsLethal => Def.IsLethal;
    public bool IsNeed => Def.IsNeed;
    public bool IsNegative => Def.Category == HealthConditionCategoryDefOf.Negative;
    public virtual string LethalityMessage => Def.LethalityMessage;

    protected PlayerCharacterRenderer PlayerRenderer => Player.Renderer;

    /// <summary>
    /// The general severity value of this condition. If the condition has stages, the value determines the active stage based on the defined stage thresholds.
    /// </summary>
    public float SeverityValue { get; private set; }

    public HealthCondition() { } // Empty constructor for Activator

    public UI_StatusEffect UiDisplayElement { get; set; }

    public void Init(PlayerCharacter player, HealthConditionDef def, float initialSeverity)
    {
        Player = player;
        Def = def;

        SeverityValue = initialSeverity;
        if (SeverityValue <= 0) SeverityValue = InitialSeverity;

        OriginDay = Game.Instance.Day;
        OnInit();

        if (SeverityValue <= 0) throw new System.Exception($"Health condition {Def} cannot be initialized with non-positive severity value as that would remove it immediately.");

        UpdateStage();
    }

    /// <summary>
    /// Modifies the severity value of this condition by the given value. If the new severity value is below 0, it will be set to 0. If it is above MaxSeverity, it will be set to MaxSeverity. After modifying the severity, the active stage will be updated accordingly.
    /// <br/>If avoidFullHeal is true, the severity will not be allowed to reach 0, and will be set to 0.1 instead.
    /// </summary>
    public void ModifySeverity(float value, bool avoidFullHeal = false)
    {
        float oldSeverity = SeverityValue;
        SeverityValue += value;

        // Clamp severity value to valid range
        if (SeverityValue < 0) SeverityValue = 0f;
        if (avoidFullHeal && SeverityValue < 0.1f) SeverityValue = 0.1f;
        if (SeverityValue > MaxSeverity) SeverityValue = MaxSeverity;

        float newSeverity = SeverityValue;

        Debug.Log($"Modified severity of {Def} by {value}. Old severity: {oldSeverity}, new severity: {newSeverity}");

        UpdateStage();
    }

    /// <summary>
    /// Sets the active stage of the condition based on the current severity value.
    /// </summary>
    public void UpdateStage()
    {
        // Remove if at 0
        if (SeverityValue <= 0f && !Def.IsNeed)
        {
            OnRemoved();
            Player.RemoveHealthCondition(this);
            return;
        }

        if (IsSingleStageCondition)
        {
            ActiveStage = Stages.First();
            OnActiveStageChanged();
            return;
        }

        foreach (HealthConditionStage stage in Stages)
        {
            if (SeverityValue >= stage.SeverityThreshold)
            {
                ActiveStage = stage;
            }
            else break;
        }

        Debug.Log($"Updated active stage of {Def.DefName} to {ActiveStage.Label} based on severity value of {SeverityValue}. ActiveStageIndex is now {ActiveStageIndex}.");
        OnActiveStageChanged();
    }

    public void ExecuteEndDayEffect(MorningReport morningReport)
    {
        // Specific effects
        OnEndDay(morningReport);
    }

    public void ApplyNaturalHealing(float healingFactor = 1f)
    {
        float naturalHealing = GetNaturalHealing();
        naturalHealing *= healingFactor;
        float randomOffset = Random.Range(-NATURAL_HEALING_RANDOM_OFFSET, NATURAL_HEALING_RANDOM_OFFSET);

        naturalHealing *= 1 + randomOffset;
        if (naturalHealing > 0f)
        {
            Debug.Log($"Applying natural healing of {naturalHealing} to {Def.DefName} with a healing factor of {healingFactor}.");
            ModifySeverity(-naturalHealing);
        }
    }

    /// <summary>
    /// Gets called once when the health condition is added to the player.
    /// </summary>
    protected virtual void OnInit() { }

    /// <summary>
    /// Gets called whenever the active stage of the condition changes. Should only be used for visual changes.
    /// </summary>
    protected abstract void OnActiveStageChanged();

    /// <summary>
    /// Gets called at the end of each day. Performs all events that happen during the night and returns a list of them for the morning report.
    /// </summary>
    protected virtual void OnEndDay(MorningReport morningReport) { }

    /// <summary>
    /// Called when this health condition is removed from the player. Should be used to clean up or reset any visual elements or other things related to this condition.
    /// </summary>
    public virtual void OnRemoved() { }

    /// <summary>
    /// Determines whether the current state represents a fatal condition that ends the game.
    /// <br/>Returns the reason of death if fatal, null otherwise.
    /// </summary>
    public string IsFatal()
    {
        if (IsLethal && SeverityValue >= MaxSeverity) return LethalityMessage;
        else return null;
    }

    #region Getters
    public string Label => (ActiveStage != null && ActiveStage.Label != "") ? ActiveStage.Label : Def.Label;
    public string Description => (ActiveStage != null && ActiveStage.Description != "") ? ActiveStage.Description : Def.Description;

    public virtual string GetReportLabel() => Label;
    public virtual string GetReportDescription() => Description;
    public virtual Color GetReportTextColor() => ActiveStage.Color;
    public virtual Color GetReportBackgroundColor() => Color.clear;

    // Stat modifiers
    public virtual Dictionary<StatDef, int> GetCurrentModifiers() => ActiveStage.StatModifiers;

    public int GetStatModifierFor(StatDef stat)
    {
        Dictionary<StatDef, int> modifiers = GetCurrentModifiers();
        if (modifiers.ContainsKey(stat)) return modifiers[stat];
        else return 0;
    }

    #endregion
}
