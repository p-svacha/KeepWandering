using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterStep
{
    public string Text;

    public List<EncounterOption> Options;

    public EncounterStep(string text, List<EncounterOption> options = null)
    {
        Text = text;
        Options = options ?? new List<EncounterOption>();
    }

    /// <summary>
    /// If this is the final step of the event, meaning that there are no more interaction options.
    /// </summary>
    public bool IsFinalStep => Options.Count == 0;

    /// <summary>
    /// Force highlight all items that can be slotted into the options of this encounter step. Required items are highlighted in green, optional items are highlighted in yellow.
    /// </summary>
    public void HighlightSlottableItems()
    {

        foreach (EncounterOption option in Options)
        {
            foreach (ItemSlot slot in option.ItemSlots)
            {
                foreach (Item item in slot.GetSlottableItems())
                {
                    Color highlightColor = slot.IsRequired ? Color.green : Color.yellow;
                    item.Renderer.Highlight(Color.green, forced: true);
                }
            }
        }
    }
}
