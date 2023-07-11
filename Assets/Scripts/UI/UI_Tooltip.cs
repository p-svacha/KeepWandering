using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Tooltip : MonoBehaviour
{
    private Vector3 MOUSE_OFFSET = new Vector3(0.2f, -0.2f, 0f);
    private const float SCREEN_EDGE_OFFSET = 0f;

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public GameObject Container;

    [Header("Prefabs")]
    public UI_StatModifier StatModifierPrefab;

    private Item CurrentItem;
    private bool FollowCursor;

    private void Update()
    {
        if (CurrentItem != null) UpdatePosition(CurrentItem);
        else if (FollowCursor) UpdatePositionAtCursor();
    }

    public void Show(string title, string description = "")
    {
        Reset();
        gameObject.SetActive(true);
        TitleText.gameObject.SetActive(true);
        DescriptionText.gameObject.SetActive(description != "");

        FollowCursor = true;
        UpdatePositionAtCursor();
        TitleText.text = title;
        DescriptionText.text = description;
    }

    public void Show(Item item)
    {
        Reset();
        gameObject.SetActive(true);
        TitleText.gameObject.SetActive(true);
        DescriptionText.gameObject.SetActive(true);

        CurrentItem = item;
        UpdatePosition(CurrentItem);
        TitleText.text = item.Name;
        DescriptionText.text = item.Description;
    }

    public void Show(UI_Stat statDisplay)
    {
        if (statDisplay.Stat.GetModifiers().Count == 0) return;

        Reset();
        gameObject.SetActive(true);
        TitleText.gameObject.SetActive(false);
        DescriptionText.gameObject.SetActive(false);

        UpdatePositionAtUi(statDisplay.gameObject);

        foreach(StatModifier mod in statDisplay.Stat.GetModifiers())
        {
            UI_StatModifier modDisplay = Instantiate(StatModifierPrefab, Container.transform);
            modDisplay.Init(mod);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Reset()
    {
        HelperFunctions.DestroyAllChildredImmediately(Container);
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

    public void UpdatePositionAtUi(GameObject uiObject)
    {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        Vector3[] rectCorners = new Vector3[4];
        rect.GetWorldCorners(rectCorners);

        Vector2 bottomLeftCorner = rectCorners[0];
        Vector2 tooltipPosition = bottomLeftCorner;

        // Make sure full tooltip is on screen
        Vector2 positionScreen = Game.Singleton.MainCamera.WorldToScreenPoint(tooltipPosition);
        Vector2 tooltipEdge = Game.Singleton.MainCamera.ScreenToWorldPoint(new Vector2(positionScreen.x + GetComponent<RectTransform>().rect.width, positionScreen.y + GetComponent<RectTransform>().rect.height));
        Vector2 tooltipDimensions = tooltipEdge - tooltipPosition;
        Vector2 screenDimensions = Game.Singleton.MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        if (tooltipEdge.x > screenDimensions.x) tooltipPosition.x = screenDimensions.x - tooltipDimensions.x - SCREEN_EDGE_OFFSET;
        //if (position.y - tooltipDimensions.y < 0) position.y = tooltipDimensions.y + SCREEN_EDGE_OFFSET;

        transform.position = tooltipPosition;
    }
}
