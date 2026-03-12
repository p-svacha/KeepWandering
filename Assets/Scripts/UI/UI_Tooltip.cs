using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Tooltip : MonoBehaviour
{
    public static UI_Tooltip Instance;

    private void Awake()
    {
        Instance = this;
    }

    private Vector3 MOUSE_OFFSET = new Vector3(0.2f, -0.2f, 0f);
    private const int SCREEN_EDGE_OFFSET = 10; // px

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

    public void Show(string title = "", string description = "")
    {
        Reset();
        gameObject.SetActive(true);
        TitleText.gameObject.SetActive(title != "");
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
        TitleText.text = item.LabelCapWord;
        DescriptionText.text = item.Description;
    }

    public void Show(UI_Stat statDisplay)
    {
        Debug.Log($"Showing tooltip for stat {statDisplay.Stat.Label}");

        Reset();
        gameObject.SetActive(true);
        TitleText.gameObject.SetActive(false);
        DescriptionText.gameObject.SetActive(false);

        UpdatePositionAtUi(statDisplay.gameObject);

        // Base value
        UI_StatModifier baseValueRow = Instantiate(StatModifierPrefab, Container.transform);
        baseValueRow.InitBaseValue(statDisplay.Stat);

        // Modifiers
        foreach (StatModifier mod in statDisplay.Stat.GetModifiers())
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
        Vector3 worldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        transform.position = worldPos + MOUSE_OFFSET;
        ClampToScreen();
    }

    public void UpdatePosition(Item item)
    {
        transform.position = item.Renderer.transform.position + new Vector3(0.1f, -0.1f, 0f);
        ClampToScreen();
    }

    public void UpdatePositionAtUi(GameObject uiObject)
    {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        Vector3[] rectCorners = new Vector3[4];
        rect.GetWorldCorners(rectCorners);

        Vector2 bottomLeftCorner = rectCorners[0];
        transform.position = (Vector3)bottomLeftCorner;
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Camera cam = Game.Instance.MainCamera;
        RectTransform tooltipRect = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);

        Vector2 screenMin = cam.WorldToScreenPoint(corners[0]);
        Vector2 screenMax = cam.WorldToScreenPoint(corners[2]);

        float dx = 0f;
        float dy = 0f;

        if (screenMax.x > Screen.width - SCREEN_EDGE_OFFSET)
            dx = (Screen.width - SCREEN_EDGE_OFFSET) - screenMax.x;
        if (screenMin.x + dx < SCREEN_EDGE_OFFSET)
            dx = SCREEN_EDGE_OFFSET - screenMin.x;

        if (screenMax.y > Screen.height - SCREEN_EDGE_OFFSET)
            dy = (Screen.height - SCREEN_EDGE_OFFSET) - screenMax.y;
        if (screenMin.y + dy < SCREEN_EDGE_OFFSET)
            dy = SCREEN_EDGE_OFFSET - screenMin.y;

        if (dx != 0f || dy != 0f)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            screenPos.x += dx;
            screenPos.y += dy;
            Vector3 newPos = cam.ScreenToWorldPoint(screenPos);
            newPos.z = 0f;
            transform.position = newPos;
        }
    }
}
