using TMPro;
using UnityEngine;

public abstract class UI_TooltipBase : MonoBehaviour
{
    private Vector3 MOUSE_OFFSET = new Vector3(0.2f, -0.2f, 0f);
    private const int SCREEN_EDGE_OFFSET = 10; // px

    protected virtual void Update()
    {
        UpdatePositionAtCursor();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    protected void UpdatePositionAtCursor()
    {
        Vector3 worldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        transform.position = worldPos + MOUSE_OFFSET;
        ClampToScreen();
    }

    protected void UpdatePositionAtUi(GameObject uiObject)
    {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        Vector3[] rectCorners = new Vector3[4];
        rect.GetWorldCorners(rectCorners);

        Vector2 bottomLeftCorner = rectCorners[0];
        transform.position = (Vector3)bottomLeftCorner;
        ClampToScreen();
    }

    protected void ClampToScreen()
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
