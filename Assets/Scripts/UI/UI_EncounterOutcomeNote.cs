using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EncounterOutcomeNote : MonoBehaviour
{
    public UI_TooltipTarget TooltipTarget;
    public Image ItemIcon;
    public TextMeshProUGUI PlusText;
    public TextMeshProUGUI MinusText;
    public TextMeshProUGUI AmountText;
    public TextMeshProUGUI StatChangeValueText;
    public TextMeshProUGUI StatChangeLabelText;


    public void Init(Sprite sprite, bool isAdded, int amount = 1, string tooltipTitle = "", string tooltipText = "")
    {
        ItemIcon.gameObject.SetActive(true);
        PlusText.gameObject.SetActive(true);
        MinusText.gameObject.SetActive(true);
        AmountText.gameObject.SetActive(true);

        ItemIcon.sprite = sprite;
        PlusText.gameObject.SetActive(isAdded);
        MinusText.gameObject.SetActive(!isAdded);

        AmountText.text = "x" + amount.ToString();
        AmountText.gameObject.SetActive(amount > 1);
        AmountText.color = isAdded ? PlusText.color : MinusText.color;

        StatChangeLabelText.gameObject.SetActive(false);
        StatChangeValueText.gameObject.SetActive(false);

        TooltipTarget.Init(tooltipTitle, tooltipText);
    }

    public void Init(StatDef stat, int value)
    {
        ItemIcon.gameObject.SetActive(false);
        PlusText.gameObject.SetActive(false);
        MinusText.gameObject.SetActive(false);
        AmountText.gameObject.SetActive(false);

        StatChangeValueText.text = value.ToString("+#;-#;0");
        StatChangeLabelText.text = stat.Abbreviation;

        TooltipTarget.Init(text: $"Base value of {stat.LabelCapWord} has {(value > 0 ? "increased" : "decreased")} by {Mathf.Abs(value)}.");
    }
}
