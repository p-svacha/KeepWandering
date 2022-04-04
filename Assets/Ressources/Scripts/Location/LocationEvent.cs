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
            options.Add(new EventOption(target.Value, () => ChoseNextLocation(game, target.Key), null));
        }
        EventStep = new EventStep(text, options, null);
    }

    private static void ChoseNextLocation(Game game, Location location)
    {
        game.SetNextDayLocation(location);
        game.EndDay();
    }
}
