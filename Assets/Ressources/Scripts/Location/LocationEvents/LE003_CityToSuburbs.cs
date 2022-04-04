using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE003_CityToSuburbs : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation == Location.City) return 10;
        else return 0;
    }

    public LE003_CityToSuburbs(Game game) : base(
        game,
        LocationEventType.LE003_CityToSuburbs,
        "You've reached a junction to the suburbs.",
        new Dictionary<Location, string>()
        {
            {Location.City, "Stay in the city." },
            {Location.Suburbs, "Go to the suburbs." }
        })
    { }
}
