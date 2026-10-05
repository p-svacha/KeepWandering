using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_EncounterStepOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Game Game => Game.Instance;
    public EncounterOption Option { get; private set; }

    [Header("Elements")]
    public TextMeshProUGUI EventOptionText;
    public Button OptionButton;
    public GameObject SkillCheckIndicator;
    public UI_TooltipTarget SkillCheckIndicator_TooltipTarget;
    public TextMeshProUGUI SkillCheckIndicator_DifficultyText;
    public GameObject ItemSlotContainer;
    

    [Header("Prefabs")]
    public UI_ItemSlot ItemSlotPrefab;
    public UI_StatRequirementSlot StatRequirementSlotPrefab;

    // Current display
    public List<UI_StatRequirementSlot> StatRequirementSlotDisplays;
    public List<UI_ItemSlot> ItemSlotDisplays;

    public void Init(EncounterOption option)
    {
        Option = option;

        EventOptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(Game, option));
        if (option is SkillCheckOption skillCheckOption)
        {
            SkillCheckIndicator.SetActive(true);
            SkillCheckIndicator_TooltipTarget.Init("Skill Check", $"This option is a skill check, meaning it requires a roll against a difficulty to determine the outcome.");
        }
        else SkillCheckIndicator.SetActive(false);

        HelperFunctions.DestroyAllChildredImmediately(ItemSlotContainer);

        // Stat requirement slots
        StatRequirementSlotDisplays = new List<UI_StatRequirementSlot>();
        foreach(KeyValuePair<StatDef, int> statRequirement in option.SkillRequirements)
        {
            UI_StatRequirementSlot statRequirementDisplay = Instantiate(StatRequirementSlotPrefab, ItemSlotContainer.transform);
            statRequirementDisplay.Init(statRequirement);
            StatRequirementSlotDisplays.Add(statRequirementDisplay);
        }

        // Item slots
        ItemSlotDisplays = new List<UI_ItemSlot>();
        foreach (ItemSlot itemSlot in option.ItemSlots)
        {
            UI_ItemSlot itemSlotDisplay = Instantiate(ItemSlotPrefab, ItemSlotContainer.transform);
            itemSlotDisplay.Init(itemSlot);
            ItemSlotDisplays.Add(itemSlotDisplay);
        }

        Refresh();
    }

    public void Refresh()
    {
        // Slots
        foreach (UI_StatRequirementSlot statRequirementSlot in StatRequirementSlotDisplays) statRequirementSlot.Refresh();
        foreach (UI_ItemSlot itemSlot in ItemSlotDisplays) itemSlot.Refresh();

        // Interactibility
        bool canSelect = Option.CanSelect();
        OptionButton.interactable = canSelect;
        OptionButton.GetComponent<Image>().color = canSelect ? ResourceManager.Color_Button_Default : ResourceManager.Color_Button_Disabled;
        SkillCheckIndicator.GetComponent<Image>().color = canSelect ? ResourceManager.Color_Panel_Highlighted : ResourceManager.Color_Button_Disabled;

        // Difficulty
        UpdateDifficulty();
    }

    public void UpdateDifficulty()
    {
        if (Option is SkillCheckOption skillCheckOption)
        {
            SkillCheckIndicator_DifficultyText.text = skillCheckOption.GetDifficultyValue().ToString();
        }
    }

    private void ChoseOption(Game game, EncounterOption option)
    {
        if (game.State == GameState.InGame)
        {
            // Sprite-bound option chosen without being locked (e.g. via the Alt reveal): lock it now,
            // so the card stays visible after Alt is released while the option resolves.
            SpriteOptionInteractionManager.EnsureLockedForOption(option);

            game.SelectEncounterOption(option);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemDragDropManager.HoveredOptionDisplay = this;
        UI_EncounterDisplay.Instance.OnOptionHovered(Option);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemDragDropManager.HoveredOptionDisplay == this)
            ItemDragDropManager.HoveredOptionDisplay = null;
        UI_EncounterDisplay.Instance.OnOptionUnhovered();
    }

    public void SetDragGreyedOut(bool greyedOut)
    {
        bool canSelect = Option.CanSelect();
        OptionButton.GetComponent<Image>().color = greyedOut ? ResourceManager.Color_Button_Disabled : (canSelect ? ResourceManager.Color_Button_Default : ResourceManager.Color_Button_Disabled);
        SkillCheckIndicator.GetComponent<Image>().color = greyedOut ? ResourceManager.Color_Button_Disabled : (canSelect ? ResourceManager.Color_Panel_Highlighted : ResourceManager.Color_Button_Disabled);
        OptionButton.interactable = !greyedOut && canSelect;
    }

    private void OnDisable()
    {
        // Deactivated while hovered (e.g. the Alt-revealed card hiding on release): OnPointerExit never fires,
        // so clear the hover state and everything it triggered by hand.
        if (ItemDragDropManager.HoveredOptionDisplay != this) return;

        ItemDragDropManager.HoveredOptionDisplay = null;

        if (UI_EncounterDisplay.Instance != null) UI_EncounterDisplay.Instance.OnOptionUnhovered();
        SkillCheckIndicator_TooltipTarget.HideTooltip();
    }
}
