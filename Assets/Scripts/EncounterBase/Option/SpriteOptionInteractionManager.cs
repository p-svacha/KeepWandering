using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Static manager for sprite-bound encounter option interactions.
/// Handles hover detection, locking, drag-hold-to-lock, reveal-hotspots, and positioning updates.
/// </summary>
public static class SpriteOptionInteractionManager
{
    // Tunable constants
    private const float DRAG_HOLD_TO_LOCK_TIME = 0.5f;
    private static readonly KeyCode REVEAL_HOTSPOTS_KEY = KeyCode.LeftAlt;

    // State
    private static bool SubscribedToCameraEvents = false;
    private static Dictionary<GameObject, SpriteOptionIndicator> ActiveIndicators = new Dictionary<GameObject, SpriteOptionIndicator>();
    public static IReadOnlyDictionary<GameObject, SpriteOptionIndicator> GetActiveIndicators() => ActiveIndicators;
    private static SpriteOptionIndicator HoveredIndicator;
    private static SpriteOptionIndicator LockedIndicator;

    // Drag-hold state
    private static SpriteOptionIndicator DragHoldTarget;
    private static float DragHoldTimer;

    /// <summary>
    /// Registers a sprite with bound options, creating/binding an indicator component.
    /// </summary>
    public static void RegisterSprite(GameObject sprite, UI_SpriteEncounterOptionContainer container, UI_EncounterOptionSpriteLabel label, List<EncounterOption> options)
    {
        if (sprite == null) return;
        EnsureSubscribedToCamera();

        // Get or add the indicator component
        SpriteOptionIndicator indicator = sprite.GetComponent<SpriteOptionIndicator>();
        if (indicator == null)
        {
            indicator = sprite.AddComponent<SpriteOptionIndicator>();
        }

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
    public static void RefreshAvailability(GameObject sprite)
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

        SpriteOptionIndicator indicator = GetIndicatorUnderMouse();
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
            SpriteOptionIndicator hoveredNow = GetIndicatorUnderMouse();

            // Check if hovering/dragging an item that can be slotted into any sprite-bound option
            Item relevantItem = ItemDragDropManager.IsDragging ? ItemDragDropManager.DraggedItem : Game.Instance.CurrentHoverItem;
            bool itemCanBeSlotted = false;
            if (relevantItem != null)
            {
                foreach (var kvp in ActiveIndicators)
                {
                    foreach (EncounterOption option in kvp.Value.Options)
                    {
                        foreach (ItemSlot slot in option.ItemSlots)
                        {
                            if (!slot.IsFilled && slot.CanAcceptItem(relevantItem))
                            {
                                itemCanBeSlotted = true;
                                break;
                            }
                        }
                        if (itemCanBeSlotted) break;
                    }
                    if (itemCanBeSlotted) break;
                }
            }

            // Update proximity for all active indicators
            foreach (var kvp in ActiveIndicators)
            {
                SpriteOptionIndicator indicator = kvp.Value;
                bool shouldReveal = revealHotspots || indicator == LockedIndicator || indicator == hoveredNow;
                if (!shouldReveal && itemCanBeSlotted && relevantItem != null)
                {
                    foreach (EncounterOption option in indicator.Options)
                    {
                        foreach (ItemSlot slot in option.ItemSlots)
                        {
                            if (!slot.IsFilled && slot.CanAcceptItem(relevantItem))
                            {
                                shouldReveal = true;
                                break;
                            }
                        }
                        if (shouldReveal) break;
                    }
                }

                indicator.SetOutlineOpacity(shouldReveal);
                indicator.SetDisplayState(indicator == hoveredNow, indicator == LockedIndicator);
            }

            HoveredIndicator = hoveredNow;

            HandleClick();
            HandleRightClick();
            HandleDragHold();
        }
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
        if (LockedIndicator != null)
        {
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
        }
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

        SpriteOptionIndicator currentDragTarget = GetIndicatorUnderMouse();

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
    /// Returns the registered sprite indicator currently under the given world position (topmost by sorting), or null.
    /// </summary>
    private static SpriteOptionIndicator GetIndicatorUnderMouse()
    {
        Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        int layer_mask = LayerMask.GetMask(UI_EncounterDisplay.SPRITE_ENCOUNTER_OPTION_LAYER);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity, layer_mask);

        if (hits.Length == 0) return null;

        SpriteOptionIndicator topmost = null;
        int topmostLayerValue = int.MinValue;
        int topmostSortingOrder = int.MinValue;

        foreach (RaycastHit2D hit in hits)
        {
            if (!ActiveIndicators.TryGetValue(hit.collider.gameObject, out SpriteOptionIndicator indicator)) continue;

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
}
