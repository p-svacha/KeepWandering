using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains collections of commonly used ItemDefs.
/// </summary>
public static class ItemSet
{
    private static List<ItemDef> Items => DefDatabase<ItemDef>.AllDefs;

    public static List<ItemDef> MedicalItems => Items.Where(item => item.HasMedicalProperties).ToList();
    public static List<ItemDef> ConsumableItems => Items.Where(item => item.IsConsumable).ToList();
}
