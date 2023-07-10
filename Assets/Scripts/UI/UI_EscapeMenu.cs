using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EscapeMenu : MonoBehaviour
{
    private Game Game;

    [Header("Add Item")]
    public Dropdown AddItemDropdown;
    public Button AddItemButton;

    [Header("Force Event")]
    public Dropdown ForceEventDropdown;
    public Dropdown ForceLocationEventDropdown;

    public void Init(Game game)
    {
        Game = game;

        // Add item
        List<Dropdown.OptionData> itemOptions = new List<Dropdown.OptionData>();
        foreach (Item item in game.ItemPrefabs)
            itemOptions.Add(new Dropdown.OptionData(item.Name));
        AddItemDropdown.options = itemOptions;
        AddItemButton.onClick.AddListener(AddItem);

        // Force event
        List<Dropdown.OptionData> eventOptions = new List<Dropdown.OptionData>();
        eventOptions.Add(new Dropdown.OptionData("No Force"));
        foreach(Event encounter in Game.EventManager.DummyEvents.Values)
            eventOptions.Add(new Dropdown.OptionData(encounter.ToString()));
        ForceEventDropdown.options = eventOptions;
        ForceEventDropdown.onValueChanged.AddListener(ForceEvent);

        /*
        // Force location event
        List<Dropdown.OptionData> locEventOptions = new List<Dropdown.OptionData>();
        locEventOptions.Add(new Dropdown.OptionData("No Force"));
        foreach (LocationEventType eventType in System.Enum.GetValues(typeof(LocationEventType)))
            locEventOptions.Add(new Dropdown.OptionData(eventType.ToString()));
        ForceLocationEventDropdown.options = locEventOptions;
        ForceLocationEventDropdown.onValueChanged.AddListener(ForceLocationEvent);
        */
    }

    private void AddItem()
    {
        Game.AddItemToInventory(Game.GetItemInstance(Game.ItemPrefabs[AddItemDropdown.value].Type));
    }

    private void ForceEvent(int value)
    {
        if (value == 0) Game.EventManager.SetForcedEvent(-1);
        else Game.EventManager.SetForcedEvent(value - 1);
    }

    /*
    private void ForceLocationEvent(int value)
    {
        if (value == 0) Game.HasForcedLocationEvent = false;
        else
        {
            Game.HasForcedLocationEvent = true;
            Game.ForcedLocationEventType = (LocationEventType)(value - 1);
        }
    }
    */
}
