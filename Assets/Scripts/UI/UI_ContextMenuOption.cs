using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_ContextMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Game Game;
    private InteractionOption Option;

    [Header("Elements")]
    public TextMeshProUGUI OptionText;
    public Button OptionButton;
    

    public void Init(Game game, InteractionOption option)
    {
        Game = game;
        Option = option;
        OptionText.text = option.Text;
        OptionButton.onClick.AddListener(() => ChoseOption(game, option));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Option.OnHoverStartAction != null) Option.OnHoverStartAction();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Option.OnHoverEndAction != null) Option.OnHoverEndAction();
    }

    private void ChoseOption(Game game, InteractionOption option)
    {
        if (game.State == GameState.InGame)
        {
            UI_ContextMenu.Instance.Hide();
            option.Action();
        }
    }
}
