using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemTooltipTag : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public List<GameObject> Stars;

    public void Init(ItemDef item, ItemTagDef tag)
    {
        LabelText.text = tag.Label;
        int tagLevel = item.Tags[tag];
        for (int i = 0; i < Stars.Count; i++) Stars[i].SetActive(i < tagLevel);
    }
}
