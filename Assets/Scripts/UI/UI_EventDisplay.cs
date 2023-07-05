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
    public GameObject EventItemChangeContainer;

    [Header("Prefabs")]
    public UI_EventOption EventOptionPrefab;
    public UI_EventItemChange EventItemChangePrefab;
    

    public void Init(EventStep step) 
    {
        Clear();
        EventText.text = step.Text;

        // Dialogue Options
        if (step.IsFinalStep)
        {
            EventOption endDayOption = new EventOption("Continue journey", EndEvent);
            UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(Game, endDayOption);
        }
        else
        {
            foreach (EventOption option in step.EventDialogueOptions)
            {
                UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(Game, option);
            }
        }

        // Item changes
        if(step.AddedItems != null)
        {
            Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();
            foreach(Item item in step.AddedItems)
            {
                if (!groupedItems.Any(x => x.Key.Type == item.Type)) groupedItems.Add(item, 1);
                else groupedItems[groupedItems.First(x => x.Key.Type == item.Type).Key]++;
            }
            foreach(KeyValuePair<Item, int> item in groupedItems)
            {
                UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
                itemChange.Init(item.Key, true, item.Value);
            }
        }
        if (step.RemovedItems != null)
        {
            Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();
            foreach (Item item in step.RemovedItems)
            {
                if (!groupedItems.Any(x => x.Key.Type == item.Type)) groupedItems.Add(item, 1);
                else groupedItems[groupedItems.First(x => x.Key.Type == item.Type).Key]++;
            }
            foreach (KeyValuePair<Item, int> item in groupedItems)
            {
                UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
                itemChange.Init(item.Key, false, item.Value);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private EventStep EndEvent()
    {
        Game.EndEvent();
        return null;
    }

    public void DisplayMorningReport(MorningReport morningReport)
    {
        Clear();

        // Night events
        string text = "";
        if (Game.Day == 1) text = "After you saw the news you knew that you weren't save anymore at home. You ran outside, grabbed your handcart and so starts your journey.";
        else if (morningReport.NightEvents.Count == 0) text = "You wake up in the " + Game.CurrentLocation.Name + ". Nothing eventful happened during the night.";
        else
        {
            text = "You wake up in the " + Game.CurrentLocation.Name + ". The following happened during the night:";
            foreach (string e in morningReport.NightEvents) text += "\n" + e;
        }
        EventText.text = text;

        // Added/Removed items
        foreach (Item item in morningReport.AddedItems)
        {
            UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
            itemChange.Init(item, true);
        }
        foreach (Item item in morningReport.RemovedItems)
        {
            UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
            itemChange.Init(item, false);
        }

        // Start day option
        EventOption endMorningReportOption = new EventOption("Start day", EndMorningReport);
        UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
        optionDisplay.Init(Game, endMorningReportOption);

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private EventStep EndMorningReport()
    {
        Game.EndMorningReport();
        return null;
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(EventOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(EventItemChangeContainer);
    }
}
