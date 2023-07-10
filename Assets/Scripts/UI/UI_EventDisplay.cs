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
            EventDialogueOption endDayOption = new EventDialogueOption("Continue journey", EndEvent);
            UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
            optionDisplay.Init(Game, endDayOption);
        }
        else
        {
            foreach (EventDialogueOption option in step.EventDialogueOptions)
            {
                UI_EventOption optionDisplay = Instantiate(EventOptionPrefab, EventOptionContainer.transform);
                optionDisplay.Init(Game, option);
            }
        }

        // Item changes
        if(Game.ItemsAddedSinceLastStep != null)
        {
            Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();
            foreach(Item item in Game.ItemsAddedSinceLastStep)
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
        if (Game.ItemsRemovedSinceLastStep != null)
        {
            Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();
            foreach (Item item in Game.ItemsRemovedSinceLastStep)
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
        Game.EndAfternoonEvent();
        return null;
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(EventOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(EventItemChangeContainer);
    }
}
