using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Mission
{
    // Base
    public MissionId Id;
    public string Text;

    // Location based missions
    public WorldMapTile Location { get; private set; }
    public TileBase MapMarker { get; private set; }
    /// <summary>
    /// Id of event that gets triggered when entering the location of the mission.
    /// </summary>
    public int EventId { get; private set; }
    public bool IsLocationBased => Location != null;

    public Mission(MissionId id, string text, WorldMapTile location = null, int eventId = -1, TileBase mapMarker = null)
    {
        Id = id;
        Text = text;
        Location = location;
        EventId = eventId;
        MapMarker = mapMarker;
    }
}
