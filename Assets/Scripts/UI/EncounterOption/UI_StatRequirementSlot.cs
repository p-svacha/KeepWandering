using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatRequirementSlot : MonoBehaviour
{
    private KeyValuePair<StatDef, int> StatRequirement;

    [Header("Elements")]
    public Image Background;
    public TextMeshProUGUI LabelText;

    public void Init(KeyValuePair<StatDef, int> statRequirement)
    {
        StatRequirement = statRequirement;

        LabelText.text = $"{statRequirement.Key.Abbreviation}\n{statRequirement.Value}";
        Refresh();
    }

    public void Refresh()
    {
        bool isMet = Game.Instance.Player.GetStatValue(StatRequirement.Key) >= StatRequirement.Value;
        Background.color = isMet ? ResourceManager.Color_Option_Slot_Filled : ResourceManager.Color_Option_Slot_Unmet;
    }
}
