using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_OptionDetails : MonoBehaviour
{
    [Header("Outcome Panel")]
    public GameObject OutcomeBarContainer;
    public GameObject OutcomeLabelContainer;

    public Image BarSegmentPrefab;
    public UI_LabelValueRow BarLabelPrefab;


    [Header("Difficulty Panel")]
    public TextMeshProUGUI DifficultyValueText;
    public GameObject DifficultyModifiersContainer;

    public UI_LabelValueRow DifficultyModifierPrefab;

    public void ShowDetailsFor(SkillCheckOption option)
    {
        List<SkillCheckOutcomeChance> outcomes = option.GetOutcomeChances();

        // Outcome bar
        HelperFunctions.DestroyAllChildredImmediately(OutcomeBarContainer);
        foreach(SkillCheckOutcomeChance outcome in outcomes)
        {
            Image barSegment = Instantiate(BarSegmentPrefab, OutcomeBarContainer.transform);
            barSegment.color = outcome.Outcome.Color;
            barSegment.GetComponent<RectTransform>().anchorMin = new Vector2(0, outcome.MinRoll / 100f);
            barSegment.GetComponent<RectTransform>().anchorMax = new Vector2(1, outcome.MaxRoll / 100f);
        }

        // Outcome bar labels
        HelperFunctions.DestroyAllChildredImmediately(OutcomeLabelContainer);
        foreach(SkillCheckOutcomeChance outcome in outcomes)
        {
            UI_LabelValueRow label = Instantiate(BarLabelPrefab, OutcomeLabelContainer.transform);
            label.Init(outcome.Label, $"{outcome.Chance * 100f:0}%");
            label.GetComponent<RectTransform>().anchorMin = new Vector2(0, outcome.MinRoll / 100f);
            label.GetComponent<RectTransform>().anchorMax = new Vector2(1, outcome.MaxRoll / 100f);
        }

        // Difficulty
        DifficultyValueText.text = option.GetDifficultyValue().ToString();

        // Modifier section
        HelperFunctions.DestroyAllChildredImmediately(DifficultyModifiersContainer);

        // Base difficulty
        UI_LabelValueRow baseValueLabel = Instantiate(DifficultyModifierPrefab, DifficultyModifiersContainer.transform);
        baseValueLabel.Init("Base Difficulty", option.BaseDifficulty.ToString());
        baseValueLabel.SetBold(true);

        // Modifiers
        foreach (KeyValuePair<string, int> modifier in option.GetDifficultyModifiers())
        {
            UI_LabelValueRow label = Instantiate(DifficultyModifierPrefab, DifficultyModifiersContainer.transform);
            string labelText = modifier.Key;
            string valueText = modifier.Value > 0 ? $"+{modifier.Value}" : modifier.Value.ToString();
            label.Init(labelText, valueText);
        }
    }
}
