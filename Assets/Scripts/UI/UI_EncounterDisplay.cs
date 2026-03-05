using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EncounterDisplay : MonoBehaviour
{
    public Game Game;
    public static UI_EncounterDisplay Instance;

    [Header("Elements")]
    public TextMeshProUGUI EventText;
    public GameObject EventOptionContainer;
    public TextMeshProUGUI HoveredOptionDescriptionText;

    public GameObject OutcomeNotesContainer;
    public UI_OptionDetails OptionDetailsPanel;
    public UI_ItemSlotDetailsBox ItemSlotDetailsBox;

    [Header("Prefabs")]
    public UI_EncounterStepOption EventOptionPrefab;
    public UI_EventOutcomeNote OutcomeNotePrefab;

    public Dictionary<EncounterStepOption, UI_EncounterStepOption> OptionDisplays;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(EncounterStep step) 
    {
        Clear();
        EventText.text = step.Text;

        // Dialogue Options
        OptionDisplays = new Dictionary<EncounterStepOption, UI_EncounterStepOption>();
        if (step.IsFinalStep)
        {
            FixedOutcomeOption endDayOption = new FixedOutcomeOption("Continue journey", "Continue your day.", EndEvent);
            UI_EncounterStepOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(this, endDayOption);
            OptionDisplays.Add(endDayOption, optionDisplay);
        }
        else
        {
            foreach (EncounterStepOption option in step.Options)
            {
                UI_EncounterStepOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(this, option);
                OptionDisplays.Add(option, optionDisplay);
            }
        }

        HideOptionDetails();
        HoveredOptionDescriptionText.gameObject.SetActive(false);
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
        // Show description
        HoveredOptionDescriptionText.gameObject.SetActive(true);
        HoveredOptionDescriptionText.text = option.Description;

        // Skill check stuff
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
        // Hide description
        HoveredOptionDescriptionText.gameObject.SetActive(false);

        // Unhighlight all stats
        GameUI.Instance.UnhighlightAllStats();

        // Hide option details
        HideOptionDetails();
    }

    public void OnItemSlotHovered(ItemSlot itemSlot)
    {
        ItemSlotDetailsBox.Show(itemSlot);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ItemSlotDetailsBox.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    public void OnItemSlotUnhovered()
    {
        ItemSlotDetailsBox.Hide();
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(EventOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(OutcomeNotesContainer);
    }

    public void RefreshOption(EncounterStepOption option)
    {
        OptionDisplays[option].Resfresh();

        // Option details
        if (OptionDetailsPanel.gameObject.activeSelf) OptionDetailsPanel.Refresh();

        // Item slot
        if (ItemSlotDetailsBox.gameObject.activeSelf) ItemSlotDetailsBox.Refresh();
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

        // Added wounds
        Dictionary<Wound, int> groupedWounds = new Dictionary<Wound, int>();
        foreach (Wound wound in Game.WoundsAddedSinceLastStep)
        {
            if (!groupedWounds.Any(x => x.Key.Def == wound.Def)) groupedWounds.Add(wound, 1);
            else groupedWounds[groupedWounds.First(x => x.Key.Def == wound.Def).Key]++;
        }
        foreach (var group in groupedWounds)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(group.Key.SpriteBase, true, group.Value);
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
        ItemSlotDetailsBox.Hide();
    }
    #endregion
}
