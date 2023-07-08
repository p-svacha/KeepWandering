using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_ContextMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Game Game;
    private InteractionOption InteractionOption;

    [Header("Elements")]
    public TextMeshProUGUI OptionText;
    public Button OptionButton;
    

    public void Init(Game game, InteractionOption option)
    {
        Game = game;
        InteractionOption = option;
        OptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, option));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InteractionOption.OnHoverStartAction != null) InteractionOption.OnHoverStartAction();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InteractionOption.OnHoverEndAction != null) InteractionOption.OnHoverEndAction();
    }

    private void ChoseOption(Game game, InteractionOption option)
    {
        if (game.State == GameState.InGame)
        {
            game.UI.ContextMenu.Hide();
            option.Action();
        }
    }
}
