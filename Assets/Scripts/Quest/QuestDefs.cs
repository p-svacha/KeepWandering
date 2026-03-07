using System.Collections.Generic;
using UnityEngine;

public static class QuestDefs
{
    public static List<QuestDef> Defs => new List<QuestDef>()
    {
        new QuestDef()
        {
            DefName = "FindR",
            Description = "R lives on random tile in a specific city.",
        },
        new QuestDef()
        {
            DefName = "GoToUnpoweredFence",
            Description = "There is a border tile where the electric fence is not unpowered. With a fence cutter it is possible to cut through the fence and get to the other side.",
        }
    };
}
