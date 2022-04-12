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

    public override List<string> OnEndDay(Game game)
    {
        List<string> eventReport = new List<string>();
        if(Random.value < FindItemChance)
        {
            ItemType foundItemType = HelperFunctions.GetWeightedRandomElement(FindItemTable);
            Item foundItem = game.GetItemInstance(foundItemType);
            game.AddItemToInventory(foundItem);
            eventReport.Add("Your dog found a " + foundItem.Name + ".");
        }
        return eventReport;
    }

    protected override void OnInit() { }
    public override void UpdateStatusEffects() { }
}
