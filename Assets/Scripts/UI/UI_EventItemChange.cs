using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventItemChange : MonoBehaviour
{
    public Image ItemIcon;
    public Text PlusText;
    public Text MinusText;
    public Text AmountText;

    public void Init(Item item, bool isAdded, int amount = 1)
    {
        ItemIcon.sprite = item.GetComponent<SpriteRenderer>().sprite;
        PlusText.gameObject.SetActive(isAdded);
        MinusText.gameObject.SetActive(!isAdded);

        AmountText.text = amount.ToString();
        AmountText.gameObject.SetActive(amount > 1);
        AmountText.color = isAdded ? PlusText.color : MinusText.color;
    }
}
