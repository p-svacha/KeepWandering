using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Quest
{
    // Base
    public QuestDef QuestDef;
    public string Text { get; private set; }

    // Location based missions
    public WorldMapTile Location { get; private set; }
    public Area Area { get; private set; }

    public bool IsLocationBased => Location != null || Area != null;

    public Quest(QuestDef questDef, string text, WorldMapTile location = null, Area area = null)
    {
        QuestDef = questDef;
        Text = text;
        Location = location;
        Area = area;

        if (Area != null && Location != null) throw new System.Exception("Mission cannot have both an area and a location.");
    }

    /// <summary>
    /// Sets the quest location. Used by auto-placement when the location is determined after quest creation.
    /// </summary>
    public void SetLocation(WorldMapTile tile) => Location = tile;

    /// <summary>
    /// Replaces {0} in the quest text with the given location text (e.g. tile coordinates).
    /// </summary>
    public void FormatText(string locationText)
    {
        if (Text.Contains("{0}"))
            Text = string.Format(Text, locationText);
    }
}
