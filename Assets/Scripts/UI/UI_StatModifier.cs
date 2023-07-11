using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_StatModifier : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    public void Init(StatModifier mod)
    {
        LabelText.text = mod.Name;
        ValueText.text = (mod.Value > 0 ? "+" : "") + mod.Value.ToString();
        ValueText.color = mod.GetValueColor();
    }
}
