using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E007_Trader : Event
{
    // Static
    public override int Id => 7;
    protected override float BaseProbability => 2f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.1f},
        {LocationType.City, 2f},
        {LocationType.Woods, 0f},
    };

    private static LootTable TradeItemTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.NutSnack, 10),
        new(ItemType.WaterBottle, 10),
        new(ItemType.Bandage, 5),
        new(ItemType.Antibiotics, 5),
        new(ItemType.Knife, 2)
    );

    private static Dictionary<int, float> TradeItemBuyPriceTable = new Dictionary<int, float>()
    {
        { 2, 20 },
        { 3, 40 },
        { 4, 25 },
        { 5, 10 },
        { 6, 5 },
    };

    // Instance
    private Dictionary<ItemType, int> ItemPrices;
    private List<Item> BuyableItems;

    public E007_Trader(Game game) : base(game) { }
    public override Event GetEventInstance => new E007_Trader(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E007_Trader);
        foreach (var text in ResourceManager.Singleton.E007_Prices) ShowEventSprite(text.gameObject);

        // Set up prices
        ItemPrices = new Dictionary<ItemType, int>();
        foreach (ItemType type in Game.GetAllItemTypes())
        {
            ItemPrices.Add(type, HelperFunctions.GetWeightedRandomElement(TradeItemBuyPriceTable));
        }

        // Set up trade items
        BuyableItems = new List<Item>();
        for (int i = 0; i < 3; i++)
        {
            Item item = TradeItemTable.GetItem();
            item.transform.position = new Vector3(5.2f + i * 1.15f, -3.5f, 0f);
            BuyableItems.Add(item);
            ResourceManager.Singleton.E007_Prices[i].text = ItemPrices[item.Type].ToString();
        }
    }
    protected override EventStep GetInitialStep()
    {
        string eventText = "You come across a trader offering various items. He says that he is also willing to buy items.";
        return GetShopStep(eventText);
    }
    protected override void OnEventEnd()
    {
        foreach (Item item in BuyableItems)
            if (!item.IsPlayerOwned) Game.DestroyItem(item);
    }

    private EventStep GetShopStep(string text)
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Options - Buy
        foreach(Item buyableItem in BuyableItems)
        {
            int price = ItemPrices[buyableItem.Type];
            if(Game.GetItemTypeAmount(ItemType.Coin) >= price)
                dialogueOptions.Add(new EventDialogueOption("Buy " + buyableItem.Name + " for " + price + " coins", () => BuyItem(buyableItem)));
        }

        // Item Options - Sell
        foreach (ItemType type in Game.GetAllItemTypes())
        {
            if (type == ItemType.Coin) continue;
            int price = ItemPrices[type] - 1;
            itemOptions.Add(new EventItemOption(type, "Sell for " + price + " coins", SellItem));
        }

        // Dialogue Option - Continue
        dialogueOptions.Add(new EventDialogueOption("Continue", Continue));

        // Event
        return new EventStep(text, dialogueOptions, itemOptions);
    }
    private EventStep BuyItem(Item item)
    {
        int price = ItemPrices[item.Type];
        string text = "You bought the " + item.Name + " for " + price + " coins.";
        List<Item> payedCoins = Game.DestroyOwnedItems(ItemType.Coin, price);
        Game.AddItemToInventory(item);
        BuyableItems.Remove(item);

        EventStep nextStep = GetShopStep(text);

        return nextStep;
    }
    private EventStep SellItem(Item item)
    {
        int price = ItemPrices[item.Type] - 1;
        Game.DestroyOwnedItem(item);
        List<Item> addedCoins = Game.AddItemsToInventory(ItemType.Coin, price);

        EventStep nextStep = GetShopStep("You sold the " + item.Name + " for " + price + " coins.");

        return nextStep;
    }
    private EventStep Continue()
    {
        return new EventStep("You wish the trader a nice day and continue.", null, null);
    }


}
