using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E001_Crate : Event
{
    private static Dictionary<ItemType, float> ItemTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 10},
        { ItemType.WaterBottle, 10},
        { ItemType.Bandage, 10},
        { ItemType.Antibiotics, 10},
        { ItemType.Bone, 10},
        { ItemType.Knife, 10},
        { ItemType.NutSnack, 10},
    };

    public static float GetProbability(Game game)
    {
        return 10;
    }

    public static E001_Crate GetEventInstance(Game game)
    {
        // Sprite
        ResourceManager.Singleton.E001_Crate.SetActive(true);

        ItemType itemType = HelperFunctions.GetWeightedRandomElement<ItemType>(ItemTable, debug: true);
        Item item = game.GetItemInstance(itemType);
        item.transform.position = new Vector3(6, 0f, 0f);
        item.transform.rotation = Quaternion.Euler(0f, 0f, -30f);

        List<EventOption> options = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Take item
        options.Add(new EventOption("Take the " + item.Name + ".", (game) => TakeItem(game, item)));

        // Dialogue Option - Don't take item
        options.Add(new EventOption("Don't take the " + item.Name + ".", (game) => DontTakeItem(game, item)));

        EventStep initialStep = new EventStep("You stumble upon a crate that looks to have a " + item.Name + " inside.", options, itemOptions);
        return new E001_Crate(initialStep, item);
    }

    private static EventStep TakeItem(Game game, Item item)
    {
        game.AddItemToInventory(item);
        return new EventStep("You took the " + item.Name + ".", null, null);
    }

    private static EventStep DontTakeItem(Game game, Item item)
    {
        return new EventStep("You didn't take the " + item.Name + ".", null, null);
    }

    public E001_Crate(EventStep initialStep, Item item) : base(
        EventType.E001_Crate, 
        initialStep, 
        new List<Item>() { item },
        new List<GameObject>() { ResourceManager.Singleton.E001_Crate },
        itemActionsAllowed: true)
    { }
}
