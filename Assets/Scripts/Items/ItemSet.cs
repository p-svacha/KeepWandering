using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// A simple container to wrap a set of items and a name of the collection.
/// </summary>
public class ItemSet
{
    public string Name { get; private set; }
    public List<ItemDef> Items { get; private set; }

    public ItemSet(string name, List<ItemDef> items)
    {
        Name = name;
        Items = items;
    }
}

public static class ItemSets
{
    private static List<ItemDef> Items => DefDatabase<ItemDef>.AllDefs;

    public static ItemSet MedicalItems => new ItemSet("Medical", Items.Where(item => item.HasMedicalProperties).ToList());
    public static ItemSet IngestibleItems => new ItemSet("Consumable", Items.Where(item => item.IsConsumable && item.ConsumptionProperties.ConsumptionType != ConsumptionTypeDefOf.Smoke).ToList());
    public static ItemSet SmokeableItems => new ItemSet("Smokeable", Items.Where(item => item.IsConsumable && item.ConsumptionProperties.ConsumptionType == ConsumptionTypeDefOf.Smoke).ToList());
    public static ItemSet CookableItems => new ItemSet("Cookable", Items.Where(item => item.IsCookable).ToList());
}
