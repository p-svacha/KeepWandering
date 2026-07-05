using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterStep
{
    /// <summary>
    /// The text displayed to the player at this step of the encounter.
    /// </summary>
    public string Text { get; private set; }

    /// <summary>
    /// The list of options available to the player at this step of the encounter. If this list is empty, it indicates that this is the final step of the encounter.
    /// </summary>
    public List<EncounterOption> Options { get; private set; }

    /// <summary>
    /// Flag indicating if this is the final step of the encounter. If true, the option is available to advance to the next time of day.
    /// </summary>
    public bool IsFinalStep { get; private set; }

    public EncounterStep(string text, List<EncounterOption> options, bool isFinalStep)
    {
        Text = text;
        Options = options;
        IsFinalStep = isFinalStep;
    }

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
