using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventItemOption
{
    public ItemType RequiredItemType;
    public string Text;
    public Func<Item, EventStep> Action;

    /// <summary>
    /// Stats that are affecting the outcome of the dialogue option. These will get highlighted when hovering the option button.
    /// </summary>
    public List<StatId> AffectingStats;

    public EventItemOption(ItemType requiredItem, string text, Func<Item, EventStep> action, params StatId[] affectingStats)
    {
        RequiredItemType = requiredItem;
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
