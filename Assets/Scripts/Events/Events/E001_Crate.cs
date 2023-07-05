using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E001_Crate : Event
{
    // Static
    private static Dictionary<ItemType, float> ItemTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 10},
        { ItemType.WaterBottle, 10},
        { ItemType.Bandage, 5},
        { ItemType.Antibiotics, 5},
        { ItemType.Bone, 3},
        { ItemType.Knife, 3},
        { ItemType.NutSnack, 10},
    };
    private const float CUT_CHANCE = 0.1f;

    // Instance
    private Item CrateItem;

    public E001_Crate(Game game) : base(game) { }
    public override Event GetEventInstance => new E001_Crate(Game);

    public override float GetEventProbability()
    {
        return 10;
    }
    public override void OnEventStart()
    {
        // Sprites
        ResourceManager.Singleton.E001_Crate.SetActive(true);

        // Crate item
        ItemType itemType = HelperFunctions.GetWeightedRandomElement<ItemType>(ItemTable);
        CrateItem = Game.GetItemInstance(itemType);
        CrateItem.transform.position = new Vector3(6, 0f, 0f);
        CrateItem.transform.rotation = Quaternion.Euler(0f, 0f, -30f);
    }
    public override EventStep GetInitialStep()
    {
        // Dialogue Options
        List<EventOption> options = new List<EventOption>();
        options.Add(new EventOption("Take the " + CrateItem.Name + ".", TakeItem)); // Take item
        options.Add(new EventOption("Don't take the " + CrateItem.Name + ".", DontTakeItem)); // Don't take item

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();      

        return new EventStep("You stumble upon a crate that looks to have a " + CrateItem.Name + " inside.", null, null, options, itemOptions);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E001_Crate.SetActive(false);
        if (!CrateItem.IsPlayerOwned) GameObject.Destroy(CrateItem.gameObject);
    }

    private EventStep TakeItem()
    {
        string text = "You reach into the crate and take out the " + CrateItem.Name + ".";
        Game.AddItemToInventory(CrateItem);
        if(Random.value < CUT_CHANCE)
        {
            Game.AddCutWound();
            text += " Upon taking out your hand you scratch yourself on a loose nail.";
        }
        return new EventStep(text, new List<Item>() { CrateItem }, null, null, null);
    }
    private EventStep DontTakeItem()
    {
        return new EventStep("You didn't take the " + CrateItem.Name + ".", null, null, null, null);
    }


}
