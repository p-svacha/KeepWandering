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
    public Image IsRequiredIndicator;

    public void Init(KeyValuePair<StatDef, int> statRequirement)
    {
        StatRequirement = statRequirement;

        LabelText.text = $"{statRequirement.Key.Abbreviation}\n{statRequirement.Value}";
        Refresh();
    }

    public void Refresh()
    {
        Background.color = ResourceManager.Color_Option_Slot;

        bool isMet = Game.Instance.Player.GetStatValue(StatRequirement.Key) >= StatRequirement.Value;
        IsRequiredIndicator.color = isMet ? ResourceManager.Color_Option_Slot_Req_Met : ResourceManager.Color_Option_Slot_Req_Unmet;
    }
}
