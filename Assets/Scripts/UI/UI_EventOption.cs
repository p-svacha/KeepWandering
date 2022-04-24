using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventOption : MonoBehaviour
{
    public Text EventOptionText;
    public Button OptionButton;

    public void Init(Game game, EventOption eventOption)
    {
        EventOptionText.text = "> " + eventOption.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, eventOption));
    }

    private void ChoseOption(Game game, EventOption option)
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
}
