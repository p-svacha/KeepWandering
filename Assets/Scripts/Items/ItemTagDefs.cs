using System.Collections.Generic;
using UnityEngine;

public static class ItemTagDefs
{
    public static List<ItemTagDef> Defs => new List<ItemTagDef>()
    {
        new ItemTagDef()
        {
            DefName = "Food",
            Label = "Food",
            Description = "Items that can be eaten to provide nutrition and/or hydration.",
        },
        new ItemTagDef()
        {
            DefName = "Drink",
            Label = "Drink",
            Description = "Items that can be drunk to provide hydration.",
        },
        new ItemTagDef()
        {
            DefName = "Medical",
            Label = "Medical",
            Description = "Items that can be used for medical purposes, such as tending wounds or healing infections.",
        },
        new ItemTagDef()
        {
            DefName = "Weapon",
            Label = "Weapon",
            Description = "Items that can be used as weapons to defend against threats.",
        },
        new ItemTagDef()
        {
            DefName = "Tool",
            Label = "Tool",
            Description = "General tag for all kinds of items commonly used as tools of some kind.",
        },
        new ItemTagDef()
        {
            DefName = "ForDogs",
            Label = "For Dogs",
            Description = "Items that are specifically for dogs, such as dog food or dog toys.",
        },
    };
}
