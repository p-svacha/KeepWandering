using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemDragDropManager
{
    private const float OFF_SCREEN_MIN_X = -12f;
    private const float OFF_SCREEN_MIN_Y = -8f;

    private const float MAX_RELEASE_VELOCITY = 25f;
    private const float MAX_RELEASE_ANGULAR_VELOCITY = 360f;

    // Drag state
    public static bool IsDragging { get; private set; }
    public static Item DraggedItem { get; private set; }

    // Drop target tracking
    public static UI_ItemSlot HoveredItemSlot;
    public static UI_EncounterStepOption HoveredOptionDisplay;
    private static List<UI_EncounterStepOption> GreyedOutOptions = new List<UI_EncounterStepOption>();
    private static List<UI_ItemSlot> HighlightedSlots = new List<UI_ItemSlot>();

    public static void Update()
    {
        if (IsDragging)
        {
            UpdateDrag();

            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }

        CheckDeferredLayerRestore();
        CheckItemsOffScreen();
    }

    public static bool CanDragItem(Item item)
    {
        return item.IsPlayerOwned && !item.Renderer.IsFrozen;
    }

    public static void StartDrag(Item item)
    {
        if (IsDragging) return;
        if (!CanDragItem(item)) return;

        IsDragging = true;
        DraggedItem = item;

        Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Disable collider so it doesn't interfere with raycasts
        item.Renderer.SetColliderEnabled(false);

        // Render above UI
        item.Renderer.SetRenderAboveUI(true);

        // Start pendulum physics - item becomes dynamic with a hinge at the grab point
        item.Renderer.StartDragPhysics(mouseWorldPos);

        // Hide tooltip and context menu
        Game.Instance.UI.HideAllTooltips();
        UI_ContextMenu.Instance.Hide();

        // Grey out invalid drop targets
        GreyOutInvalidTargets(item);
    }

    public static void CancelDrag()
    {
        if (!IsDragging) return;

        if (DraggedItem != null)
        {
            DraggedItem.Renderer.StopDragPhysics();
            DraggedItem.Renderer.SetRenderAboveUI(false);
            DraggedItem.Renderer.SetColliderEnabled(true);
            DraggedItem.Unfreeze();
        }

        RestoreAllTargets();
        IsDragging = false;
        DraggedItem = null;
    }

    private static void UpdateDrag()
    {
        if (DraggedItem == null)
        {
            CancelDrag();
            return;
        }

        // Move the hinge anchor to the mouse position - the item swings naturally around the grab point
        Vector2 mouseWorldPos = Game.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        DraggedItem.Renderer.UpdateDragAnchor(mouseWorldPos);
    }

    private static void EndDrag()
    {
        if (DraggedItem == null)
        {
            CancelDrag();
            return;
        }

        Item item = DraggedItem;

        // Stop pendulum physics - item keeps its velocity for inertia/flicking
        item.Renderer.StopDragPhysics();
        item.Renderer.ClampVelocity(MAX_RELEASE_VELOCITY, MAX_RELEASE_ANGULAR_VELOCITY);

        // Re-enable collider
        item.Renderer.SetColliderEnabled(true);

        // Try to drop on a valid target
        if (TryDropOnTarget(item))
        {
            // Successfully slotted - restore layer immediately (item is hidden by Fill)
            item.Renderer.SetRenderAboveUI(false);
        }
        else
        {
            // No valid target - item keeps flying with inertia.
            // Layer restore is deferred until the item falls back near the cart (y < 0).
            item.Unfreeze();
        }

        RestoreAllTargets();
        IsDragging = false;
        DraggedItem = null;
    }

    private static bool TryDropOnTarget(Item item)
    {
        // Priority 1: Specific hovered slot
        if (HoveredItemSlot != null)
        {
            if (IsMouseOverRectTransform((RectTransform)HoveredItemSlot.transform) && HoveredItemSlot.ItemSlot.CanAcceptItem(item))
            {
                HoveredItemSlot.ItemSlot.Fill(item);
                return true;
            }
            else
            {
                HoveredItemSlot = null; // stale - clear so it doesn't linger into later frames
            }
        }

        // Priority 2: Hovered option
        if (HoveredOptionDisplay != null)
        {
            if (IsMouseOverRectTransform((RectTransform)HoveredOptionDisplay.transform))
            {
                foreach (UI_ItemSlot slot in HoveredOptionDisplay.ItemSlotDisplays)
                {
                    if (!slot.ItemSlot.IsFilled && slot.ItemSlot.CanAcceptItem(item))
                    {
                        slot.ItemSlot.Fill(item);
                        return true;
                    }
                }
            }
            else
            {
                HoveredOptionDisplay = null;
            }
        }

        // Priority 3: Sprite-bound option under the cursor
        if (SpriteOptionInteractionManager.TryDropItemOnSprite(item))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether the current mouse position is actually within the given RectTransform's screen bounds.
    /// Handles both Screen Space - Overlay and Screen Space - Camera / World Space canvases.
    /// </summary>
    private static bool IsMouseOverRectTransform(RectTransform rect)
    {
        if (rect == null) return false;

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null) return false;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
    }

    #region Deferred Layer Restore

    /// <summary>
    /// Items released from a drag stay rendered above the UI until they fall back below y=0.
    /// This prevents them from visually disappearing behind the UI panel mid-flight.
    /// </summary>
    private static void CheckDeferredLayerRestore()
    {
        if (Game.Instance == null) return;

        foreach (Item item in Game.Instance.Inventory)
        {
            if (IsDragging && item == DraggedItem) continue;
            if (!item.Renderer.IsRenderingAboveUI) continue;

            if (item.Renderer.transform.position.y < 0f)
            {
                item.Renderer.SetRenderAboveUI(false);
            }
        }
    }

    #endregion

    #region Target Greying Out

    private static void GreyOutInvalidTargets(Item item)
    {
        if (UI_EncounterDisplay.Instance == null) return;

        // Get all option displays (including fixed outcome options not in the dictionary)
        List<UI_EncounterStepOption> allOptionDisplays = UI_EncounterDisplay.Instance.OptionDisplays.Values.ToList();

        Debug.Log($"Greying out invalid targets for {item.Label}, isPlayerOwned={item.IsPlayerOwned}, numOptionDisplays={allOptionDisplays.Count}, itemDef = {item.Def.DefName}");

        foreach (UI_EncounterStepOption optionDisplay in allOptionDisplays)
        {
            Debug.Log($"Checking option display for option {optionDisplay.Option.Text}, numItemSlots={optionDisplay.ItemSlotDisplays.Count}");

            // Grey out the option button itself
            optionDisplay.SetDragGreyedOut(true);
            GreyedOutOptions.Add(optionDisplay);

            // Grey out invalid slots, highlight valid ones
            foreach (UI_ItemSlot slot in optionDisplay.ItemSlotDisplays)
            {
                Debug.Log($"Checking slot {slot.ItemSlot}, isFilled={slot.ItemSlot.IsFilled}, canAccept={slot.ItemSlot.CanAcceptItem(item)}");
                if (!slot.ItemSlot.IsFilled && slot.ItemSlot.CanAcceptItem(item))
                {
                    slot.SetDragHighlighted(true);
                    HighlightedSlots.Add(slot);
                }
                else
                {
                    slot.SetDragGreyedOut(true);
                }
            }
        }

        // Future: Grey out other invalid targets (SpriteRenderers, etc.)
    }

    private static void RestoreAllTargets()
    {
        // Restore greyed out options and their slots
        foreach (UI_EncounterStepOption optionDisplay in GreyedOutOptions)
        {
            if (optionDisplay == null) continue;

            optionDisplay.SetDragGreyedOut(false);
            foreach (UI_ItemSlot slot in optionDisplay.ItemSlotDisplays)
            {
                slot.SetDragGreyedOut(false);
            }
        }
        GreyedOutOptions.Clear();

        // Restore highlighted slots
        foreach (UI_ItemSlot slot in HighlightedSlots)
        {
            if (slot != null) slot.SetDragHighlighted(false);
        }
        HighlightedSlots.Clear();
    }

    #endregion

    #region Off-Screen Recovery

    private static void CheckItemsOffScreen()
    {
        if (Game.Instance == null) return;

        float offScreenMaxX = 12f;
        float offScreenMinX = OFF_SCREEN_MIN_X + Game.Instance.CurrentEncounter.CameraXOffset;

        if (EncounterCamera.Instance.Camera.orthographicSize > EncounterCamera.DEFAULT_CAMERA_SIZE)
        {
            float increase = (EncounterCamera.Instance.Camera.orthographicSize - EncounterCamera.DEFAULT_CAMERA_SIZE) * EncounterCamera.Instance.Camera.aspect * 2f;
            offScreenMaxX += increase;
        }

        for (int i = Game.Instance.Inventory.Count - 1; i >= 0; i--)
        {
            Item item = Game.Instance.Inventory[i];
            if (IsDragging && item == DraggedItem) continue;

            Vector3 pos = item.Renderer.transform.position;
            if (pos.y < OFF_SCREEN_MIN_Y || pos.x < offScreenMinX || pos.x > offScreenMaxX)
            {
                Game.Instance.DropItemIntoCart(item);
            }
        }
    }

    #endregion
}
