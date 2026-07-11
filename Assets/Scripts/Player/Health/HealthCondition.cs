using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HealthCondition
{
    private const float NATURAL_HEALING_RANDOM_OFFSET = 0.1f; // in %, both directions

    public Game Game => Player.Game;
    public PlayerCharacter Player { get; private set; }
    public HealthConditionDef Def { get; private set; }
    public virtual List<HealthConditionStage> Stages => Def.Stages;
    public HealthConditionStage ActiveStage { get; private set; }
    public int OriginDay { get; private set; }
    public int ActiveStageIndex => Stages.IndexOf(ActiveStage);
    public List<string> Source { get; private set; }

    // Def overrides
    public virtual float GetNaturalHealing() => Def.NaturalHealing;
    public virtual float InitialSeverity => Def.DefaultInitialSeverity;
    public virtual float MaxSeverity => Def.MaxSeverity;
    public virtual bool IsLethal => Def.IsLethal;
    public bool IsNeed => Def.IsVital;
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
        Source = new List<string>();
        OnInit();

        if (SeverityValue <= 0) throw new System.Exception($"Health condition {Def} cannot be initialized with non-positive severity value as that would remove it immediately.");

        UpdateStage();
    }

    /// <summary>
    /// Executes the end of day effect for this health condition. This includes applying natural severity changes and any specific effects defined in the OnEndDay method.
    /// </summary>
    public void ExecuteEndDayEffect(MorningReport morningReport)
    {
        // End of day severity change (excluding natural healing as that is done separately in advance)
        float endOfDaySeverityChange = GetEndOfDaySeverityChange(excludeNaturalHealing: true);
        if (endOfDaySeverityChange != 0f) ModifySeverity(endOfDaySeverityChange);

        // Specific effects
        OnEndDay(morningReport);
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
        if (SeverityValue <= 0f && !Def.IsVital)
        {
            OnRemoved();
            Player.RemoveHealthCondition(this);
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

        // If current stage is hidden, clear all sources
        if (!ActiveStage.IsVisible) Source.Clear();

        Debug.Log($"Updated active stage of {Def.DefName} to {ActiveStage.Label} based on severity value of {SeverityValue}. ActiveStageIndex is now {ActiveStageIndex}.");
        OnActiveStageChanged();
    }

    /// <summary>
    /// Applies natural healing to this health condition based on its natural healing rate and the given healing factor. The healing factor can be used to modify the amount of healing applied (e.g., if the player is resting, the healing factor might be higher). A random offset is applied to the healing amount to add some variability. If the resulting healing amount is greater than 0, it will reduce the severity of the condition accordingly.
    /// </summary>
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

    public float GetEndOfDaySeverityChange(bool excludeNaturalHealing = false)
    {
        // Natural severity change defined in the health condition definition
        float endOfDaySeverityChange = Def.NaturalSeverityChange;

        // Natural Healing
        if (!excludeNaturalHealing) endOfDaySeverityChange -= GetNaturalHealing();

        // Modifications from other health conditions
        foreach (HealthCondition hc in Player.HealthConditions)
        {
            if (hc == this) continue; // Skip self

            Dictionary<HealthConditionDef, float> endOfDayChanges = hc.GetCurrentEndOfDayVitalChanges();
            if (endOfDayChanges.ContainsKey(Def)) endOfDaySeverityChange += endOfDayChanges[Def];
        }

        return endOfDaySeverityChange;
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

    /// <summary>
    /// Returns the amount of days remaining until this health condition is resolved (severity reaches 0) under current conditions.
    /// </summary>
    public int GetRemainingDurationInDays()
    {
        float severityChangePerDay = GetEndOfDaySeverityChange();
        if (severityChangePerDay >= 0f) return -1; // Severity is not decreasing, so the condition will never end
        else return Mathf.CeilToInt(SeverityValue / -severityChangePerDay);
    }

    /// <summary>
    /// Returns the amount of days remaining until this health condition reaches the next stage (severity reaches the next stage's threshold) under current conditions.
    /// </summary>
    public int GetDaysUntilWorsening()
    {
        float severityChangePerDay = GetEndOfDaySeverityChange();
        if (severityChangePerDay <= 0f) return -1; // Severity is not increasing, so the condition will never worsen
        if (ActiveStageIndex >= Stages.Count - 1) return -1; // Already at the last stage, so it cannot worsen

        float nextStageThreshold = Stages[ActiveStageIndex + 1].SeverityThreshold;
        float severityUntilNextStage = nextStageThreshold - SeverityValue;
        return Mathf.CeilToInt(severityUntilNextStage / severityChangePerDay);
    }

    /// <summary>
    /// Returns the amount of days remaining until this health condition reaches the previous stage (severity reaches the previous stage's threshold) under current conditions.
    /// </summary>
    public int GetDaysUntilImprovement()
    {
        float severityChangePerDay = GetEndOfDaySeverityChange();
        if (severityChangePerDay >= 0f) return -1; // Severity is not decreasing, so the condition will never improve
        if (ActiveStageIndex <= 0) return -1; // Already at the first stage, so it cannot improve

        float previousStageThreshold = Stages[ActiveStageIndex - 1].SeverityThreshold;
        float severityUntilPreviousStage = SeverityValue - previousStageThreshold;
        return Mathf.CeilToInt(severityUntilPreviousStage / -severityChangePerDay);
    }

    /// <summary>
    /// Returns the amount of days remaining until this health condition causes death (severity reaches MaxSeverity) under current conditions.
    /// </summary>
    public int GetDaysUntilDeath()
    {
        float severityChangePerDay = GetEndOfDaySeverityChange();
        if (!IsLethal || severityChangePerDay >= 0f) return -1; // Not lethal or severity is not increasing, so the condition will never cause death

        float severityUntilDeath = MaxSeverity - SeverityValue;
        return Mathf.CeilToInt(severityUntilDeath / severityChangePerDay);
    }

    public string Label => (ActiveStage != null && ActiveStage.Label != "") ? ActiveStage.Label : Def.Label;
    public string Description => (ActiveStage != null && ActiveStage.Description != "") ? ActiveStage.Description : Def.Description;
    public virtual string GetInterActionsString() => Def.Interactions;

    public virtual string GetReportLabel() => Label;
    public virtual string GetReportDescription() => Description;
    public virtual Color GetReportTextColor() => ActiveStage.Color;
    public virtual Color GetReportBackgroundColor() => Color.clear;

    public string GetTrendAsString()
    {
        float delta = GetEndOfDaySeverityChange();

        // Special cases
        if (this is HC_Hunger hunger && ActiveStageIndex == 0) return "Fading"; // Well fed
        if (this is HC_Thirst thirst && ActiveStageIndex == 0) return "Fading"; // Well hydrated

        if (delta > 0f) return IsNegative ? "Worsening" : "Intensifying";
        else if (delta < 0f) return IsNegative ? "Improving" : "Fading";
        else return "Stable";
    }

    /// <summary>
    /// Returns all sources of this health condition as a single string.
    /// </summary>
    public string GetSourcesAsSingleString()
    {
        // Take the sources from this health condition first
        List<string> sources = new List<string>(Source);

        // Fetch other health conditions as sources
        foreach (HealthCondition hc in Player.HealthConditions.Where(hc => hc != this && hc.ActiveStage.IsVisible))
        {
            Dictionary<HealthConditionDef, float> endOfDayChanges = hc.GetCurrentEndOfDayVitalChanges();
            if (endOfDayChanges.ContainsKey(Def) && !sources.Contains(hc.Label))
            {
                sources.Add(hc.Label);
            }
        }

        // Group sources by name and count duplicates
        Dictionary<string, int> sourceGroups = new Dictionary<string, int>();
        foreach (string source in sources)
        {
            if (sourceGroups.ContainsKey(source)) sourceGroups[source]++;
            else sourceGroups[source] = 1;
        }

        // Display each group as one line, append " (xN)" if there are multiple sources of the same name
        List<string> lines = new List<string>();
        foreach (var kvp in sourceGroups)
        {
            if (kvp.Value > 1) lines.Add($"- {kvp.Key} (x{kvp.Value})");
            else lines.Add($"- {kvp.Key}");
        }

        // Make into single string
        string result = string.Join("\n", lines).Trim();
        return result;
    }

    // Stat modifiers
    public virtual Dictionary<StatDef, int> GetStatCurrentModifiers() => ActiveStage.StatModifiers;

    public int GetStatModifierFor(StatDef stat)
    {
        Dictionary<StatDef, int> modifiers = GetStatCurrentModifiers();
        if (modifiers.ContainsKey(stat)) return modifiers[stat];
        else return 0;
    }

    public virtual Dictionary<HealthConditionDef, float> GetCurrentEndOfDayVitalChanges() => ActiveStage.EndOfDayVitalChanges;

    #endregion
}
