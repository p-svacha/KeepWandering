using System.Collections.Generic;
using UnityEngine;

public static class BiomeDefs
{
    public static List<BiomeDef> Defs => new List<BiomeDef>()
    {
        new BiomeDef()
        {
            DefName = "Woods",
            Label = "woods",
            Description = "A place of many wild animals and plants.",
        },

        new BiomeDef()
        {
            DefName = "Farmland",
            Label = "farmland",
            Description = "A place of many crops and farm animals.",
        },

        new BiomeDef()
        {
            DefName = "City",
            Label = "city",
            Description = "A bustling urban area with many buildings and people.",
        },

        new BiomeDef()
        {
            DefName = "Lake",
            Label = "lake",
            Description = "A serene body of water surrounded by nature.",
            IsPassable = false,
        }
    };
}
