using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthConditionTooltip : UI_TooltipBase
{
    public static UI_HealthConditionTooltip Instance;
    private HealthCondition HealthCondition;

    [Header("Elements")]
    public Image Icon;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI SubtitleText;

    public GameObject CurrentEffectsDivider;
    public TextMeshProUGUI CurrentEffectsText;

    public GameObject DetailsDivider;
    public GameObject DetailsContainer;
    public GameObject RelatedConditionRow;
    public TextMeshProUGUI RelatedConditionText;
    public TextMeshProUGUI SeverityText;
    public GameObject NaturalChangeRow;
    public TextMeshProUGUI NaturalChangeText;
    public GameObject NaturalHealingRow;
    public TextMeshProUGUI NaturalHealingText;

    public GameObject ProgressionDivider;
    public GameObject ProgressionTrendRow;
    public TextMeshProUGUI ProgressionTrendText;
    public GameObject ProgressionRemainingDurationRow;
    public TextMeshProUGUI ProgressionRemainingDurationText;
    public GameObject ProgressionWorseningInRow;
    public TextMeshProUGUI ProgressionWorseningInLabel;
    public TextMeshProUGUI ProgressionWorseningInText;
    public GameObject ProgressionImprovingInRow;
    public TextMeshProUGUI ProgressionImprovingInLabel;
    public TextMeshProUGUI ProgressionImprovingInText;
    public GameObject ProgressionLethalInRow;
    public TextMeshProUGUI ProgressionLethalInText;
    public GameObject ProgressionCurrentConditionsInfo;

    public GameObject InteractionsDivider;
    public TextMeshProUGUI InteractionsText;

    public GameObject SourceDivider;
    public TextMeshProUGUI SourceText;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(HealthCondition healthCondition)
    {
        gameObject.SetActive(true);
        HealthCondition = healthCondition;

        // Header
        Icon.sprite = healthCondition.Sprite;
        TitleText.text = healthCondition.Label;
        TitleText.color = healthCondition.GetReportTextColor();
        SubtitleText.text = healthCondition.Description;

        // Details
        bool showDetails = !healthCondition.IsSimpleBinaryCondition();
        DetailsDivider.SetActive(showDetails);
        DetailsContainer.SetActive(showDetails);
        if (showDetails)
        {
            string severityFormat = "0.0";

            RelatedConditionRow.SetActive(healthCondition.Def.Stages.Count > 1);
            RelatedConditionText.text = healthCondition.Def.Label;

            SeverityText.text = $"{healthCondition.SeverityValue.ToString(severityFormat)} / {healthCondition.Def.MaxSeverity.ToString(severityFormat)}";

            float naturalChange = healthCondition.GetEndOfDaySeverityChange(excludeNaturalHealing: true);
            NaturalChangeText.text = $"{naturalChange.ToString($"+{severityFormat};-{severityFormat};{severityFormat}")} per day";
            NaturalChangeRow.SetActive(naturalChange != 0);

            float naturalHealing = healthCondition.GetNaturalHealing();
            NaturalHealingText.text = $"-{naturalHealing.ToString(severityFormat)}";
            NaturalHealingRow.SetActive(naturalHealing != 0);
        }

        // Current Effects
        Dictionary<StatDef, int> statModifiers = healthCondition.GetStatCurrentModifiers();
        Dictionary<HealthConditionDef, float> endOfDayVitalChanges = healthCondition.GetCurrentEndOfDayVitalChanges();
        List<(HealthConditionDef Condition, float Chance)> appliedHealthConditions = healthCondition.GetCurrentAppliedHealthConditions();
        (int Modifier, float Chance)? skillCheckModifier = healthCondition.GetCurrentSkillCheckModifier();

        bool hasCurrentEffects = statModifiers.Count > 0 || endOfDayVitalChanges.Count > 0 || appliedHealthConditions.Count > 0 || skillCheckModifier.HasValue;
        CurrentEffectsDivider.SetActive(hasCurrentEffects);
        CurrentEffectsText.gameObject.SetActive(hasCurrentEffects);
        if (hasCurrentEffects)
        {
            string currentEffectsText = "";

            // Stat modifiers
            if (statModifiers.Count > 0)
            {
                foreach (var statChange in statModifiers)
                {
                    currentEffectsText += $"\n{statChange.Value.ToSignedString()} {statChange.Key.LabelCap}";
                }
            }

            // End of day vital changes
            if (endOfDayVitalChanges.Count > 0)
            {
                foreach (var vitalChange in endOfDayVitalChanges)
                {
                    bool isIncrease = vitalChange.Value > 0;
                    string increases = isIncrease ? "Increases" : "Decreases";
                    currentEffectsText += $"\n{increases} {vitalChange.Key.Label} each night";
                }
            }

            // Applied health conditions
            if (appliedHealthConditions.Count > 0)
            {
                foreach (var appliedCondition in appliedHealthConditions)
                {
                    currentEffectsText += $"\n{appliedCondition.Chance * 100}% chance to apply {appliedCondition.Condition.Label} each night";
                }
            }

            // Skill check modifier
            if (skillCheckModifier.HasValue)
            {
                currentEffectsText += $"\n{skillCheckModifier.Value.Chance * 100}% chance to modify skill check rolls by {skillCheckModifier.Value.Modifier}";
            }

            CurrentEffectsText.text = currentEffectsText.TrimStart('\n');
        }

        // Progression
        float severityChange = healthCondition.GetEndOfDaySeverityChange();

        ProgressionTrendRow.SetActive(true);
        string trend = healthCondition.GetTrendAsString();
        ProgressionTrendText.text = trend;

        int remainingDuration = healthCondition.GetRemainingDurationInDays();
        bool hasRemainingDuration = remainingDuration > 0;
        ProgressionRemainingDurationRow.SetActive(hasRemainingDuration);
        if (hasRemainingDuration) ProgressionRemainingDurationText.text = $"{remainingDuration} {"day".Pluralize(remainingDuration)}*";

        int worseningIn = healthCondition.GetDaysUntilWorsening();
        bool hasWorseningIn = worseningIn > 0;
        ProgressionWorseningInRow.SetActive(hasWorseningIn);
        if (hasWorseningIn)
        {
            ProgressionWorseningInLabel.text = $"{trend} in";
            ProgressionWorseningInText.text = $"{worseningIn} {"day".Pluralize(worseningIn)}*";
        }

        int improvingIn = healthCondition.GetDaysUntilImprovement();
        bool hasImprovingIn = improvingIn > 0;
        ProgressionImprovingInRow.SetActive(hasImprovingIn);
        if (hasImprovingIn)
        {
            ProgressionImprovingInLabel.text = $"{trend} in";
            ProgressionImprovingInText.text = $"{improvingIn} {"day".Pluralize(improvingIn)}*";
        }

        int lethalIn = healthCondition.GetDaysUntilDeath();
        bool hasLethalIn = lethalIn > 0;
        ProgressionLethalInRow.SetActive(hasLethalIn);
        if (hasLethalIn) ProgressionLethalInText.text = $"{lethalIn} {"day".Pluralize(lethalIn)}*";

        bool hasProgressionInfo = hasRemainingDuration || hasWorseningIn || hasImprovingIn || hasLethalIn;
        ProgressionCurrentConditionsInfo.SetActive(hasProgressionInfo);

        // Interactions
        string interactionsString = healthCondition.GetInterActionsString();
        bool hasInteractions = !string.IsNullOrEmpty(interactionsString);
        InteractionsDivider.SetActive(hasInteractions);
        InteractionsText.gameObject.SetActive(hasInteractions);
        if (hasInteractions) InteractionsText.text = interactionsString;

        // Sources
        string sourceString = healthCondition.GetSourcesAsSingleString();
        bool hasSources = !string.IsNullOrEmpty(sourceString);
        SourceDivider.SetActive(hasSources);
        SourceText.gameObject.SetActive(hasSources);
        if (hasSources) SourceText.text = sourceString;
    }
}
