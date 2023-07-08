using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Tooltip : MonoBehaviour
{
    private Vector3 MOUSE_OFFSET = new Vector3(0.2f, -0.2f, 0f);

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;

    private Item CurrentItem;
    private bool FollowCursor;

    private void Update()
    {
        if (CurrentItem != null) UpdatePosition(CurrentItem);
        else if (FollowCursor) UpdatePositionAtCursor();
    }

    public void Show(string title, string description = "")
    {
        gameObject.SetActive(true);

        FollowCursor = true;
        UpdatePositionAtCursor();
        TitleText.text = title;
        DescriptionText.text = description;
        DescriptionText.gameObject.SetActive(description != "");
    }

    public void Show(Item item)
    {
        gameObject.SetActive(true);

        CurrentItem = item;
        UpdatePosition(CurrentItem);
        TitleText.text = item.Name;
        DescriptionText.text = item.Description;
        DescriptionText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        FollowCursor = false;
        CurrentItem = null;
    }

    public void UpdatePositionAtCursor()
    {
        transform.position = Input.mousePosition + MOUSE_OFFSET;
    }

    public void UpdatePosition(Item item)
    {
        transform.position = item.transform.position + new Vector3(0.1f, -0.1f, 0f);
    }
}
