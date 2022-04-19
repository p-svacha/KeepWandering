using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventDisplay : MonoBehaviour
{
    public Game Game;

    public Text EventText;
    public GameObject EventOptionContainer;
    public UI_EventOption EventOptionPrefab;

    public GameObject EventItemChangeContainer;
    public UI_EventItemChange EventItemChangePrefab;
    

    public void Init(EventStep step) 
    {
        Clear();
        EventText.text = step.Text;

        // Dialogue Options
        if (step.EventDialogueOptions != null)
        {
            foreach (EventOption option in step.EventDialogueOptions)
            {
                UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(Game, option);
            }
        }
        else
        {
            EventOption endDayOption = new EventOption("Continue journey", EndEvent);
            UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(Game, endDayOption);
        }

        // Item changes
        if(step.AddedItems != null)
        {
            foreach(Item item in step.AddedItems)
            {
                UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
                itemChange.Init(item, true);
            }
        }
        if (step.RemovedItems != null)
        {
            foreach (Item item in step.RemovedItems)
            {
                UI_EventItemChange itemChange = Instantiate(EventItemChangePrefab, EventItemChangeContainer.transform);
                itemChange.Init(item, false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private EventStep EndEvent(Game game)
    {
        game.EndEvent();
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

    private EventStep EndMorningReport(Game game)
    {
        game.EndMorningReport();
        return null;
    }

    private void Clear()
    {
        foreach (Transform child in EventOptionContainer.transform)
        {
            if (child.GetSiblingIndex() >= 2) Destroy(child.gameObject);
        }

        foreach (Transform child in EventItemChangeContainer.transform) Destroy(child.gameObject);
    }
}
