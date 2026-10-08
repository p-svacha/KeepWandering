using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum QuestState
{
    Active,
    Completed,
    Failed
}


public class Quest
{
    // Base
    public QuestDef Def;
    public string Text { get; private set; }
    public QuestState State { get; private set; }

    // Location
    public WorldMapTile Location { get; private set; }
    public Area Area { get; private set; }
    public bool HasRelatedLocation => Location != null || Area != null;

    // Item
    public ItemDef RelatedItem { get; private set; }
    public bool HasRelatedItem => RelatedItem != null;

    public Quest(QuestDef questDef, string text, WorldMapTile location = null, Area area = null, ItemDef relatedItem = null)
    {
        Def = questDef;
        Text = text;
        RelatedItem = relatedItem;
        Location = location;
        Area = area;

        State = QuestState.Active;
    }

    public void SetState(QuestState newState)
    {
        State = newState;
    }

    /// <summary>
    /// Sets the quest location. Used by auto-placement when the location is determined after quest creation.
    /// </summary>
    public void SetLocation(WorldMapTile tile) => Location = tile;

    /// <summary>
    /// Replaces {LOC} in the quest text with the given location text (e.g. tile coordinates) and {ITEM} with the related item name.
    /// </summary>
    public void FormatText(string locationText)
    {
        if (Text.Contains("{LOC}"))
            Text = Text.Replace("{LOC}", locationText);
        if (Text.Contains("{ITEM}") && RelatedItem != null)
            Text = Text.Replace("{ITEM}", RelatedItem.LabelCapWord);
    }
}
