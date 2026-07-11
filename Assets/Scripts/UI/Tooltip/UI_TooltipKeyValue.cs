using TMPro;
using UnityEngine;

public class UI_TooltipKeyValue : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    public void Init(string label, string value)
    {
        LabelText.text = label;
        ValueText.text = value;
    }
}
