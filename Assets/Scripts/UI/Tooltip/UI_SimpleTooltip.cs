using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SimpleTooltip : UI_TooltipBase
{
    public static UI_SimpleTooltip Instance;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;

    public void Show(string title = "", string description = "")
    {
        gameObject.SetActive(true);

        TitleText.gameObject.SetActive(title != "");
        DescriptionText.gameObject.SetActive(description != "");

        UpdatePositionAtCursor();
        TitleText.text = title;
        DescriptionText.text = description;
    }
}
