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

    public void Init(UI_EncounterDisplay encounterDisplay, EncounterStepOption option)
    {
        EncounterDisplay = encounterDisplay;
        Option = option;

        EventOptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(Game, option));
        SkillCheckIndicator.SetActive(option is SkillCheckOption);
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
