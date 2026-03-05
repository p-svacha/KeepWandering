using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_StatModifier : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    public void InitBaseValue(Stat stat)
    {
        LabelText.text = "Base Value";
        ValueText.text = stat.BaseValue.ToString();
        SetBold(true);
    }

    public void Init(StatModifier mod)
    {
        LabelText.text = mod.Name;
        ValueText.text = (mod.Value > 0 ? "+" : "") + mod.Value.ToString();
        ValueText.color = mod.GetValueColor();
    }

    public void SetBold(bool bold)
    {
        LabelText.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        ValueText.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
    }
}
