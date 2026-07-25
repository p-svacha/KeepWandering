using Clipper2Lib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Static manager for sprite-bound encounter option interactions.
/// Handles hover detection, locking, drag-hold-to-lock, reveal-hotspots, and positioning updates.
/// </summary>
public static class SpriteOptionInteractionManager
{
    // Tunable constants
    private const float DRAG_HOLD_TO_LOCK_TIME = 1.5f;

    private const float OUTLINE_OFFSET = 0.15f;
    private const float MIN_OPACITY = 0f;
    private const float FULL_OPACITY = 1f;
    private const float ITEM_HOVER_OPACITY = 0.4f;

    public static string SPRITE_ENCOUNTER_OPTION_LAYER = "EncounterOptionSprite";

    private static readonly KeyCode REVEAL_HOTSPOTS_KEY = KeyCode.LeftAlt;

    // State
    private static bool SubscribedToCameraEvents = false;
    private static Dictionary<SpriteRenderer, SpriteOptionIndicator> ActiveIndicators = new Dictionary<SpriteRenderer, SpriteOptionIndicator>();
    public static IReadOnlyDictionary<SpriteRenderer, SpriteOptionIndicator> GetActiveIndicators() => ActiveIndicators;
    public static SpriteOptionIndicator HoveredIndicator { get; private set; }
    public static SpriteOptionIndicator LockedIndicator { get; private set; }


    // Drag-hold state
    private static SpriteOptionIndicator DragHoldTarget;
    private static float DragHoldTimer;

    public static void Init()
    {
        SubscribedToCameraEvents = false;
    }

    /// <summary>
    /// Registers a sprite with bound options, creating/binding an indicator component.
    /// </summary>
    public static void RegisterSprite(SpriteRenderer sprite, UI_SpriteEncounterOptionContainer container, UI_EncounterOptionSpriteLabel label, List<EncounterOption> options)
    {
        if (sprite == null) return;
        EnsureSubscribedToCamera();

        // Get or add the indicator component
        SpriteOptionIndicator indicator = sprite.GetComponent<SpriteOptionIndicator>();
        if (indicator == null) indicator = sprite.gameObject.AddComponent<SpriteOptionIndicator>();

        // Bind it with the container and options
        indicator.Bind(container, label, options);
        indicator.RefreshUiPosition();
        indicator.SetDisplayState(false, false);

        // Track it
        if (!ActiveIndicators.ContainsKey(sprite))
        {
            ActiveIndicators.Add(sprite, indicator);
        }
        else
        {
            ActiveIndicators[sprite] = indicator;
        }
    }

    private static void EnsureSubscribedToCamera()
    {
        if (SubscribedToCameraEvents) return;
        if (EncounterCamera.Instance == null) return;

        EncounterCamera.Instance.OnTransitionComplete += OnCameraTransitionComplete;
        SubscribedToCameraEvents = true;
    }

    private static void OnCameraTransitionComplete()
    {
        foreach (var kvp in ActiveIndicators)
        {
            kvp.Value?.RefreshUiPosition();
            kvp.Value?.ApplyZoomScale();
        }
    }

    /// <summary>
    /// Clears all tracked indicators and resets state. Called when starting a new encounter step.
    /// </summary>
    public static void ClearAll()
    {
        // Destroy all tracked indicator components
        foreach (var kvp in ActiveIndicators)
        {
            if (kvp.Value != null)
            {
                Object.DestroyImmediate(kvp.Value);
            }
        }

        ActiveIndicators.Clear();
        HoveredIndicator = null;
        LockedIndicator = null;
        DragHoldTarget = null;
        DragHoldTimer = 0f;
    }

    /// <summary>
    /// Refreshes the availability color of a specific sprite's indicator.
    /// </summary>
    public static void RefreshAvailability(SpriteRenderer sprite)
    {
        if (sprite == null) return;
        if (ActiveIndicators.TryGetValue(sprite, out SpriteOptionIndicator indicator))
        {
            indicator.UpdateAvailabilityColor();
        }
    }

