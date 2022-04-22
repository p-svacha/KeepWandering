using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Companion
{
    private const float FindItemChance = 0.3f;
    private static Dictionary<ItemType, float> FindItemTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 10},
        { ItemType.NutSnack, 10},
        { ItemType.Bone, 10},
        { ItemType.WaterBottle, 5},
        { ItemType.Bandage, 5},
    };

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        if(Random.value < FindItemChance)
        {
            ItemType foundItemType = HelperFunctions.GetWeightedRandomElement(FindItemTable);
            Item foundItem = game.GetItemInstance(foundItemType);
            game.AddItemToInventory(foundItem);
            morningReport.NightEvents.Add("Your dog found a " + foundItem.Name + ".");
            morningReport.AddedItems.Add(foundItem);
        }
    }

    protected override void OnInit() { }
    public override void UpdateStatusEffects() { }
}
