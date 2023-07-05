using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InteractionBoxOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Game Game;
    public Text OptionText;
    public Button OptionButton;
    public ItemInteractionOption ItemInteractionOption;

    public void Init(Game game, ItemInteractionOption option)
    {
        Game = game;
        ItemInteractionOption = option;
        OptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, option));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemInteractionOption.OnHoverStartAction != null) ItemInteractionOption.OnHoverStartAction();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemInteractionOption.OnHoverEndAction != null) ItemInteractionOption.OnHoverEndAction();
    }

    private void ChoseOption(Game game, ItemInteractionOption option)
    {
        if (game.State == GameState.InGame)
        {
            game.UI.HideInteractionBox();
            option.Action();
        }
    }
}
