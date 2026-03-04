using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_EncounterStepOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI_EncounterDisplay EncounterDisplay;
    private Game Game => EncounterDisplay.Game;
    private EncounterStepOption Option;

    [Header("Elements")]
    public TextMeshProUGUI EventOptionText;
    public Button OptionButton;
    public GameObject SkillCheckIndicator;
    public GameObject ItemSlotContainer;

    [Header("Prefabs")]
    public UI_ItemSlot ItemSlotPrefab;

    public void Init(UI_EncounterDisplay encounterDisplay, EncounterStepOption option)
    {
        EncounterDisplay = encounterDisplay;
        Option = option;

        EventOptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(Game, option));
        SkillCheckIndicator.SetActive(option is SkillCheckOption);

        // Item slots
        HelperFunctions.DestroyAllChildredImmediately(ItemSlotContainer);
        foreach(ItemSlot itemSlot in option.ItemSlots)
        {
            UI_ItemSlot uiItemSlot = Instantiate(ItemSlotPrefab, ItemSlotContainer.transform);
            uiItemSlot.Init(EncounterDisplay, itemSlot);
        }
    }

    private void ChoseOption(Game game, EncounterStepOption option)
    {
        game.UI.StatPanel.UnhighlightAll();

        if (game.State == GameState.InGame)
        {
            EncounterStep nextEventStep = option.Execute();
            if(nextEventStep != null) game.DisplayEncounterStep(nextEventStep);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EncounterDisplay.OnOptionHovered(Option);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EncounterDisplay.OnOptionUnhovered();
        
    }
}
