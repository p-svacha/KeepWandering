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
    public GameObject ItemSlotContainer;

    [Header("Prefabs")]
    public UI_ItemSlot ItemSlotPrefab;

    public List<UI_ItemSlot> ItemSlotDisplays;

    public void Init(EncounterOption option)
    {
        Option = option;

        EventOptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(Game, option));
        SkillCheckIndicator.SetActive(option is SkillCheckOption);

        // Item slots
        ItemSlotDisplays = new List<UI_ItemSlot>();
        HelperFunctions.DestroyAllChildredImmediately(ItemSlotContainer);
        foreach (ItemSlot itemSlot in option.ItemSlots)
        {
            UI_ItemSlot itemSlotDisplay = Instantiate(ItemSlotPrefab, ItemSlotContainer.transform);
            itemSlotDisplay.Init(itemSlot);
            ItemSlotDisplays.Add(itemSlotDisplay);
        }

        Resfresh();
    }

    public void Resfresh()
    {
        // Slots
        foreach (UI_ItemSlot itemSlot in ItemSlotDisplays) itemSlot.Refresh();

        // Interactibility
        bool canSelect = Option.CanSelect();
        OptionButton.interactable = canSelect;
        OptionButton.GetComponent<Image>().color = canSelect ? ResourceManager.Color_Button_Default : ResourceManager.Color_Button_Disabled;
        SkillCheckIndicator.GetComponent<Image>().color = canSelect ? ResourceManager.Color_Panel_Highlighted : ResourceManager.Color_Button_Disabled;
    }

    private void ChoseOption(Game game, EncounterOption option)
    {
        if (game.State == GameState.InGame)
        {
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
}
