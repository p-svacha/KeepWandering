using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE003_CityToSuburbs : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation.Type == LocationType.City) return 10;
        else return 0;
    }

    public LE003_CityToSuburbs(Game game) : base(
        game,
        LocationEventType.LE003_CityToSuburbs,
        "You've reached a junction to the suburbs.",
        new Dictionary<Location, string>()
        {
            {ResourceManager.Singleton.LOC_City, "Stay in the city." },
            {ResourceManager.Singleton.LOC_Suburbs, "Go to the suburbs." }
        })
    { }
}
