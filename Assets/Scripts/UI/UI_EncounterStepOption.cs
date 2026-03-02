using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_EncounterStepOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EncounterStepOption Option;

    [Header("Elements")]
    public TextMeshProUGUI EventOptionText;
    public Button OptionButton;

    public void Init(Game game, EncounterStepOption option)
    {
        Option = option;
        EventOptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, option));
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
        if (Option is SkillCheckOption skillCheckOption)
        {
            foreach(StatDef stat in skillCheckOption.AssociatedStats.Keys) GameUI.Instance.HightlightStat(stat);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Option is SkillCheckOption skillCheckOption)
        {
            foreach (StatDef stat in skillCheckOption.AssociatedStats.Keys) GameUI.Instance.UnhighlightStat(stat);
        }
    }
}
