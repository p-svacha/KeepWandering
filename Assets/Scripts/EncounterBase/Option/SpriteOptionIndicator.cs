using Clipper2Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Dynamically added component to any sprite that has bound encounter options.
/// Renders a dashed outline traced from the sprite's PolygonCollider2D and handles
/// proximity-based opacity and availability color tinting.
/// </summary>
public class SpriteOptionIndicator : MonoBehaviour
{
    // Tunable constants
    private const float MIN_OPACITY = 0f;
    private const float FULL_OPACITY = 1f;
    private const float OUTLINE_WIDTH = 0.2f;
    private const float OUTLINE_OFFSET = 0.15f;
    private const float TEXTURE_SCALE_X = 0.2f;
    private const float SCROLL_SPEED = 0.05f;

    private const float UI_OFFSET_X = 0f;
    private const float UI_OFFSET_Y = 30f;

    // Cached references
    private SpriteRenderer SpriteRenderer;
    public SpriteRenderer Sprite => SpriteRenderer;
    private PolygonCollider2D Collider;
    private List<LineRenderer> OutlineRenderers = new List<LineRenderer>();
    private List<float> OutlinePerimeters = new List<float>();

    // State
    public UI_SpriteEncounterOptionContainer OptionsContainer { get; private set; }
    public UI_EncounterOptionSpriteLabel LabelElement { get; private set; }
    public List<EncounterOption> Options { get; private set; }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<PolygonCollider2D>();

