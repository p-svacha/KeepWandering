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
        Refresh();
    }

    public void Refresh()
    {
        // Label
        LabelText.text = Stat.Def.LabelCapWord;

        // Value
        ValueText.text = Stat.GetValue().ToString();
        ValueText.color = Stat.GetValueColor();
    }

    private void Update()
    {
        if (IsFocussed && !Game.Instance.UI.Tooltip.gameObject.activeSelf)
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
        Game.Instance.UI.Tooltip.Show(this);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        Game.Instance.UI.Tooltip.Hide();
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
