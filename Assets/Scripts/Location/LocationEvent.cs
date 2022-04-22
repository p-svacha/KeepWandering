using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LocationEvent
{
    public LocationEventType Type;
    public EventStep EventStep;

    public LocationEvent(Game game, LocationEventType type, string text, Dictionary<Location, string> targetLocations)
    {
        Type = type;
        List<EventOption> options = new List<EventOption>();
        foreach(KeyValuePair<Location, string> target in targetLocations)
        {
            options.Add(new EventOption(target.Value, (game) => ChoseNextLocation(game, target.Key)));
        }
        EventStep = new EventStep(text, null, null, options, null);
    }

    private static EventStep ChoseNextLocation(Game game, Location location)
    {
        game.SetNextDayLocation(location);
        game.EndDay();
        return null;
    }
}