    /// <summary>
    /// Attempts to drop the given item onto whichever registered sprite is currently under the mouse cursor.
    /// If a bound option has an unfilled slot that accepts the item, the item is slotted into the first such
    /// slot of the first eligible option, and the sprite's indicator is immediately locked.
    /// Returns true if the item was successfully dropped onto a sprite.
    /// </summary>
    public static bool TryDropItemOnSprite(Item item)
    {
        if (Game.Instance == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return false;

        SpriteOptionIndicator indicator = GetIndicatorUnderMouse(item);
        if (indicator == null) return false;

        foreach (EncounterOption option in indicator.Options)
        {
            foreach (ItemSlot slot in option.ItemSlots)
            {
                if (!slot.IsFilled && slot.CanAcceptItem(item))
                {
                    slot.Fill(item);
                    EnsureLocked(indicator);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Per-frame update logic. Must be called from Game.Update() unconditionally (even while dragging).
    /// </summary>
    public static void Update()
    {
        if (Game.Instance == null) return;

        // Hide/show outlines based on camera transition state
        bool isTransitioning = EncounterCamera.Instance != null && EncounterCamera.Instance.IsTransitioning;
        if (isTransitioning)
        {
            // Hide all outlines during camera movement
            foreach (var kvp in ActiveIndicators)
            {
                kvp.Value?.SetOutlineVisible(false);
            }
        }
        else
        {
            // Show all outlines when camera is stable
            foreach (var kvp in ActiveIndicators)
            {
                kvp.Value?.SetOutlineVisible(true);
            }

            Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            bool revealHotspots = Input.GetKey(REVEAL_HOTSPOTS_KEY);

            SpriteOptionIndicator hoveredIndicator = GetIndicatorUnderMouse();
            Item relevantItem = ItemDragDropManager.IsDragging ? ItemDragDropManager.DraggedItem : Game.Instance.CurrentHoverItem;

            // While dragging, find the topmost sprite that can actually accept the item,
            // skipping ineligible sprites even if they're visually in front.
            SpriteOptionIndicator effectiveHoveredIndicator = (ItemDragDropManager.IsDragging && relevantItem != null)
                ? GetIndicatorUnderMouse(relevantItem)
                : hoveredIndicator;

            bool itemCanBeSlotted = relevantItem != null && ActiveIndicators.Values.Any(i => IndicatorHasSlotFor(i, relevantItem));

            // Override hoveredNow to null if the mouse is over a UI element (to prevent accidental locking when clicking on UI)
            if (EventSystem.current.IsPointerOverGameObject()) effectiveHoveredIndicator = null;

            // Check if hovering/dragging an item that can be slotted into any sprite-bound option
            foreach (var kvp in ActiveIndicators)
            {
                SpriteOptionIndicator indicator = kvp.Value;

                float targetOpacity;
                if (revealHotspots || indicator == LockedIndicator || indicator == effectiveHoveredIndicator)
                {
                    targetOpacity = 1f;
                }
                else if (itemCanBeSlotted && IndicatorHasSlotFor(indicator, relevantItem))
                {
                    targetOpacity = 0.5f;
                }
                else
                {
                    targetOpacity = 0f;
                }

                indicator.SetOutlineOpacity(targetOpacity);
                indicator.SetDisplayState(indicator == effectiveHoveredIndicator, indicator == LockedIndicator);
            }

            HoveredIndicator = hoveredIndicator;

            HandleClick();
            HandleRightClick();
            HandleDragHold();
        }
    }

    private static bool IndicatorHasSlotFor(SpriteOptionIndicator indicator, Item item)
    {
        foreach (EncounterOption option in indicator.Options)
        {
            foreach (ItemSlot slot in option.ItemSlots)
            {
                if (!slot.IsFilled && slot.CanAcceptItem(item)) return true;
            }
        }
        return false;
    }

    private static void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Don't unlock if this click is about to start an item drag
        if (Game.Instance.CurrentHoverItem != null && ItemDragDropManager.CanDragItem(Game.Instance.CurrentHoverItem))
        {
            return;
        }

        // Clicking a hovered sprite locks/transfers the lock
        if (HoveredIndicator != null)
        {
            // If clicking the already-locked sprite, do nothing (keep it locked)
            if (HoveredIndicator == LockedIndicator) return;

            // Hide the previously locked card
            if (LockedIndicator != null) ClearLock();

            // Lock the newly clicked sprite
            LockIndicator(HoveredIndicator);
        }

        // Clicking empty space clears the lock
        else ClearLock();
    }

    private static void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        // Right-click anywhere unlocks
        ClearLock();
    }

    public static void ClearLock()
    {
        if (LockedIndicator == null) return;


        LockedIndicator.SetLockedLineMaterial(false);

        // Return any items placed in this sprite's option slots back to the inventory
        foreach (EncounterOption option in LockedIndicator.Options)
        {
            foreach (ItemSlot slot in option.ItemSlots)
            {
                if (slot.IsFilled) slot.Empty();
            }
        }

        LockedIndicator = null;

        // UI
        UI_EncounterDisplay.Instance.OnOptionUnhovered();
    }

    private static void LockIndicator(SpriteOptionIndicator indicator)
    {
        LockedIndicator = indicator;
        LockedIndicator.SetLockedLineMaterial(true);
    }

    /// <summary>
    /// Ensures the given indicator is locked. If a different indicator is currently locked, clears it first.
    /// </summary>
    public static void EnsureLocked(SpriteOptionIndicator indicator)
    {
        if (LockedIndicator != indicator)
        {
            if (LockedIndicator != null) ClearLock();
            LockIndicator(indicator);
        }
    }

    private static void HandleDragHold()
    {
        if (!ItemDragDropManager.IsDragging)
        {
            DragHoldTarget = null;
            DragHoldTimer = 0f;
            return;
        }

        SpriteOptionIndicator currentDragTarget = GetIndicatorUnderMouse(ItemDragDropManager.DraggedItem);

        if (currentDragTarget != DragHoldTarget)
        {
            DragHoldTarget = currentDragTarget;
            DragHoldTimer = 0f;
        }

        if (DragHoldTarget != null)
        {
            DragHoldTimer += Time.deltaTime;

            if (DragHoldTimer >= DRAG_HOLD_TO_LOCK_TIME && LockedIndicator != DragHoldTarget)
            {
                if (LockedIndicator != null) ClearLock();
                LockIndicator(DragHoldTarget);
            }
        }
    }

    /// <summary>
    /// Returns the registered sprite indicator currently under the mouse (topmost by sorting), or null.
    /// If filterItem is provided, indicators with no slot that can accept it are skipped entirely,
    /// so the topmost eligible sprite is found even if an ineligible sprite is visually in front of it.
    /// </summary>
    private static SpriteOptionIndicator GetIndicatorUnderMouse(Item filterItem = null)
    {
        Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        int layer_mask = LayerMask.GetMask(SPRITE_ENCOUNTER_OPTION_LAYER);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity, layer_mask);

        if (hits.Length == 0) return null;

        SpriteOptionIndicator topmost = null;
        int topmostLayerValue = int.MinValue;
        int topmostSortingOrder = int.MinValue;

        foreach (RaycastHit2D hit in hits)
        {
            if (!ActiveIndicators.TryGetValue(hit.collider.GetComponent<SpriteRenderer>(), out SpriteOptionIndicator indicator)) continue;
            if (filterItem != null && !IndicatorHasSlotFor(indicator, filterItem)) continue;

            SpriteRenderer sr = indicator.Sprite;
            if (sr == null) continue;

            int layerValue = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
            int sortingOrder = sr.sortingOrder;

            bool isOnTop = topmost == null
                || layerValue > topmostLayerValue
                || (layerValue == topmostLayerValue && sortingOrder > topmostSortingOrder);

            if (isOnTop)
            {
                topmost = indicator;
                topmostLayerValue = layerValue;
                topmostSortingOrder = sortingOrder;
            }
        }

        return topmost;
    }


    /// <summary>
    /// Ensures that the given sprite renderer is configured to have an enlarged PolygonCollider2D that matches the sprite's bounds, so that the sprite can be hovered and interacted with even if its visible sprite is small.
    /// </summary>
    public static void SetupEncounterSpriteCollider(SpriteRenderer spriteRenderer)
    {
        GameObject obj = spriteRenderer.gameObject;
        // If there is no collider yet, add one and enlarge it
        if (obj.GetComponent<PolygonCollider2D>() == null)
        {
            // Add collider (initial shape will match the sprite's bounds)
            PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();

            // Extract the collider's paths
            List<Vector2[]> colliderPaths = new List<Vector2[]>();
            for (int i = 0; i < collider.pathCount; i++) colliderPaths.Add(collider.GetPath(i));

            // Enlarge the collider paths outward by a fixed offset to make it easier to hover/interact with
            List<Vector2[]> enlargedColliderPaths = ComputeOffsetOutlines(colliderPaths, OUTLINE_OFFSET);

            // Replace the collider's paths with the enlarged paths
            collider.pathCount = enlargedColliderPaths.Count;
            for (int i = 0; i < enlargedColliderPaths.Count; i++) collider.SetPath(i, enlargedColliderPaths[i]);
        }

        // Ensure the collider is a trigger and on the correct layer for raycasting
        obj.GetComponent<PolygonCollider2D>().isTrigger = true;
        obj.layer = LayerMask.NameToLayer(SPRITE_ENCOUNTER_OPTION_LAYER);
    }

    /// <summary>
    /// Computes outward-offset outlines for all of the collider's sub-paths combined.
    /// Overlapping/crossing paths are automatically merged before offsetting, and concave
    /// corners are handled with round joins to avoid any spiking.
    /// </summary>
    private static List<Vector2[]> ComputeOffsetOutlines(List<Vector2[]> sourcePaths, float offset)
    {
        PathsD inputPaths = new PathsD();
        foreach (Vector2[] path in sourcePaths)
        {
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
}
