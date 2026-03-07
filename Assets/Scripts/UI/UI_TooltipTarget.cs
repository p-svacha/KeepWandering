using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UI_TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Title;
    public string Text;

    public bool Disabled;

    [HideInInspector] public bool IsFocussed;
    private float Delay = 0.5f;
    [HideInInspector] public float CurrentDelay;

    public void Init(string title = "", string text = "")
    {
        Title = title;
        Text = text;

        if(Title == "" && Text == "") Disabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsFocussed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void Update()
    {
        if (Disabled) return;
        if(IsFocussed)
        {
            if(CurrentDelay < Delay) CurrentDelay += UnityEngine.Time.deltaTime;
            else ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        if (UI_Tooltip.Instance.gameObject.activeSelf) return;

        UI_Tooltip.Instance.Show(Title, Text);
    }

    public void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        UI_Tooltip.Instance.Hide();
    }
}

