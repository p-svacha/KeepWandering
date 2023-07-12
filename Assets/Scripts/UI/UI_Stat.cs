using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Stat : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Stat Stat { get; private set; }

    [Header("Elements")]
    public Image HighlightImage;
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    private bool IsFocussed;
    private float CurrentDelay;

    public void Init(Stat stat)
    {
        Stat = stat;
        UpdateStat();
    }

    public void UpdateStat()
    {
        // Label
        LabelText.text = Stat.Name;

        // Value
        ValueText.text = Stat.GetValue() + "%";
        ValueText.color = Stat.GetValueColor();

        // Tooltip
        string text = "";
        foreach (StatModifier mod in Stat.GetModifiers())
        {
            text += "\n" + mod.Name;
        }
    }

    private void Update()
    {
        if (IsFocussed && !Game.Singleton.UI.Tooltip.gameObject.activeSelf)
        {
            if (CurrentDelay < GameUI.TOOLTIP_HOVER_TIME) CurrentDelay += Time.deltaTime;
            else ShowTooltip();
        }
    }

    public void Highlight()
    {
        HighlightImage.color = Color.white;
    }
    public void Unhighlight()
    {
        HighlightImage.color = Color.clear;
    }

    private void ShowTooltip()
    {
        Game.Singleton.UI.Tooltip.Show(this);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        Game.Singleton.UI.Tooltip.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsFocussed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
}
