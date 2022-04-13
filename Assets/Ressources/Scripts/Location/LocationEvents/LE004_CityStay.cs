using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE004_CityStay : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation.Type == LocationType.City) return 15;
        else return 0;
    }

    public LE004_CityStay(Game game) : base(
        game,
        LocationEventType.LE004_CityStay,
        "You keep walking through the city.",
        new Dictionary<Location, string>()
        {
            {ResourceManager.Singleton.LOC_City, "Okay" },
        })
    { }
}