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

    public void Init(EventStep step) 
    {
        Clear();
        EventText.text = step.Text;
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
    }

    private EventStep EndEvent(Game game)
    {
        game.EndEvent();
        return null;
    }

    public void DisplayMorningReport(List<string> nightEvents)
    {
        Clear();
        string text = "";
        if (Game.Day == 1) text = "After you saw the news you knew that you weren't save anymore at home. You ran outside, grabbed your handcart and so starts your journey.";
        else if (nightEvents.Count == 0) text = "You wake up in the " + HelperFunctions.GetEnumDescription(Game.CurrentLocation) + ". Nothing eventful happened during the night.";
        else
        {
            text = "You wake up in the " + HelperFunctions.GetEnumDescription(Game.CurrentLocation) + ". The following happened during the night:";
            foreach (string e in nightEvents) text += "\n" + e;
        }
        EventText.text = text;

        EventOption endMorningReportOption = new EventOption("Start day", EndMorningReport);
        UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
        optionDisplay.Init(Game, endMorningReportOption);
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
    }
}
