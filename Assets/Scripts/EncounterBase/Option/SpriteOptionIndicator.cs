using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Dynamically added component to any sprite that has bound encounter options.
/// Renders a dashed outline traced from the sprite's PolygonCollider2D and handles
/// proximity-based opacity and availability color tinting.
/// </summary>
public class SpriteOptionIndicator : MonoBehaviour
{
    // Tunable constants
    private const float MIN_OPACITY = 0.25f;
    private const float FULL_OPACITY = 1f;
    private const float MAX_PROXIMITY_DISTANCE = 2.5f;
    private const float OUTLINE_WIDTH = 0.05f;

    // Cached references
    private SpriteRenderer SpriteRenderer;
    private PolygonCollider2D Collider;
    private List<LineRenderer> OutlineRenderers = new List<LineRenderer>();

    // State
    public UI_SpriteEncounterOptionContainer Container { get; private set; }
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

    /// <summary>
    /// Binds this indicator to a container and option list, creating outline LineRenderers for each collider sub-path.
    /// </summary>
    public void Bind(UI_SpriteEncounterOptionContainer container, List<EncounterOption> options)
    {
        Container = container;
        Options = options;

        // Clear any existing outlines
        foreach (LineRenderer lr in OutlineRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        OutlineRenderers.Clear();

        // Create one LineRenderer per collider sub-path
        Material outlineMaterial = ResourceManager.LoadMaterial("Encounters/DashedOutline");
        for (int pathIndex = 0; pathIndex < Collider.pathCount; pathIndex++)
        {
            Vector2[] path = Collider.GetPath(pathIndex);

            // Create child GameObject with LineRenderer
            GameObject outlineObj = new GameObject($"Outline_{pathIndex}");
            outlineObj.transform.SetParent(transform, false);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;

            LineRenderer lineRenderer = outlineObj.AddComponent<LineRenderer>();

            // Configure LineRenderer
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = path.Length;
            lineRenderer.startWidth = OUTLINE_WIDTH;
            lineRenderer.endWidth = OUTLINE_WIDTH;
            lineRenderer.textureMode = LineTextureMode.Tile;

            // Set material (accessing .material auto-instances for per-renderer tinting)
            lineRenderer.material = outlineMaterial;

            // Sync sorting with the bound SpriteRenderer
            lineRenderer.sortingLayerID = SpriteRenderer.sortingLayerID;
            lineRenderer.sortingOrder = SpriteRenderer.sortingOrder + 1;

            // Copy collider path points
            for (int i = 0; i < path.Length; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(path[i].x, path[i].y, 0));
            }

            OutlineRenderers.Add(lineRenderer);
        }

        // Initial state
        UpdateAvailabilityColor();
        UpdateProximity(Vector2.zero, false); // Start at min opacity
    }

    /// <summary>
    /// Updates the outline color based on whether any bound option is currently selectable.
    /// </summary>
    public void UpdateAvailabilityColor()
    {
        if (Options == null || OutlineRenderers.Count == 0) return;

        bool anySelectable = Options.Any(o => o.CanSelect());
        Color baseColor = anySelectable ? ResourceManager.Color_Button_Default : ResourceManager.Color_Button_Disabled;

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
    /// Updates the outline opacity based on cursor proximity to the sprite's collider, or snaps to full opacity if revealAll is true.
    /// </summary>
    public void UpdateProximity(Vector2 mouseWorldPos, bool revealAll)
    {
        if (Collider == null || OutlineRenderers.Count == 0) return;

        float targetAlpha;

        if (revealAll)
        {
            targetAlpha = FULL_OPACITY;
        }
        else
        {
            // Compute shape-aware distance using ClosestPoint
            Vector2 closestPoint = Collider.ClosestPoint(mouseWorldPos);
            float distance = Vector2.Distance(mouseWorldPos, closestPoint);

            // Map distance to opacity: 0 distance = full opacity, MAX_PROXIMITY_DISTANCE = min opacity
            float t = Mathf.Clamp01(distance / MAX_PROXIMITY_DISTANCE);
            targetAlpha = Mathf.Lerp(FULL_OPACITY, MIN_OPACITY, t);
        }

        // Apply alpha to all outline renderers (preserve RGB from availability color)
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
}
