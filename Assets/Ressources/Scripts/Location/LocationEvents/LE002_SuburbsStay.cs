using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE002_SuburbsStay : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation == Location.Suburbs) return 15;
        else return 0;
    }

    public LE002_SuburbsStay(Game game) : base(
        game,
        LocationEventType.LE002_SuburbsStay,
        "You keep walking through the suburbs.",
        new Dictionary<Location, string>()
        {
            {Location.Suburbs, "Okay" },
        })
    { }
}
