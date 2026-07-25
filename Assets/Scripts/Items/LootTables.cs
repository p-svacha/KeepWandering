using UnityEngine;

/// <summary>
/// Contains some general loot tables that can be used in different encounters and be referenced in other loot tables. These are not biome-specific, but they can be modified by the biome loot tables when used in encounters.
/// </summary>
public static class LootTables
{
    public static LootTable Food => new LootTable
    {
        { ItemDefOf.Beans, 10 },
        { ItemDefOf.NutSnack, 8 },
        { ItemDefOf.MeatRaw, 3 },
    };

    public static LootTable Drinks => new LootTable
    {
        { ItemDefOf.WaterBottle, 10 },
    };

    public static LootTable Medical => new LootTable
    {
        { ItemDefOf.Bandage, 10 },
        { ItemDefOf.Antibiotics, 6 },
        { ItemDefOf.MedicalKit, 3 },
        { ItemDefOf.Antidote, 2 },
    };

    public static LootTable Plants => new LootTable
    {
        { ItemDefOf.Berries, 10 },
        { ItemDefOf.MedicinalHerbs, 8 },
    };

    public static LootTable Tools => new LootTable
    {
        { ItemDefOf.Crowbar, 7 },
        { ItemDefOf.Rope, 5 },
        { ItemDefOf.Knife, 5 },
        { ItemDefOf.Lockpick, 3 },
        { ItemDefOf.Shovel, 3 },
        { ItemDefOf.Trap, 1 },
        { ItemDefOf.FenceCutter, 0.1f },
    };

    public static LootTable Weapons => new LootTable
    {
        { ItemDefOf.Knife, 10 },
        { ItemDefOf.Crowbar, 3 },
        { ItemDefOf.Shovel, 1 },
    };

    public static LootTable Trash => new LootTable
    {
        { ItemDefOf.Bone, 5 },
        { ItemDefOf.Coin, 2 }
    };

    public static LootTable TrapLoot => new LootTable
    {
        { ItemDefOf.MeatRaw, 40 },
        { ItemDefOf.Bone, 25 },
    };

    public static LootTable Bandit => new LootTable
    {
        { Weapons, 10 },
        { Tools, 8 },
        { Food, 6 },
        { Drinks, 6 },
        { Medical, 4 },
    };

    public static LootTable Civilian => new LootTable
    {
        { Food, 12 },
        { Drinks, 10 },
        { ItemDefOf.Coin, 10 },
        { Tools, 5 },
        { Medical, 8 },
        { Trash, 5 },
        { Plants, 2 },
    };

    public static LootTable Building => new LootTable
    {
        { Tools, 10 },
        { Medical, 8 },
        { ItemDefOf.Coin, 8 },
        { Food, 5 },
        { Drinks, 5 },
        { Plants, 2 },
    };
}
