using System.Collections.Generic;
using UnityEngine;

public static class ItemTagDefs
{
    public static List<ItemTagDef> Defs => new List<ItemTagDef>()
    {
        new ItemTagDef()
        {
            DefName = "BuildingMaterial",
            Label = "Building Material",
            Description = "Items that can be used as building materials, such as wood, metal, stone etc.",
        },
        new ItemTagDef()
        {
            DefName = "Drink",
            Label = "Drink",
            Description = "Items that can be drunk to provide hydration.",
        },
        new ItemTagDef()
        {
            DefName = "Food",
            Label = "Food",
            Description = "Items that can be eaten to provide nutrition and/or hydration.",
        },
        new ItemTagDef()
        {
            DefName = "ForDogs",
            Label = "For Dogs",
            Description = "Items that are specifically for dogs, such as dog food or dog toys.",
        },
        new ItemTagDef()
        {
            DefName = "Medical",
            Label = "Medical",
            Description = "Items that can be used for medical purposes, such as tending wounds or healing infections.",
        },
        new ItemTagDef()
        {
            DefName = "Plant",
            Label = "Plant",
            Description = "Items that are plants or plant-based, such as herbs, fruits, vegetables etc.",
        },
        new ItemTagDef()
        {
            DefName = "Scavenging",
            Label = "Scavenging",
            Description = "Items that useful for scavenging.",
        },
        new ItemTagDef()
        {
            DefName = "Tool",
            Label = "Tool",
            Description = "General tag for all kinds of items commonly used as tools of some kind.",
        },
        new ItemTagDef()
        {
            DefName = "Trash",
            Label = "Trash",
            Description = "Items that can be generally found among trash. They're usually not great, still have their uses.",
        },
        new ItemTagDef()
        {
            DefName = "Weapon",
            Label = "Weapon",
            Description = "Items that can be used as weapons to defend against threats.",
        },
    };
}
