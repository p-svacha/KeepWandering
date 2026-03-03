using TMPro;
using UnityEngine;

public class UI_LabelValueRow : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public TextMeshProUGUI ValueText;

    public void Init(string label, string value)
    {
        LabelText.text = label;
        ValueText.text = value;
    }

    public void SetBold(bool bold)
    {
        LabelText.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        ValueText.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
    }
}
