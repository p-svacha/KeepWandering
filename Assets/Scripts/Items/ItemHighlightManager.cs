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
        EncounterStep step = Game.Instance.CurrentEncounterStep;

        // 1. Alt held - everything slottable anywhere in the current step
        if (Input.GetKey(HIGHLIGHT_ALL_KEY))
        {
            return GetSlottableItemsForOptions(step.Options);
        }

        // 2. A specific item is hovered
        if (Game.Instance.CurrentHoverItem != null)
        {
            return new List<Item> { Game.Instance.CurrentHoverItem };
        }

        // 3. A specific item slot is hovered
        if (ItemDragDropManager.HoveredItemSlot != null)
        {
            return ItemDragDropManager.HoveredItemSlot.ItemSlot.GetSlottableItems();
        }

        // 4. An encounter option is hovered
        if (ItemDragDropManager.HoveredOptionDisplay != null)
        {
            return GetSlottableItemsForOptions(new List<EncounterOption> { ItemDragDropManager.HoveredOptionDisplay.Option });
        }

        // 5. A sprite-bound indicator is hovered
        SpriteOptionIndicator hoveredIndicator = SpriteOptionInteractionManager.HoveredIndicator;
        if (hoveredIndicator != null)
        {
            return GetSlottableItemsForOptions(hoveredIndicator.Options);
        }

        return new List<Item>();
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