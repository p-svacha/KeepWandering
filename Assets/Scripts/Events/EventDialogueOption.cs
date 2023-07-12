using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventDialogueOption
{
    /// <summary>
    /// Text that gets displayed on the button of the dialogue option.
    /// </summary>
    public string Text { get; private set; }
    /// <summary>
    /// Action that gets executed when chosing this option. Must return the next EventStep in the Event.
    /// </summary>
    public Func<EventStep> Action { get; private set; }

    /// <summary>
    /// Stats that are affecting the outcome of the dialogue option. These will get highlighted when hovering the option button.
    /// </summary>
    public List<StatId> AffectingStats;

    public EventDialogueOption(string text, Func<EventStep> action, params StatId[] affectingStats)
    {
        Text = text;
        Action = action;
        AffectingStats = affectingStats == null ? new List<StatId>() : affectingStats.ToList();
    }

    public void OnHoverStart()
    {
        foreach (StatId stat in AffectingStats) Game.Singleton.UI.HightlightStat(stat);
    }

    public void OnHoverEnd()
    {
        foreach (StatId stat in AffectingStats) Game.Singleton.UI.UnhighlightStat(stat);
    }
}