        if (SpriteRenderer == null)
            throw new System.Exception($"SpriteOptionIndicator requires a SpriteRenderer on {gameObject.name}");
        if (Collider == null)
            throw new System.Exception($"SpriteOptionIndicator requires a PolygonCollider2D on {gameObject.name}");
    }

    private void Update()
    {
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr == null || lr.material == null) continue;
            Vector2 offset = lr.material.mainTextureOffset;
            offset.x += SCROLL_SPEED * Time.deltaTime;
            lr.material.mainTextureOffset = offset;
        }
    }

    /// <summary>
    /// Binds this indicator to a container and option list, creating outline LineRenderers for each collider sub-path.
    /// </summary>
    public void Bind(UI_SpriteEncounterOptionContainer container, UI_EncounterOptionSpriteLabel label, List<EncounterOption> options)
    {
        OptionsContainer = container;
        LabelElement = label;
        Options = options;

        // Clear any existing outlines
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        OutlineRenderers.Clear();
        OutlinePerimeters.Clear();

        // Create one LineRenderer per collider sub-path
        Material outlineMaterial = ResourceManager.LoadMaterial("Encounters/DashedOutline");
        List<Vector2[]> outlines = ComputeOffsetOutlines(Collider, OUTLINE_OFFSET);

        for (int outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex++)
        {
            Vector2[] outline = outlines[outlineIndex];

            float perimeter = 0f;
            for (int i = 0; i < outline.Length; i++)
            {
                perimeter += Vector2.Distance(outline[i], outline[(i + 1) % outline.Length]);
            }
            OutlinePerimeters.Add(perimeter);

            GameObject outlineObj = new GameObject($"Outline_{outlineIndex}");
            outlineObj.transform.SetParent(transform, false);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;

            LineRenderer lineRenderer = outlineObj.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = outline.Length;
            lineRenderer.startWidth = OUTLINE_WIDTH;
            lineRenderer.endWidth = OUTLINE_WIDTH;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.textureScale = new Vector2(TEXTURE_SCALE_X, 1f);
            lineRenderer.material = outlineMaterial;
            lineRenderer.sortingLayerName = "SpriteOptionOutline";

            for (int i = 0; i < outline.Length; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(outline[i].x, outline[i].y, 0));
            }

            OutlineRenderers.Add(lineRenderer);
        }

        ApplyZoomScale();

        // Initial state
        UpdateAvailabilityColor();
        SetOutlineOpacity(false); // Start hidden
    }

    /// <summary>
    /// Rescales outline width and texture tiling so both appear visually consistent regardless of
    /// camera zoom, and snaps texture scale so each loop tiles seamlessly (whole number of dash repeats).
    /// </summary>
    public void ApplyZoomScale()
    {
        if (EncounterCamera.Instance == null) return;

        float zoomFactor = EncounterCamera.Instance.Camera.orthographicSize / EncounterCamera.DEFAULT_CAMERA_SIZE;
        float targetWidth = OUTLINE_WIDTH * zoomFactor;
        float targetDashWorldLength = (1f / TEXTURE_SCALE_X) * zoomFactor;

        for (int i = 0; i < OutlineRenderers.Count; i++)
        {
            LineRenderer lr = OutlineRenderers[i];
            if (lr == null) continue;

            float perimeter = OutlinePerimeters[i];
            int repeats = Mathf.Max(1, Mathf.RoundToInt(perimeter / targetDashWorldLength));
            float snappedTextureScaleX = repeats / perimeter;

            lr.startWidth = targetWidth;
            lr.endWidth = targetWidth;
            lr.textureScale = new Vector2(snappedTextureScaleX, 1f);
        }
    }

    public void SetLockedLineMaterial(bool locked)
    {
        foreach(LineRenderer lr in OutlineRenderers)
        {
            if (locked) lr.material = ResourceManager.LoadMaterial("Encounters/DashedOutline_Stroke");
            else lr.material = ResourceManager.LoadMaterial("Encounters/DashedOutline");
        }
    }

    /// <summary>
    /// Shows or hides all outline renderers.
    /// </summary>
    public void SetOutlineVisible(bool visible)
    {
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr != null) lr.enabled = visible;
        }
    }

    /// <summary>
    /// Updates the outline color based on whether any bound option is currently selectable.
    /// </summary>
    public void UpdateAvailabilityColor()
    {
        if (Options == null || OutlineRenderers.Count == 0) return;

        bool anySelectable = Options.Any(o => o.CanSelect(countInventoryItems: true));
        Color baseColor = anySelectable ? new Color(0.91f, 0.61f, 0f) : ResourceManager.Color_Button_Disabled;

        // Update all outline renderers (preserve alpha, which is controlled by proximity)
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr == null) continue;
            Color currentStart = lr.startColor;
            lr.startColor = new Color(baseColor.r, baseColor.g, baseColor.b, currentStart.a);
            lr.endColor = new Color(baseColor.r, baseColor.g, baseColor.b, currentStart.a);
        }
    }

    /// <summary>
    /// Sets the outline to full opacity or fully hidden. No longer distance-based -
    /// visibility is now driven entirely by whether this is the topmost indicator under the mouse.
    /// </summary>
    public void SetOutlineOpacity(bool visible)
    {
        if (OutlineRenderers.Count == 0) return;

        float targetAlpha = visible ? FULL_OPACITY : MIN_OPACITY;

        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr == null) continue;
            Color currentColor = lr.startColor;
            lr.startColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
            lr.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        }
    }

    private void OnDestroy()
    {
        // Clean up dynamically-created outline child GameObjects
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        OutlineRenderers.Clear();
    }


    /// <summary>
    /// Computes outward-offset outlines for all of the collider's sub-paths combined.
    /// Overlapping/crossing paths are automatically merged before offsetting, and concave
    /// corners are handled with round joins to avoid any spiking.
    /// </summary>
    private List<Vector2[]> ComputeOffsetOutlines(PolygonCollider2D collider, float offset)
    {
        PathsD inputPaths = new PathsD();
        for (int i = 0; i < collider.pathCount; i++)
        {
            Vector2[] path = collider.GetPath(i);
            PathD pathD = new PathD(path.Length);
            foreach (Vector2 p in path) pathD.Add(new PointD(p.x, p.y));
            inputPaths.Add(pathD);
        }

        PathsD resultPaths = Clipper.InflatePaths(inputPaths, offset, Clipper2Lib.JoinType.Round, Clipper2Lib.EndType.Polygon);

        List<Vector2[]> outlines = new List<Vector2[]>();
        foreach (PathD path in resultPaths)
        {
            Vector2[] verts = new Vector2[path.Count];
            for (int i = 0; i < path.Count; i++) verts[i] = new Vector2((float)path[i].x, (float)path[i].y);
            outlines.Add(verts);
        }
        return outlines;
    }

    /// <summary>
    /// Shows/hides the label and options container based on current hover/lock state.
    /// Called every frame for every registered indicator, so display state is always
    /// fully recomputed rather than relying on edge-triggered show/hide calls.
    /// </summary>
    public void SetDisplayState(bool isHovered, bool isLocked)
    {
        if (isLocked)
        {
            LabelElement.Hide();
            OptionsContainer.Show();
        }
        else if (isHovered)
        {
            // LabelElement.Show(); // Currently disabled, looks better without
            OptionsContainer.Hide();
        }
        else
        {
            LabelElement.Hide();
            OptionsContainer.Hide();
        }
    }

    /// <summary>
    /// Recomputes and applies the canvas position for both the label and options container,
    /// anchored to this sprite's collider bounds (top-center, offset toward top-right).
    /// </summary>
    public void RefreshUiPosition()
    {
        OptionsContainer.SetAnchoredPosition(ComputeUiAnchorPosition(OptionsContainer.GetComponent<RectTransform>()));
        LabelElement.SetAnchoredPosition(ComputeUiAnchorPosition(LabelElement.GetComponent<RectTransform>()));
    }

    private Vector2 ComputeUiAnchorPosition(RectTransform target)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(target);

        // Compute world-space anchor point (top-center of collider bounds)
        Vector3 worldMin = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 worldMax = new Vector3(float.MinValue, float.MinValue, 0);
        for (int pathIndex = 0; pathIndex < Collider.pathCount; pathIndex++)
        {
            Vector2[] path = Collider.GetPath(pathIndex);
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 worldPoint = transform.TransformPoint(path[i]);
                worldMin = Vector3.Min(worldMin, worldPoint);
                worldMax = Vector3.Max(worldMax, worldPoint);
            }
        }
        Vector3 anchorPoint = new Vector3((worldMin.x + worldMax.x) / 2f, worldMax.y, 0);

        Vector3 screenPoint = Game.Instance.MainCamera.WorldToScreenPoint(anchorPoint);

        Canvas canvas = target.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera canvasCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvasCamera, out Vector2 localPoint);

        localPoint += new Vector2(UI_OFFSET_X, UI_OFFSET_Y);

        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 targetSize = target.rect.size;

        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f - targetSize.x;
        float minY = -canvasSize.y * 0.5f;
        float maxY = canvasSize.y * 0.5f - targetSize.y;

        localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

        return localPoint;
    }
}
