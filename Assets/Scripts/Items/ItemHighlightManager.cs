using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemHighlightManager
{
    private static readonly KeyCode HIGHLIGHT_ALL_KEY = KeyCode.LeftAlt;
    private static List<Item> CurrentlyHighlighted = new List<Item>();

    public static void Update()
    {
        if (Game.Instance == null || Game.Instance.CurrentEncounterStep == null) return;

        List<Item> itemsToHighlight = ComputeItemsToHighlight();

        foreach (Item item in CurrentlyHighlighted)
        {
            if (!itemsToHighlight.Contains(item)) item.Renderer.Unhighlight();
        }
        foreach (Item item in itemsToHighlight)
        {
            item.Renderer.Highlight(ResourceManager.ItemHighlight_UsableColor);
        }

        CurrentlyHighlighted = itemsToHighlight;
    }

    private static List<Item> ComputeItemsToHighlight()
    {
        List<Item> items;
        EncounterStep step = Game.Instance.CurrentEncounterStep;

        // 1. Alt held - everything slottable anywhere in the current step
        if (Input.GetKey(HIGHLIGHT_ALL_KEY))
        {
            items = GetSlottableItemsForOptions(step.Options);
        }
        // 2. A specific item is hovered
        else if (Game.Instance.CurrentHoverItem != null)
        {
            items = new List<Item> { Game.Instance.CurrentHoverItem };
        }
        // 3. A specific item slot is hovered
        else if (ItemDragDropManager.HoveredItemSlot != null)
        {
            items = ItemDragDropManager.HoveredItemSlot.ItemSlot.GetSlottableItems();
        }
        // 4. An encounter option is hovered
        else if (ItemDragDropManager.HoveredOptionDisplay != null)
        {
            items = GetSlottableItemsForOptions(new List<EncounterOption> { ItemDragDropManager.HoveredOptionDisplay.Option });
        }
        // 5. A sprite-bound indicator is hovered
        else if (SpriteOptionInteractionManager.HoveredIndicator != null)
        {
            items = GetSlottableItemsForOptions(SpriteOptionInteractionManager.HoveredIndicator.Options);
        }
        else
        {
            items = new List<Item>();
        }

        // Always keep the actively dragged item highlighted
        if (ItemDragDropManager.IsDragging && ItemDragDropManager.DraggedItem != null && !items.Contains(ItemDragDropManager.DraggedItem))
        {
            items.Add(ItemDragDropManager.DraggedItem);
        }

        return items;
    }

    private static List<Item> GetSlottableItemsForOptions(List<EncounterOption> options)
    {
        HashSet<Item> items = new HashSet<Item>();
        foreach (EncounterOption option in options)
            foreach (ItemSlot slot in option.ItemSlots)
                foreach (Item item in slot.GetSlottableItems())
                    items.Add(item);
        return items.ToList();
    }

    public static void ClearAll()
    {
        foreach (Item item in CurrentlyHighlighted) item.Renderer.Unhighlight();
        CurrentlyHighlighted.Clear();
    }
}