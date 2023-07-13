using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EventDisplay : MonoBehaviour
{
    public Game Game;

    [Header("Elements")]
    public TextMeshProUGUI EventText;
    public GameObject EventOptionContainer;
    public GameObject OutcomeNotesContainer;

    [Header("Prefabs")]
    public UI_EventDialogueOption EventOptionPrefab;
    public UI_EventOutcomeNote OutcomeNotePrefab;
    

    public void Init(EventStep step) 
    {
        Clear();
        EventText.text = step.Text;

        // Dialogue Options
        if (step.IsFinalStep)
        {
            EventDialogueOption endDayOption = new EventDialogueOption("Continue journey", EndEvent);
            UI_EventDialogueOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(Game, endDayOption);
        }
        else
        {
            foreach (EventDialogueOption option in step.EventDialogueOptions)
            {
                UI_EventDialogueOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(Game, option);
            }
        }

        InitEventStepOutcomeNotes();

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void InitEventStepOutcomeNotes()
    {
        // Added items
        Dictionary<Item, int> groupedAddedItems = new Dictionary<Item, int>();
        foreach (Item item in Game.ItemsAddedSinceLastStep)
        {
            if (!groupedAddedItems.Any(x => x.Key.Type == item.Type)) groupedAddedItems.Add(item, 1);
            else groupedAddedItems[groupedAddedItems.First(x => x.Key.Type == item.Type).Key]++;
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
            if (!groupedRemovedItems.Any(x => x.Key.Type == item.Type)) groupedRemovedItems.Add(item, 1);
            else groupedRemovedItems[groupedRemovedItems.First(x => x.Key.Type == item.Type).Key]++;
        }
        foreach (KeyValuePair<Item, int> item in groupedRemovedItems)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(item.Key.Sprite, false, item.Value);
        }

        // Added injuries
        Dictionary<Injury, int> groupedInjuries = new Dictionary<Injury, int>();
        foreach (Injury injury in Game.InjuriesAddedSinceLastStep)
        {
            if (!groupedInjuries.Any(x => x.Key.Type == injury.Type)) groupedInjuries.Add(injury, 1);
            else groupedInjuries[groupedInjuries.First(x => x.Key.Type == injury.Type).Key]++;
        }
        foreach (var group in groupedInjuries)
        {
            UI_EventOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(group.Key.GetSprite(), true, group.Value);
        }
    }

    private EventStep EndEvent()
    {
        Game.EndAfternoonEvent();
        return null;
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(EventOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(OutcomeNotesContainer);
    }
}
