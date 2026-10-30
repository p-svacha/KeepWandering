using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DevmodeMenu : MonoBehaviourSingleton<UI_DevmodeMenu>
{
    private Game Game;

    [Header("Elements")]
    public Button CloseButton;

    public TMP_Dropdown AddItemDropdown;
    public Button AddItemButton;

    public TMP_Dropdown ForcedAfternoonEncounterDropdown;
    public TMP_Dropdown ForcedNightEncounterDropdown;

    public TMP_Dropdown HealthConditionDropwdown;
    public Button ApplyHealthConditionButton;
    public Button ApplyArmFractureButton;
    public Button ApplyLegFractureButton;
    public Button ApplyCutWoundButton;
    public Button ApplyBruiseWoundButton;
    public Button ApplyBurnWoundButton;

    // Forced encounters
    public EncounterDef ForcedAfternoonEncounter => ForcedAfternoonEncounterDropdown.value == 0 ? null : EncounterManager.GetAllAfternoonEncounters()[ForcedAfternoonEncounterDropdown.value - 1] as EncounterDef;
    public EncounterDef ForcedNightEncounter => ForcedNightEncounterDropdown.value == 0 ? null : EncounterManager.GetAllNightEncounters()[ForcedNightEncounterDropdown.value - 1] as EncounterDef;

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

        // Forced afternoon encounter
        List<TMP_Dropdown.OptionData> forcedAfternoonEncounterOptions = new List<TMP_Dropdown.OptionData>();
        forcedAfternoonEncounterOptions.Add(new TMP_Dropdown.OptionData("No Force"));
        foreach (EncounterDef encounterDef in EncounterManager.GetAllAfternoonEncounters())
        {
            forcedAfternoonEncounterOptions.Add(new TMP_Dropdown.OptionData(encounterDef.DefName));
        }
        ForcedAfternoonEncounterDropdown.options = forcedAfternoonEncounterOptions;

        // Forced night encounter
        List<TMP_Dropdown.OptionData> forcedNightEncounterOptions = new List<TMP_Dropdown.OptionData>();
        forcedNightEncounterOptions.Add(new TMP_Dropdown.OptionData("No Force"));
        foreach (EncounterDef encounterDef in EncounterManager.GetAllNightEncounters())
        {
            forcedNightEncounterOptions.Add(new TMP_Dropdown.OptionData(encounterDef.DefName));
        }
        ForcedNightEncounterDropdown.options = forcedNightEncounterOptions;

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
        ApplyArmFractureButton.onClick.AddListener(ApplyArmFracture);
        ApplyLegFractureButton.onClick.AddListener(ApplyLegFracture);
        ApplyCutWoundButton.onClick.AddListener(ApplyCutWound);
        ApplyBruiseWoundButton.onClick.AddListener(ApplyBruiseWound);
        ApplyBurnWoundButton.onClick.AddListener(ApplyBurnWound);

        // Button click sounds
        foreach(Button button in GetComponentsInChildren<Button>()) button.onClick.AddListener(AudioManager.PlayStandardButtonClick);
    }

    private List<HealthConditionDef> ApplicableHealthConditions => DefDatabase<HealthConditionDef>.AllDefs.Where(hc => !hc.IsVital && !hc.IsWound && !hc.IsFracture).ToList();
    private void RefreshStep(string text)
    {
        Game.DisplayEncounterStep(new EncounterStep(text, Game.CurrentEncounterStep.Options, Game.CurrentEncounterStep.IsFinalStep));
    }

    private void AddItem()
    {
        Game.AddNewItemToInventory(DefDatabase<ItemDef>.AllDefs[AddItemDropdown.value]);
        RefreshStep($"Added {DefDatabase<ItemDef>.AllDefs[AddItemDropdown.value].DefName} to inventory");
    }

    private void ApplyHealthCondition()
    {
        Game.ApplyHealthCondition(ApplicableHealthConditions[HealthConditionDropwdown.value], "Devmode Menu");
        RefreshStep($"Applied {ApplicableHealthConditions[HealthConditionDropwdown.value].DefName}");
    }
    private void ApplyArmFracture()
    {
        Game.ApplyArmFracture(1f, "Devmode Menu");
        RefreshStep("Applied Arm Fracture");
    }
    private void ApplyLegFracture()
    {
        Game.ApplyLegFracture(1f, "Devmode Menu");
        RefreshStep("Applied Leg Fracture");
    }
    
    private void ApplyCutWound()
    {
        Game.ApplyCutWound("Devmode Menu");
        RefreshStep("Applied Cut Wound");
    }
    private void ApplyBruiseWound()
    {
        Game.ApplyBruiseWound("Devmode Menu");
        RefreshStep("Applied Bruise Wound");
    }
    private void ApplyBurnWound()
    {
        Game.ApplyBurnWound("Devmode Menu");
        RefreshStep("Applied Burn Wound");
    }


    public void ResetForcedAfternoonEncounter()
    {
        ForcedAfternoonEncounterDropdown.value = 0;
    }

    public void ResetForcedNightEncounter()
    {
        ForcedNightEncounterDropdown.value = 0;
    }
}
