using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E007_Trader : Event
{
    private static float BaseProbability = 2f;
    private static Dictionary<LocationType, float> LocationProbabilityTable = new Dictionary<LocationType, float>()
    {
        {LocationType.Suburbs, 0.5f},
        {LocationType.City, 2f},
        {LocationType.Woods, 0f},
        {LocationType.GroceryStore, 1f},
    };

    private static Dictionary<ItemType, float> TradeItemTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 10},
        { ItemType.NutSnack, 10},
        { ItemType.WaterBottle, 10},
        { ItemType.Bandage, 5},
        { ItemType.Antibiotics, 5},
        { ItemType.Knife, 2},
    };

    private static Dictionary<int, float> TradeItemBuyPriceTable = new Dictionary<int, float>()
    {
        { 2, 20 },
        { 3, 40 },
        { 4, 25 },
        { 5, 10 },
        { 6, 5 },
    };

    public static float GetProbability(Game game)
    {
        return BaseProbability * LocationProbabilityTable[game.CurrentLocation.Type];
    }

    public static E007_Trader GetEventInstance(Game game)
    {
        // Sprite
        ResourceManager.Singleton.E007_Trader.SetActive(true);
        foreach (var text in ResourceManager.Singleton.E007_Prices) text.gameObject.SetActive(true);

        // Set up prices
        Dictionary<ItemType, int> buyPrices = new Dictionary<ItemType, int>();
        foreach (ItemType type in game.GetAllItemTypes())
        {
            buyPrices.Add(type, HelperFunctions.GetWeightedRandomElement(TradeItemBuyPriceTable));
        }

        // Set up trade items
        List<Item> buyableItems = new List<Item>();
        for(int i = 0; i < 3; i++)
        {
            ItemType type = HelperFunctions.GetWeightedRandomElement(TradeItemTable);
            Item item = game.GetItemInstance(type);
            item.transform.position = new Vector3(5.2f + i * 1.15f, -3.5f, 0f);
            buyableItems.Add(item);
            ResourceManager.Singleton.E007_Prices[i].text = buyPrices[type].ToString();
        }

        // Event
        string eventText = "You come across the bunker that " + E004_ParrotWoman.WomanName + " told you about.";
        return new E007_Trader(GetInitialStep(game, eventText, buyableItems, buyPrices), buyableItems);
    }

    private static EventStep GetInitialStep(Game game, string text, List<Item> buyableItems, Dictionary<ItemType, int> buyPrices)
    {
        // Options
        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Options - Buy
        foreach(Item buyableItem in buyableItems)
        {
            int price = buyPrices[buyableItem.Type];
            if(game.GetItemTypeAmount(ItemType.Coin) >= price)
                dialogueOptions.Add(new EventOption("Buy " + buyableItem.Name + " for " + price + " coins", (game) => BuyItem(game, buyableItem, buyableItems, buyPrices)));
        }

        // Item Options - Sell
        foreach (ItemType type in game.GetAllItemTypes())
        {
            if (type == ItemType.Coin) continue;
            int price = buyPrices[type] - 1;
            itemOptions.Add(new EventItemOption(type, "Sell for " + price + " coins", (game, item) => SellItem(game, item, buyableItems, buyPrices)));
        }

        // Dialogue Option - Continue
        dialogueOptions.Add(new EventOption("Continue", Continue));

        // Event
        return new EventStep(text, null, null, dialogueOptions, itemOptions);
    }

    private static EventStep BuyItem(Game game, Item item, List<Item> buyableItems, Dictionary<ItemType, int> buyPrices)
    {
        int price = buyPrices[item.Type];
        string text = "You bought the " + item.Name + " for " + price + " coins.";
        List<Item> payedCoins = game.DestroyOwnedItems(ItemType.Coin, price);
        game.AddItemToInventory(item);
        buyableItems.Remove(item);

        EventStep nextStep = GetInitialStep(game, text, buyableItems, buyPrices);
        nextStep.AddedItems = new List<Item>() { item };
        nextStep.RemovedItems = payedCoins;

        return nextStep;
    }

    private static EventStep SellItem(Game game, Item item, List<Item> buyableItems, Dictionary<ItemType, int> buyPrices)
    {
        int price = buyPrices[item.Type] - 1;
        game.DestroyOwnedItem(item);
        List<Item> addedCoins = game.AddItemsToInventory(ItemType.Coin, price);

        EventStep nextStep = GetInitialStep(game, "You sold the " + item.Name + " for " + price + " coins.", buyableItems, buyPrices);
        nextStep.AddedItems = addedCoins;
        nextStep.RemovedItems = new List<Item>() { item };

        return nextStep;
    }

    private static EventStep Continue(Game game)
    {
        return new EventStep("You wish the trader a nice day and continue.", null, null, null, null);
    }

    public E007_Trader(EventStep initialStep, List<Item> eventItems) : base(
        EventType.E007_Trader,
        initialStep,
        eventItems,
        new List<GameObject>() { ResourceManager.Singleton.E007_Trader, ResourceManager.Singleton.E007_Prices[0].gameObject, ResourceManager.Singleton.E007_Prices[1].gameObject, ResourceManager.Singleton.E007_Prices[2].gameObject },
        itemActionsAllowed: true)
    { }
}
