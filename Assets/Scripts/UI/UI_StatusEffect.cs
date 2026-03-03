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

    public void OnPointerEnter(PointerEventData eventData)
    {
        HealthReport.ShowDescriptionBox(HealthCondition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HealthReport.HideDescriptionBox();
    }
}
