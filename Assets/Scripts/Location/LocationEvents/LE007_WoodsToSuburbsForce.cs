using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE007_WoodsToSuburbsForce : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation.Type == LocationType.Woods) return 5;
        else return 0;
    }

    public LE007_WoodsToSuburbsForce(Game game) : base(
        game,
        LocationEventType.LE007_WoodsToSuburbsForce,
        "You've reached the end of the woods and walk into the suburbs.",
        new Dictionary<Location, string>()
        {
            {ResourceManager.Singleton.LOC_Suburbs, "Okay" },
        })
    { }
}
