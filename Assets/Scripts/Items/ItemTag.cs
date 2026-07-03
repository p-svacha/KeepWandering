using System.Collections.Generic;
using UnityEngine;

public class ItemTagDef : Def
{
    public override string DefTypeLabel => "Item Tag";

    public ItemTagDef(string defName) : base(defName) { }
}

public static class ItemTagDefs
{
    public static List<ItemTagDef> Defs => new List<ItemTagDef>()
    {
        new ItemTagDef("BuildingMaterial")
        {
            Label = "Building Material",
            Description = "Items with this tag can be used to construct buildings and structures."
        },
        new ItemTagDef("Cutting")
        {
            Label = "Cutting",
            Description = "Items with this tag can be used to cut or slice objects."
        },
        new ItemTagDef("Digging")
        {
            Label = "Digging",
            Description = "Items with this tag can be used to dig or excavate soil and other materials."
        },
        new ItemTagDef("DogToy")
        {
            Label = "Dog Toy",
            Description = "Items with this tag can be used to entertain and play with dogs."
        },
        new ItemTagDef("Lockpicking")
        {
            Label = "Lockpicking",
            Description = "Items with this tag can be used to pick locks and open secured containers."
        },
        new ItemTagDef("PryingTool")
        {
            Label = "Prying Tool",
            Description = "Items with this tag can be used to pry open objects or containers."
        },
        new ItemTagDef("Scavenging")
        {
            Label = "Scavenging",
            Description = "Items with this tag can be used for scavenging resources from the environment."
        },
        new ItemTagDef("Weapon")
        {
            Label = "Weapon",
            Description = "Items with this tag can be used as weapons."
        },
    };
}

[DefOf]
public static class ItemTagDefOf
{
    public static ItemTagDef BuildingMaterial;
    public static ItemTagDef Cutting;
    public static ItemTagDef Digging;
    public static ItemTagDef DogToy;
    public static ItemTagDef Lockpicking;
    public static ItemTagDef PryingTool;
    public static ItemTagDef Scavenging;
    public static ItemTagDef Weapon;
}
