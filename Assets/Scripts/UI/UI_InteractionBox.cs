using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractionBox : MonoBehaviour
{
    public Game Game;

    public UI_InteractionBoxOption InteractionOptionPrefab;
    public List<UI_InteractionBoxOption> Options = new List<UI_InteractionBoxOption>();
    public GameObject ContentContainer;
    public GameObject OptionsContainer;
    public Text TitleText;

    public void Init(Item item)
    {
        Clear();
        UpdatePosition(item);

        TitleText.text = item.Name;
        foreach(ItemInteractionOption option in item.GetInteractionOptions())
        {
            UI_InteractionBoxOption optionButton = Instantiate(InteractionOptionPrefab, OptionsContainer.transform);
            Options.Add(optionButton);
            optionButton.Init(Game, option);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentContainer.GetComponent<RectTransform>());
    }

    public void Clear()
    {
        foreach (UI_InteractionBoxOption option in Options) Destroy(option.gameObject);
        Options.Clear();
    }

    public void UpdatePosition(Item item)
    {
        transform.position = Camera.main.WorldToScreenPoint(item.transform.position) + new Vector3(0.5f, 0.5f, 0f);
    }
}
