using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ContextMenu : MonoBehaviour
{
    private Game Game;
    private List<UI_ContextMenuOption> Options = new List<UI_ContextMenuOption>();
    private Item CurrentItem;

    private Vector3 MOUSE_OFFSET = new Vector3(0f, 0f, 0f);

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public GameObject OptionsContainer;

    [Header("Prefabs")]
    public UI_ContextMenuOption InteractionOptionPrefab;

    public void Init(Game game)
    {
        Game = game;
    }

    private void Update()
    {
        if (CurrentItem != null) UpdatePosition(CurrentItem);
    }

    public void Show(Item item)
    {
        CurrentItem = item;
        Show(item.Name, item.GetInteractionOptions());
        UpdatePosition(item);
    }

    public void Show(string title, List<InteractionOption> options)
    {
        gameObject.SetActive(true);
        Clear();
        UpdatePositionAtCursor();

        TitleText.text = title;
        foreach (InteractionOption option in options)
        {
            UI_ContextMenuOption optionButton = Instantiate(InteractionOptionPrefab, OptionsContainer.transform);
            Options.Add(optionButton);
            optionButton.Init(Game, option);
        }

        // LayoutRebuilder.ForceRebuildLayoutImmediate(OptionsContainer.GetComponent<RectTransform>());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        CurrentItem = null;
    }

    private void Clear()
    {
        HelperFunctions.DestroyAllChildredImmediately(OptionsContainer);
        Options.Clear();
    }

    public void UpdatePosition(Item item)
    {
        transform.position = item.transform.position + new Vector3(0.1f, -0.1f, 0f);
    }

    public void UpdatePositionAtCursor()
    {
        transform.position = Game.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }
}
