using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_EventItemChange : MonoBehaviour
{
    public Image ItemIcon;
    public TextMeshProUGUI PlusText;
    public TextMeshProUGUI MinusText;
    public TextMeshProUGUI AmountText;

    public void Init(Item item, bool isAdded, int amount = 1)
    {
        ItemIcon.sprite = item.Sprite;
        PlusText.gameObject.SetActive(isAdded);
        MinusText.gameObject.SetActive(!isAdded);

        AmountText.text = amount.ToString();
        AmountText.gameObject.SetActive(amount > 1);
        AmountText.color = isAdded ? PlusText.color : MinusText.color;
    }
}
