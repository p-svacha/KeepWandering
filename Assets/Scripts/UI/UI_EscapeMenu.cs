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

    public void Init(Game game)
    {
        Game = game;

        // Add item
        List<Dropdown.OptionData> itemOptions = new List<Dropdown.OptionData>();
        foreach (ItemDef itemDef in DefDatabase<ItemDef>.AllDefs)
        {
            itemOptions.Add(new Dropdown.OptionData(itemDef.DefName));
        }
        AddItemDropdown.options = itemOptions;
        AddItemButton.onClick.AddListener(AddItem);

        // Force event
        List<Dropdown.OptionData> eventOptions = new List<Dropdown.OptionData>();
        eventOptions.Add(new Dropdown.OptionData("No Force"));
        foreach (EncounterDef encounterDef in DefDatabase<EncounterDef>.AllDefs)
        {
            eventOptions.Add(new Dropdown.OptionData(encounterDef.DefName));
        }
        ForceEventDropdown.options = eventOptions;
        ForceEventDropdown.onValueChanged.AddListener(ForceEncounter);
    }

    private void AddItem()
    {
        Game.AddNewItemToInventory(DefDatabase<ItemDef>.AllDefs[AddItemDropdown.value]);
    }

    private void ForceEncounter(int value)
    {
        Game.EncounterManager.ForceEncounter(value == 0 ? null : DefDatabase<EncounterDef>.AllDefs[value - 1]);
    }
}
