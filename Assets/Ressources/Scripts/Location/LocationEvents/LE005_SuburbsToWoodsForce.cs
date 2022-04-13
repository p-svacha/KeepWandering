using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE005_SuburbsToWoodsForce : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation.Type == LocationType.Suburbs) return 5;
        else return 0;
    }

    public LE005_SuburbsToWoodsForce(Game game) : base(
        game,
        LocationEventType.LE005_SuburbsToWoodsForce,
        "You've reached the end of the suburbs and walk into the woods.",
        new Dictionary<Location, string>()
        {
            {ResourceManager.Singleton.LOC_Woods, "Okay" },
        })
    { }
}
