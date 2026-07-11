using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StatusEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI_HealthReport HealthReport;
    private HealthCondition HealthCondition;

    public Image BackgroundImage;
    public TextMeshProUGUI StatusEffectText;

    // Tooltip
    private bool IsFocussed;
    private float CurrentDelay;

    public void Init(UI_HealthReport report, HealthCondition healthCondition)
    {
        HealthReport = report;
        HealthCondition = healthCondition;

        healthCondition.UiDisplayElement = this;

        StatusEffectText.text = healthCondition.GetReportLabel();
        StatusEffectText.color = healthCondition.GetReportTextColor();
        BackgroundImage.color = healthCondition.GetReportBackgroundColor();
        LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundImage.GetComponent<RectTransform>());
    }


    // Tooltip
    private void Update()
    {
        if (IsFocussed && !UI_HealthConditionTooltip.Instance.gameObject.activeSelf)
        {
            if (CurrentDelay < GameUI.TOOLTIP_HOVER_TIME) CurrentDelay += Time.deltaTime;
            else ShowTooltip();
        }
    }

    private void ShowTooltip()
    {
        UI_HealthConditionTooltip.Instance.Show(HealthCondition);
    }

    private void HideTooltip()
    {
        IsFocussed = false;
        CurrentDelay = 0;
        Game.Instance.UI.HideAllTooltips();
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
