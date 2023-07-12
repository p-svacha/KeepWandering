using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_EventOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EventDialogueOption Option;

    [Header("Elements")]
    public TextMeshProUGUI EventOptionText;
    public Button OptionButton;

    public void Init(Game game, EventDialogueOption eventOption)
    {
        Option = eventOption;
        EventOptionText.text = eventOption.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, eventOption));
    }

    private void ChoseOption(Game game, EventDialogueOption option)
    {
        if (game.State == GameState.InGame)
        {
            if (option.Action != null)
            {
                EventStep nextEventStep = option.Action();
                if(nextEventStep != null) game.DisplayEventStep(nextEventStep);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (StatId stat in Option.AffectingStats) Game.Singleton.UI.HightlightStat(stat);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (StatId stat in Option.AffectingStats) Game.Singleton.UI.UnhighlightStat(stat);
    }
}
