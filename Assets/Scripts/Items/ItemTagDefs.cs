using System.Collections.Generic;
using UnityEngine;

public static class ItemTagDefs
{
    public static List<ItemTagDef> Defs => new List<ItemTagDef>()
    {
        new ItemTagDef("BuildingMaterial"),
        new ItemTagDef("Cutting"),
        new ItemTagDef("Digging"),
        new ItemTagDef("Drink"),
        new ItemTagDef("Food"),
        new ItemTagDef("ForDogs"),
        new ItemTagDef("Lockpicking"),
        new ItemTagDef("Medical"),
        new ItemTagDef("Plant"),
        new ItemTagDef("Scavenging"),
        new ItemTagDef("Tool"),
        new ItemTagDef("Trash"),
        new ItemTagDef("Weapon"),
    };
}
