using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EventItemChange : MonoBehaviour
{
    public Image ItemIcon;
    public Text PlusText;
    public Text MinusText;

    public void Init(Item item, bool isAdded)
    {
        ItemIcon.sprite = item.GetComponent<SpriteRenderer>().sprite;
        PlusText.gameObject.SetActive(isAdded);
        MinusText.gameObject.SetActive(!isAdded);
    }
}
