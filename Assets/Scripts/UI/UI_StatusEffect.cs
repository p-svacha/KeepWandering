using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StatusEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image BackgroundImage;
    public Text StatusEffectText;
    public GameObject DescriptionBox;
    public Text DescriptionText;

    private const float TimeToShowDescription = 1f;
    private bool IsHovering;
    private float CurrentHoverTime;

    public void Init(HealthCondition healthCondition)
    {
        healthCondition.UiDisplayElement = this;

        StatusEffectText.text = healthCondition.GetReportLabel();
        StatusEffectText.color = healthCondition.GetReportTextColor();
        DescriptionText.text = healthCondition.GetReportDescription();
        BackgroundImage.color = healthCondition.GetReportBackgroundColor();
        LayoutRebuilder.ForceRebuildLayoutImmediate(BackgroundImage.GetComponent<RectTransform>());
    }

    private void Update()
    {
        if(IsHovering)
        {
            CurrentHoverTime += Time.deltaTime;
            if (CurrentHoverTime >= TimeToShowDescription)
            {
                DescriptionBox.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(DescriptionBox.GetComponent<RectTransform>());
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovering = true;
        CurrentHoverTime = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DescriptionBox.SetActive(false);
        IsHovering = false;
    }
}
