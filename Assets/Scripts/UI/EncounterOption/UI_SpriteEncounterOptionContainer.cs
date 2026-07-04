using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Container used to display sprite-bound encounter options. Container is instantiated in Canvas-space to display options bound to a specific SpriteRenderer.
/// </summary>
public class UI_SpriteEncounterOptionContainer : MonoBehaviour
{
    // Tunable constants
    private const float CARD_OFFSET_X = 24f;
    private const float CARD_OFFSET_Y = 24f;

    public bool IsLocked { get; private set; }

    [Header("Prefabs")]
    public UI_EncounterStepOption OptionPrefab;

    // Tracked state
    private SpriteRenderer BoundSprite;
    public Dictionary<EncounterOption, UI_EncounterStepOption> OptionDisplays { get; private set; }

    public void Init(SpriteRenderer sprite, List<EncounterOption> options)
    {
        BoundSprite = sprite;
        OptionDisplays = new Dictionary<EncounterOption, UI_EncounterStepOption>();

        HelperFunctions.DestroyAllChildredImmediately(gameObject);

        foreach(EncounterOption option in options)
        {
            UI_EncounterStepOption optionUI = Instantiate(OptionPrefab, transform);
            optionUI.Init(option);
            OptionDisplays.Add(option, optionUI);
        }

        // Force layout rebuild and position before hiding (layout rebuilding is unreliable on inactive GameObjects)
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        RefreshPosition();

        // Hide initially
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        if (IsLocked && !gameObject.activeSelf) Show();
    }

    /// <summary>
    /// Recomputes and updates the container's canvas position based on the bound sprite's world position.
    /// Called on init and after camera zoom transitions.
    /// </summary>
    public void RefreshPosition()
    {
        if (BoundSprite == null) return;

        // Compute world-space anchor point (top-center of sprite bounds)
        Bounds bounds = BoundSprite.bounds;
        Vector3 worldPoint = new Vector3(bounds.center.x, bounds.max.y, 0);

        // Convert to screen space
        Vector3 screenPoint = Game.Instance.MainCamera.WorldToScreenPoint(worldPoint);

        // Get parent canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform containerRect = GetComponent<RectTransform>();

        // Convert screen point to local canvas coordinates
        Camera canvasCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvasCamera, out Vector2 localPoint);

        // Apply offset (top-right)
        localPoint += new Vector2(CARD_OFFSET_X, CARD_OFFSET_Y);

        // Clamp to keep container within canvas bounds
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 containerSize = containerRect.rect.size;

        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f - containerSize.x;
        float minY = -canvasSize.y * 0.5f;
        float maxY = canvasSize.y * 0.5f - containerSize.y;

        localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

        // Apply position
        containerRect.anchoredPosition = localPoint;
    }
}
