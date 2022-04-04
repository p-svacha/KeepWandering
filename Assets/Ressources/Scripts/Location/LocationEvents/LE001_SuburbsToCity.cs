using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE001_SuburbsToCity : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation == Location.Suburbs) return 10;
        else return 0;
    }

    public LE001_SuburbsToCity(Game game) : base(
        game,
        LocationEventType.LE001_SuburbsToCity,
        "You've reached a junction to the main road.",
        new Dictionary<Location, string>()
        {
            {Location.Suburbs, "Stay in the suburbs." },
            {Location.City, "Go to the city." }
        })
    { }
}
