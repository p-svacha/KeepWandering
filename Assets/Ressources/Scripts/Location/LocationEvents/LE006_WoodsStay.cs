using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE006_WoodsStay : LocationEvent
{
    public static float GetProbability(Game game)
    {
        if (game.CurrentLocation.Type == LocationType.Woods) return 10;
        else return 0;
    }

    public LE006_WoodsStay(Game game) : base(
        game,
        LocationEventType.LE006_WoodsStay,
        "You keep walking through the woods.",
        new Dictionary<Location, string>()
        {
            {ResourceManager.Singleton.LOC_Woods, "Okay" },
        })
    { }
}
