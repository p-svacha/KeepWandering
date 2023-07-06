using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_EventOption : MonoBehaviour
{
    public TextMeshProUGUI EventOptionText;
    public Button OptionButton;

    public void Init(Game game, EventDialogueOption eventOption)
    {
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
}
