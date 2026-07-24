using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DevmodeMenu : MonoBehaviour
{
    private Game Game;

    [Header("Elements")]
    public Button CloseButton;

    public TMP_Dropdown AddItemDropdown;
    public Button AddItemButton;

    public TMP_Dropdown ForceEventDropdown;

    public TMP_Dropdown HealthConditionDropwdown;
    public Button ApplyHealthConditionButton;
    public Button ApplyArmFractureButton;
    public Button ApplyLegFractureButton;
    public Button ApplyCutWoundButton;
    public Button ApplyBruiseWoundButton;

    public void Init(Game game)
    {
        Game = game;

        // General
        CloseButton.onClick.AddListener(() => Game.UI.CloseEscapeMenu());

        // Add item
        List<TMP_Dropdown.OptionData> itemOptions = new List<TMP_Dropdown.OptionData>();
        foreach (ItemDef itemDef in DefDatabase<ItemDef>.AllDefs)
        {
            itemOptions.Add(new TMP_Dropdown.OptionData(itemDef.DefName));
        }
        AddItemDropdown.options = itemOptions;
        AddItemButton.onClick.AddListener(AddItem);

        // Force event
        List<TMP_Dropdown.OptionData> eventOptions = new List<TMP_Dropdown.OptionData>();
        eventOptions.Add(new TMP_Dropdown.OptionData("No Force"));
        foreach (EncounterDef encounterDef in DefDatabase<EncounterDef>.AllDefs)
        {
            eventOptions.Add(new TMP_Dropdown.OptionData(encounterDef.DefName));
        }
        ForceEventDropdown.options = eventOptions;
        ForceEventDropdown.onValueChanged.AddListener(ForceEncounter);

        // Apply health condition
        List<TMP_Dropdown.OptionData> healthConditionOptions = new List<TMP_Dropdown.OptionData>();
        foreach (HealthConditionDef healthConditionDef in ApplicableHealthConditions)
        {
            healthConditionOptions.Add(new TMP_Dropdown.OptionData(healthConditionDef.DefName));
        }
        HealthConditionDropwdown.options = healthConditionOptions;
        ApplyHealthConditionButton.onClick.AddListener(ApplyHealthCondition);

        gameObject.SetActive(false);

        // Health condition buttons
        ApplyArmFractureButton.onClick.AddListener(() => Game.ApplyArmFracture(1f, "Devmode Menu"));
        ApplyLegFractureButton.onClick.AddListener(() => Game.ApplyLegFracture(1f, "Devmode Menu"));
        ApplyCutWoundButton.onClick.AddListener(() => Game.ApplyCutWound("Devmode Menu"));
        ApplyBruiseWoundButton.onClick.AddListener(() => Game.ApplyBruiseWound("Devmode Menu"));
    }

    private void AddItem()
    {
        Game.AddNewItemToInventory(DefDatabase<ItemDef>.AllDefs[AddItemDropdown.value]);
    }

    private void ForceEncounter(int value)
    {
        Game.EncounterManager.ForceEncounter(value == 0 ? null : DefDatabase<EncounterDef>.AllDefs[value - 1]);
    }

    private List<HealthConditionDef> ApplicableHealthConditions => DefDatabase<HealthConditionDef>.AllDefs.Where(hc => !hc.IsVital && !hc.IsWound && !hc.IsFracture).ToList();
    private void ApplyHealthCondition()
    {
        Game.ApplyHealthCondition(ApplicableHealthConditions[HealthConditionDropwdown.value], "Devmode Menu");
    }
}
