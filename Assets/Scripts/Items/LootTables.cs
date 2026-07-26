using UnityEngine;

/// <summary>
/// Contains some general loot tables that can be used in different encounters and be referenced in other loot tables. These are not biome-specific, but they can be modified by the biome loot tables when used in encounters.
/// </summary>
public static class LootTables
{
    public static LootTable Food { get; private set; }
    public static LootTable Drinks { get; private set; }
    public static LootTable Medical { get; private set; }
    public static LootTable Plants { get; private set; }
    public static LootTable Tools { get; private set; }
    public static LootTable Weapons { get; private set; }
    public static LootTable Trash { get; private set; }
    public static LootTable TrapLoot { get; private set; }
    public static LootTable Bandit { get; private set; }
    public static LootTable Civilian { get; private set; }
    public static LootTable Building { get; private set; }


    public static void Init()
    {
        // Base tables first, since composite tables below reference them as sub-tables
        Food = new LootTable
        {
            { ItemDefOf.Beans, Rarity.Common },
            { ItemDefOf.NutSnack, Rarity.Occasional },
            { ItemDefOf.MeatRaw, Rarity.Rare },
        };

        Drinks = new LootTable
        {
            { ItemDefOf.WaterBottle, Rarity.Common },
        };

        Medical = new LootTable
        {
            { ItemDefOf.Bandage, Rarity.Common },
            { ItemDefOf.Antibiotics, Rarity.Occasional },
            { ItemDefOf.MedicalKit, Rarity.Rare },
            { ItemDefOf.Antidote, Rarity.VeryRare },
        };

        Plants = new LootTable
        {
            { ItemDefOf.Berries, Rarity.Common },
            { ItemDefOf.MedicinalHerbs, Rarity.Occasional },
        };

        Tools = new LootTable
        {
            { ItemDefOf.Crowbar, Rarity.Occasional },
            { ItemDefOf.Rope, Rarity.Rare },
            { ItemDefOf.Knife, Rarity.Rare },
            { ItemDefOf.Lockpick, Rarity.VeryRare },
            { ItemDefOf.Shovel, Rarity.VeryRare },
            { ItemDefOf.Trap, Rarity.ExtremelyRare },
            { ItemDefOf.FenceCutter, Rarity.ExtremelyRare },
        };

        Weapons = new LootTable
        {
            { ItemDefOf.Knife, Rarity.Common },
            { ItemDefOf.Crowbar, Rarity.Rare },
            { ItemDefOf.Shovel, Rarity.VeryRare },
        };

        Trash = new LootTable
        {
            { ItemDefOf.Bone, Rarity.Common },
            { ItemDefOf.Coin, Rarity.Rare },
        };

        TrapLoot = new LootTable
        {
            { ItemDefOf.MeatRaw, Rarity.VeryCommon },
            { ItemDefOf.Bone, Rarity.Common },
        };

        // Composite tables, built from the ones above
        Bandit = new LootTable
        {
            { Weapons, Rarity.Occasional },
            { Tools, Rarity.Rare },
            { Food, Rarity.Rare },
            { Drinks, Rarity.Rare },
            { Medical, Rarity.VeryRare },
        };

        Civilian = new LootTable
        {
            { Food, Rarity.Occasional },
            { Drinks, Rarity.Rare },
            { ItemDefOf.Coin, Rarity.Rare },
            { Tools, Rarity.VeryRare },
            { Medical, Rarity.Rare },
            { Trash, Rarity.VeryRare },
            { Plants, Rarity.ExtremelyRare },
        };

        Building = new LootTable
        {
            { Tools, Rarity.Occasional },
            { Medical, Rarity.Rare },
            { ItemDefOf.Coin, Rarity.Rare },
            { Food, Rarity.VeryRare },
            { Drinks, Rarity.VeryRare },
            { Plants, Rarity.ExtremelyRare },
        };
    }
}
