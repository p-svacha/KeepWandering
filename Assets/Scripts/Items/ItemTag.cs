using System.Collections.Generic;
using UnityEngine;

public class ItemTagDef : Def
{
    public override string DefTypeLabel => "Item Tag";

    public ItemTagDef(string defName) : base(defName) { }

    // Label should be written in a way, so that "item can be used as {label}" makes sense.
}

public static class ItemTagDefs
{
    public static List<ItemTagDef> Defs => new List<ItemTagDef>()
    {
        new ItemTagDef("Climbing")
        {
            Label = "Climbing Tool",
            Description = "Items with this tag can be used for climbing."
        },
        new ItemTagDef("Cutting")
        {
            Label = "Cutting Tool",
            Description = "Items with this tag can be used to cut or slice objects."
        },
        new ItemTagDef("Digging")
        {
            Label = "Digging Tool",
            Description = "Items with this tag can be used to dig or excavate soil and other materials."
        },
        new ItemTagDef("DogToy")
        {
            Label = "Dog Toy",
            Description = "Items with this tag can be used to entertain and play with dogs."
        },
        new ItemTagDef("FireStarter")
        {
            Label = "Fire Starter",
            Description = "Items with this tag can be used to ignite fires."
        },
        new ItemTagDef("Fuel")
        {
            Label = "Fuel",
            Description = "Items with this tag can be used as fuel for fires or engines."
        },
        new ItemTagDef("InfectionTreatment")
        {
            Label = "Infection Treatment",
            Description = "Items with this tag can be used to treat infections."
        },
        new ItemTagDef("LightSource")
        {
            Label = "Light Source",
            Description = "Items with this tag can be used to provide light."
        },
        new ItemTagDef("Lockpicking")
        {
            Label = "Lockpicking Tool",
            Description = "Items with this tag can be used to pick locks and open secured containers."
        },
        new ItemTagDef("PoisonTreatment")
        {
            Label = "Poison Treatment",
            Description = "Items with this tag can be used to treat poisoning."
        },
        new ItemTagDef("PryingTool")
        {
            Label = "Prying Tool",
            Description = "Items with this tag can be used to pry open objects or containers."
        },
        new ItemTagDef("Weapon")
        {
            Label = "Weapon",
            Description = "Items with this tag can be used as weapons."
        },
        new ItemTagDef("WoundBandaging")
        {
            Label = "Wound Bandage",
            Description = "Items with this tag can be used to bandage wounds."
        },
    };
}

[DefOf]
public static class ItemTagDefOf
{
    public static ItemTagDef Climbing;
    public static ItemTagDef Cutting;
    public static ItemTagDef Digging;
    public static ItemTagDef DogToy;
    public static ItemTagDef FireStarter;
    public static ItemTagDef Fuel;
    public static ItemTagDef InfectionTreatment;
    public static ItemTagDef LightSource;
    public static ItemTagDef Lockpicking;
    public static ItemTagDef PoisonTreatment;
    public static ItemTagDef PryingTool;
    public static ItemTagDef Weapon;
    public static ItemTagDef WoundBandaging;
}
