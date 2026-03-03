using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EncounterDisplay : MonoBehaviour
{
    public Game Game;

    [Header("Elements")]
    public TextMeshProUGUI EventText;
    public GameObject EventOptionContainer;
    public GameObject OutcomeNotesContainer;
    public UI_OptionDetails OptionDetailsPanel;

    [Header("Prefabs")]
    public UI_EncounterStepOption EventOptionPrefab;
    public UI_EventOutcomeNote OutcomeNotePrefab;
    

    public void Init(EncounterStep step) 
    {
        Clear();
        EventText.text = step.Text;

        // Dialogue Options
        if (step.IsFinalStep)
        {
            FixedOutcomeOption endDayOption = new FixedOutcomeOption("Continue journey", EndEvent);
            UI_EncounterStepOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(this, endDayOption);
        }
        else
        {
            foreach (EncounterStepOption option in step.Options)
            {
                UI_EncounterStepOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(this, option);
            }
        }

        HideOptionDetails();
        InitEventStepOutcomeNotes();

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private EncounterStep EndEvent()
    {
        Game.EndAfternoonEvent();
        return null;
    }

    public void OnOptionHovered(EncounterStepOption option)
    {
        if (option is SkillCheckOption skillCheckOption)
        {
            // Highlight associated stats
            foreach (StatDef stat in skillCheckOption.RelevantStats.Keys) GameUI.Instance.HightlightStat(stat);

            // Show option details
            ShowOptionDetails(skillCheckOption);
        }
    }

    public void OnOptionUnhovered()
    {
        // Unhighlight all stats
        GameUI.Instance.UnhighlightAllStats();

        // Hide option details
        HideOptionDetails();
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(EventOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(OutcomeNotesContainer);
    }

    #region Outcome Notes

    private void InitEventStepOutcomeNotes()
    {
        // Added items
        Dictionary<Item, int> groupedAddedItems = new Dictionary<Item, int>();
        foreach (Item item in Game.ItemsAddedSinceLastStep)
        {
            if (!groupedAddedItems.Any(x => x.Key.Def == item.Def)) groupedAddedItems.Add(item, 1);
            else groupedAddedItems[groupedAddedItems.First(x => x.Key.Def == item.Def).Key]++;
        }
        foreach (KeyValuePair<Item, int> item in groupedAddedItems)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(item.Key.Sprite, true, item.Value);
        }

        // Removed items
        Dictionary<Item, int> groupedRemovedItems = new Dictionary<Item, int>();
        foreach (Item item in Game.ItemsRemovedSinceLastStep)
        {
            if (!groupedRemovedItems.Any(x => x.Key.Def == item.Def)) groupedRemovedItems.Add(item, 1);
            else groupedRemovedItems[groupedRemovedItems.First(x => x.Key.Def == item.Def).Key]++;
        }
        foreach (KeyValuePair<Item, int> item in groupedRemovedItems)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(item.Key.Sprite, false, item.Value);
        }

        // Added injuries
        Dictionary<Wound, int> groupedInjuries = new Dictionary<Wound, int>();
        foreach (Wound wound in Game.InjuriesAddedSinceLastStep)
        {
            if (!groupedInjuries.Any(x => x.Key.Def == wound.Def)) groupedInjuries.Add(wound, 1);
            else groupedInjuries[groupedInjuries.First(x => x.Key.Def == wound.Def).Key]++;
        }
        foreach (var group in groupedInjuries)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(group.Key.GetSprite(), true, group.Value);
        }
    }

    #endregion

    #region Option Details

    public void ShowOptionDetails(SkillCheckOption option)
    {
        OptionDetailsPanel.gameObject.SetActive(true);
        OptionDetailsPanel.ShowDetailsFor(option);
    }

    public void HideOptionDetails()
    {
        OptionDetailsPanel.gameObject.SetActive(false);
    }
    #endregion
}
