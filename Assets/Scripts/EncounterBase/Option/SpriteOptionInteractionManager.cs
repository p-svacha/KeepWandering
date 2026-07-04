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
    private static Dictionary<GameObject, SpriteOptionIndicator> ActiveIndicators = new Dictionary<GameObject, SpriteOptionIndicator>();
    private static SpriteOptionIndicator HoveredIndicator;
    private static SpriteOptionIndicator LockedIndicator;

    // Drag-hold state
    private static SpriteOptionIndicator DragHoldTarget;
    private static float DragHoldTimer;

    // Transition edge detection
    private static bool WasTransitioning;

    /// <summary>
    /// Registers a sprite with bound options, creating/binding an indicator component.
    /// </summary>
    public static void RegisterSprite(GameObject sprite, UI_SpriteEncounterOptionContainer container, List<EncounterOption> options)
    {
        if (sprite == null) return;

        // Get or add the indicator component
        SpriteOptionIndicator indicator = sprite.GetComponent<SpriteOptionIndicator>();
        if (indicator == null)
        {
            indicator = sprite.AddComponent<SpriteOptionIndicator>();
        }

        // Bind it with the container and options
        indicator.Bind(container, options);

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
        WasTransitioning = false;
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
    /// Per-frame update logic. Must be called from Game.Update() unconditionally (even while dragging).
    /// </summary>
    public static void Update()
    {
        if (Game.Instance == null) return;

        Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        bool revealHotspots = Input.GetKey(REVEAL_HOTSPOTS_KEY);

        // Update proximity for all active indicators
        foreach (var kvp in ActiveIndicators)
        {
            if (kvp.Value != null)
            {
                kvp.Value.UpdateProximity(mouseWorldPos, revealHotspots);
            }
        }

        // Hover detection via raycast
        UpdateHover(mouseWorldPos);

        // Click handling (lock/unlock/transfer)
        HandleClick();

        // Drag-hold-to-lock
        HandleDragHold(mouseWorldPos);

        // Transition edge detection for repositioning
        HandleTransitionEdge();
    }

    private static void UpdateHover(Vector2 mouseWorldPos)
    {
        SpriteOptionIndicator prevHovered = HoveredIndicator;
        SpriteOptionIndicator newHovered = null;

        // Raycast to find hovered sprite
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (ActiveIndicators.TryGetValue(hitObject, out SpriteOptionIndicator indicator))
            {
                newHovered = indicator;
            }
        }

        // Update hover state
        if (newHovered != prevHovered)
        {
            // Hide previously hovered card (unless it's locked)
            if (prevHovered != null && prevHovered != LockedIndicator)
            {
                prevHovered.Container.Hide();
            }

            HoveredIndicator = newHovered;

            // Show newly hovered card (respecting "suppress peek while locked" for plain hover)
            if (newHovered != null && LockedIndicator == null)
            {
                newHovered.Container.Show();
            }
        }
    }

    private static void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Clicking a hovered sprite locks/transfers the lock
        if (HoveredIndicator != null)
        {
            // If clicking the already-locked sprite, do nothing (keep it locked)
            if (HoveredIndicator == LockedIndicator) return;

            // Hide the previously locked card
            if (LockedIndicator != null)
            {
                LockedIndicator.Container.SetLocked(false);
                LockedIndicator.Container.Hide();
            }

            // Lock the newly clicked sprite
            LockedIndicator = HoveredIndicator;
            LockedIndicator.Container.SetLocked(true);
            LockedIndicator.Container.Show();
        }
        // Clicking empty space clears the lock
        else
        {
            if (LockedIndicator != null)
            {
                LockedIndicator.Container.SetLocked(false);
                LockedIndicator.Container.Hide();
                LockedIndicator = null;
            }
        }
    }

    private static void HandleDragHold(Vector2 mouseWorldPos)
    {
        if (!ItemDragDropManager.IsDragging)
        {
            // Not dragging - reset drag-hold state
            DragHoldTarget = null;
            DragHoldTimer = 0f;
            return;
        }

        // While dragging, detect which sprite the item is held over
        SpriteOptionIndicator currentDragTarget = null;
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (ActiveIndicators.TryGetValue(hitObject, out SpriteOptionIndicator indicator))
            {
                currentDragTarget = indicator;
            }
        }

        // If target changed, reset timer
        if (currentDragTarget != DragHoldTarget)
        {
            DragHoldTarget = currentDragTarget;
            DragHoldTimer = 0f;

            // If we moved away from a sprite, hide its card (unless it's locked)
            if (DragHoldTarget == null && HoveredIndicator != null && HoveredIndicator != LockedIndicator)
            {
                HoveredIndicator.Container.Hide();
            }
        }

        // If holding over a sprite, accumulate time
        if (DragHoldTarget != null)
        {
            DragHoldTimer += Time.deltaTime;

            // Once threshold is reached, lock the sprite and show its card
            if (DragHoldTimer >= DRAG_HOLD_TO_LOCK_TIME)
            {
                // If not already locked, lock it now
                if (LockedIndicator != DragHoldTarget)
                {
                    // Hide the previously locked card
                    if (LockedIndicator != null)
                    {
                        LockedIndicator.Container.SetLocked(false);
                        LockedIndicator.Container.Hide();
                    }

                    // Lock the drag target
                    LockedIndicator = DragHoldTarget;
                    LockedIndicator.Container.SetLocked(true);
                    LockedIndicator.Container.Show();
                }

                // Keep the card visible while dragging over it (bypasses peek suppression)
                DragHoldTarget.Container.Show();
            }
        }
    }

    private static void HandleTransitionEdge()
    {
        if (EncounterCamera.Instance == null) return;

        bool isTransitioning = EncounterCamera.Instance.IsTransitioning;

        // Detect transition completion (true → false edge)
        if (WasTransitioning && !isTransitioning)
        {
            // Refresh positions of all active containers
            foreach (var kvp in ActiveIndicators)
            {
                if (kvp.Value != null && kvp.Value.Container != null)
                {
                    kvp.Value.Container.RefreshPosition();
                }
            }
        }

        WasTransitioning = isTransitioning;
    }
}
