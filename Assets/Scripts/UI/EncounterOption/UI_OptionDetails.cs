using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_OptionDetails : MonoBehaviour
{
    public EncounterOption CurrentOption { get; private set; }

    [Header("Description")]
    public GameObject DescriptionPanel;
    public TextMeshProUGUI DescriptionText;

    [Header("Outcome Panel")]
    public GameObject OutcomesPanel;
    public GameObject OutcomeBarContainer;
    public GameObject OutcomeLabelContainer;

    public Image BarSegmentPrefab;
    public UI_LabelValueRow BarLabelPrefab;


    [Header("Difficulty Panel")]
    public GameObject DifficultyPanel;
    public TextMeshProUGUI DifficultyValueText;
    public GameObject DifficultyModifiersContainer;

    public UI_LabelValueRow DifficultyModifierPrefab;

    [Header("Requirements Panel")]
    public GameObject RequirementsPanel;
    public GameObject ItemCategory;
    public GameObject SkillCategory;
    public GameObject MiscCategory;

    public UI_RequirementsRow RequirementPrefab;

    public void ShowDetailsFor(EncounterOption option)
    {
        CurrentOption = option;
        Refresh();
    }

    public void Refresh()
    {
        DescriptionText.text = CurrentOption.Description;
        DescriptionPanel.SetActive(!string.IsNullOrEmpty(CurrentOption.Description));

        ShowSkillCheckProperties();
        ShowRequirements();
    }

    private void ShowSkillCheckProperties()
    {
        if (CurrentOption is SkillCheckOption skillCheckOption)
        {
            OutcomesPanel.SetActive(true);
            DifficultyPanel.SetActive(true);

            List<SkillCheckOutcomeChance> outcomes = skillCheckOption.GetOutcomeChances();

            // Outcome bar
            HelperFunctions.DestroyAllChildredImmediately(OutcomeBarContainer);
            foreach (SkillCheckOutcomeChance outcome in outcomes)
            {
                Image barSegment = Instantiate(BarSegmentPrefab, OutcomeBarContainer.transform);
                barSegment.color = outcome.Outcome.Color;
                barSegment.GetComponent<RectTransform>().anchorMin = new Vector2(0, outcome.MinRoll / 100f);
                barSegment.GetComponent<RectTransform>().anchorMax = new Vector2(1, outcome.MaxRoll / 100f);
            }

            // Outcome bar labels
            HelperFunctions.DestroyAllChildredImmediately(OutcomeLabelContainer);
            foreach (SkillCheckOutcomeChance outcome in outcomes)
            {
                if(outcome.Chance < 0.02f) continue; // Skip labels for very small chances to avoid cluttering the UI

                UI_LabelValueRow label = Instantiate(BarLabelPrefab, OutcomeLabelContainer.transform);
                label.Init(outcome.Label, $"{outcome.Chance * 100f:0}%");
                label.GetComponent<RectTransform>().anchorMin = new Vector2(0, outcome.MinRoll / 100f);
                label.GetComponent<RectTransform>().anchorMax = new Vector2(1, outcome.MaxRoll / 100f);
            }

            // Difficulty
            DifficultyValueText.text = skillCheckOption.GetDifficultyValue().ToString();

            // Modifier section
            HelperFunctions.DestroyAllChildredImmediately(DifficultyModifiersContainer);

            // Base difficulty
            UI_LabelValueRow baseValueLabel = Instantiate(DifficultyModifierPrefab, DifficultyModifiersContainer.transform);
            baseValueLabel.Init("Base Difficulty", skillCheckOption.Difficulty.ToString());
            baseValueLabel.SetBold(true);

            // Modifiers
            foreach (DifficultyModifier modifier in skillCheckOption.GetDifficultyModifiers())
            {
                UI_LabelValueRow label = Instantiate(DifficultyModifierPrefab, DifficultyModifiersContainer.transform);
                string labelText = modifier.Label;
                string valueText = modifier.Value > 0 ? $"+{modifier.Value}" : modifier.Value.ToString();
                label.Init(labelText, valueText);
            }
        }

        // Hide the panels if the option is not a skill check
        else
        {
            OutcomesPanel.SetActive(false);
            DifficultyPanel.SetActive(false);
        }
       
    }

    private void ShowRequirements()
    {
        // Hide the requirements panel if there are no requirements
        if (!CurrentOption.HasRequirements())
        {
            RequirementsPanel.gameObject.SetActive(false);
            return;
        }

        RequirementsPanel.gameObject.SetActive(true);

        // Item requirements
        List<ItemSlot> requiredItemSlots = CurrentOption.ItemSlots.Where(slot => slot.IsRequired).ToList();
        ItemCategory.SetActive(requiredItemSlots.Count > 0);
        HelperFunctions.DestroyAllChildredImmediately(ItemCategory, skipElements: 1);
        foreach (ItemSlot slot in requiredItemSlots)
        {
            UI_RequirementsRow elem = Instantiate(RequirementPrefab, ItemCategory.transform);
            elem.Init(slot.Label(), slot.IsFilled);
        }

        // Skill requirements
        SkillCategory.SetActive(CurrentOption.SkillRequirements.Count > 0);
        HelperFunctions.DestroyAllChildredImmediately(SkillCategory, skipElements: 1);
        foreach (var requirement in CurrentOption.SkillRequirements)
        {
            UI_RequirementsRow elem = Instantiate(RequirementPrefab, SkillCategory.transform);
            string label = $"{requirement.Key.DefName}: {requirement.Value}";
            bool isMet = Game.Instance.Player.GetStatValue(requirement.Key) >= requirement.Value;
            elem.Init(label, isMet);
        }

        // Misc requirements (not yet used)
        MiscCategory.SetActive(false);
        HelperFunctions.DestroyAllChildredImmediately(MiscCategory, skipElements: 1);
    }
}
