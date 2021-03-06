using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DescriptionBox : MonoBehaviour
{
    public Text TitleText;
    public Text DescriptionText;

    public void Init(Item item)
    {
        gameObject.SetActive(true);
        UpdatePosition(item);
        TitleText.text = item.Name;
        DescriptionText.text = item.Description;
    }

    public void UpdatePosition(Item item)
    {
        transform.position = Camera.main.WorldToScreenPoint(item.transform.position) + new Vector3(0.5f, 0.5f, 0f);
    }
}
