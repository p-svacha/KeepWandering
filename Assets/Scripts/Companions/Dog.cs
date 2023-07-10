using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Companion
{
    public override string Name => "Dog";
    private const float FindItemChance = 0.3f;
    private static LootTable FindItemTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.NutSnack, 10),
        new(ItemType.Bone, 10),
        new(ItemType.WaterBottle, 5),
        new(ItemType.Bandage, 5),
        new(ItemType.Coin, 1)
    );

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        if(Random.value < FindItemChance)
        {
            Item foundItem = GetLocationLootTable(FindItemTable).AddItemToInventory();

            morningReport.NightEvents.Add("Your dog found a " + foundItem.Name + ".");
            morningReport.AddedItems.Add(foundItem);
        }
    }

    protected override void OnInit() { }
    public override void UpdateStatusEffects() { }
}
