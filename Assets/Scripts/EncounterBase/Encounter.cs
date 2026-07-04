using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An instance of an encounter.
/// </summary>
public abstract class Encounter
{
    public Game Game { get; private set; }
    public WorldMap WorldMap => WorldMap.Instance;
    public EncounterDef Def { get; private set; }
    public Quest Mission { get; private set; }
    public WorldMapTile Tile { get; private set; }

    public BiomeDef Biome => Tile.Biome;
    public LootTable BiomeLootTable => Tile.Biome.LootTable;

    // Persistence
    private List<Item> EncounterItems = new List<Item>();

    // During encounter
    protected bool IsEncounterDone; // If set to true, the next step will have no more options (will default to "continue journey" (or similar based on time of day))
    private List<GameObject> EncounterSprites = new List<GameObject>();
    private HashSet<string> UsedOncePerDayOptions = new HashSet<string>();
    private HashSet<string> UsedOnceEverOptions = new HashSet<string>();

    public Encounter() { } // Empty constructor for activator
    public virtual void Init(Game game, EncounterDef def, WorldMapTile tile)
    {
        Game = game;
        Def = def;
        Tile = tile;
        OnInitialize();
    }

    /// <summary>
    /// Returns the modified version of a loot table taking in account the current biome.
    /// </summary>
    protected LootTable GetBiomeAlteredLootTable(LootTable table)
    {
        return table.Union(BiomeLootTable);
    }

    public EncounterStep StartEncounter()
    {
        IsEncounterDone = false;
        UsedOncePerDayOptions.Clear();

        // Show encounter items
        foreach (Item item in EncounterItems.Where(i => !i.IsPlayerOwned && !i.IsDestroyed)) item.Show();

        // General subclass logic
        OnStartExtension();

        string startText = OnStart();

        return GetNextEncounterStep(startText);
    }
    protected virtual void OnStartExtension() { }

    public EncounterStep GetNextEncounterStep(string text)
    {
        RefreshSprites();

        return new EncounterStep(text, _GetOptions());
    }

    /// <summary>
    /// Gets called when the player has chosen an option, after the effect of the option has been executed and before the next step is generated.
    /// </summary>
    public virtual void OnOptionChosen(EncounterOption option)
    {
        // If the option is once per day, add it to the used options so it won't be available again today.
        if (option.OncePerDay) UsedOncePerDayOptions.Add(option.Text);

        // If the option is once ever, add it to the used options so it won't be available again.
        if (option.OnceEver) UsedOnceEverOptions.Add(option.Text);
    }


    /// <summary>
    /// Generates an item belonging to this encounter, that will automatically shown when the encounter starts and hidden when the encounter ends.
    /// <br/>Handles automatically if the item is destroyed or taken by the player during the encounter.
    /// </summary>
    protected Item GenerateEncounterItem(ItemDef itemDef, Vector2? position = null, float? rotation = null, int? sortingOrder = null)
    {
        Item item = Game.CreateItem(itemDef, hidden: true);
        if (position.HasValue) item.Renderer.SetPosition(position.Value.x, position.Value.y);
        if (rotation.HasValue) item.Renderer.SetRotation(rotation.Value);
        if (sortingOrder.HasValue) item.Renderer.SetSortingOrder(sortingOrder.Value);

        EncounterItems.Add(item);
        return item;
    }

    /// <summary>
    /// Adds all of a list of encounter items to the player's inventory.
    /// </summary>
    protected void TakeAllItems(List<Item> items)
    {
        foreach (Item item in items)
        {
            Game.AddExistingItemToInventory(item);
            if (EncounterItems.Contains(item)) EncounterItems.Remove(item);
        }
        items.Clear();
    }

    /// <summary>
    /// Adds a random item from a list of encounter items to the player's inventory. Removes the item from the list and the encounter items list.
    /// </summary>
    protected void TakeRandomItem(List<Item> items)
    {
        if (items.Count == 0) return;

        Item item = items.RandomElement();
        Game.AddExistingItemToInventory(item);
        if (EncounterItems.Contains(item)) EncounterItems.Remove(item);
        items.Remove(item);
    }

    /// <summary>
    /// Destroys all items in the specified list and removes them from the encounter items collection.
    /// </summary>
    protected void DestroyItems(List<Item> items)
    {
        foreach (Item item in items)
        {
            Game.DestroyItem(item);
            if (EncounterItems.Contains(item)) EncounterItems.Remove(item);
        }
        items.Clear();
    }


    private List<EncounterOption> _GetOptions()
    {
        if (IsEncounterDone) return new List<EncounterOption>();
        else if (IsTrading) return GetTradingOptions();
        else
        {
            List<EncounterOption> options = GetOptions();
            options.RemoveAll(o => (o.OncePerDay && UsedOncePerDayOptions.Contains(o.Text)) || (o.OnceEver && UsedOnceEverOptions.Contains(o.Text)));
            if (IsMoveOnOptionAvailable())
            {
                // Move on
                options.Add(new FixedOutcomeOption()
                {
                    Text = "Move on",
                    Action = MoveOn
                });
            }
            return options;
        }
    }


    /// <summary>
    /// Called exactly once when the encounter is first created, regardless if the player is there or the encounter is starting or not. Used to set up all initial randomized values (like items, etc.).
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// Called whenever this encounter starts, returning the text of the initial step of the encounter. For location encounters that player can come back to, this is called every time the player enters the location, not just the first time. For encounters that are only played once, this is called after InitializeEncounter.
    /// </summary>
    protected abstract string OnStart();

    /// <summary>
    /// Called every time the player chooses an option. Shows/Hides sprites according to the current state of the encounter.
    /// </summary>
    protected abstract void RefreshSprites();

    /// <summary>
    /// Returns the options that the player can choose from at the current step of the encounter based on the encounters current state. This is called every time the encounter step changes, so it can be used to change the options based on the player's previous choices.
    /// </summary>
    protected abstract List<EncounterOption> GetOptions();

    /// <summary>
    /// If true, there is a "Move On" option available to end the encounter.
    /// </summary>
    protected abstract bool IsMoveOnOptionAvailable();

    /// <summary>
    /// Called when the player chooses the "Move On" option, before the encounter is ended. Usually used to clean up visuals.
    /// </summary>
    protected virtual void OnMoveOn() { }

    /// <summary>
    /// Gets called when the encounter is over.
    /// </summary>
    protected virtual void OnEnd() { }

    /// <summary>
    /// Makes a gameobject belonging to this encounter visible. The gameobject will be hidden when the encounter ends.
    /// <br/>In the Unity hierarchy, the GameObject needs to be placed in GameScreen/Encounters/{EncounterDefName}/{spriteName}.
    /// </summary>
    protected void ShowEncounterSprite(string spriteName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        spriteObj.gameObject.SetActive(true);

        EncounterSprites.Add(spriteObj);
    }
    protected void HideEncounterSprite(string spriteName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        spriteObj.gameObject.SetActive(false);
        EncounterSprites.Remove(spriteObj);
    }
    protected void SetEncounterSpriteVisibility(string spriteName, bool show)
    {
        if (show) ShowEncounterSprite(spriteName);
        else HideEncounterSprite(spriteName);
    }
    protected void SetSprite(string objectName, string spriteName)
    {
        SpriteRenderer renderer = Game.EncounterContainer.transform.Find($"{Def.DefName}/{objectName}").gameObject.GetComponent<SpriteRenderer>();
        Sprite sprite = ResourceManager.LoadSprite($"Encounters/{Def.DefName}/{Def.DefName}_{spriteName}");
        renderer.sprite = sprite;
    }
    protected void SetBackground(string backgroundName)
    {
        Sprite sprite = ResourceManager.LoadSprite($"Backgrounds/{backgroundName}");
        Game.SetBackground(sprite);
    }
    protected void ShowPlayerCharacter(bool value) => Game.ShowPlayerCharacter(value);

    /// <summary>
    /// Ends the encounter. Only called from Game.
    /// </summary>
    public void EndEncounter()
    {
        // Hide encounter items
        foreach (Item item in EncounterItems.Where(i => !i.IsPlayerOwned && !i.IsDestroyed)) item.Hide();

        // Hide encounter sprites
        foreach (GameObject sprite in EncounterSprites) sprite.gameObject.SetActive(false);

        // Reset some state flags
        IsTrading = false;

        EncounterSprites.Clear();
        OnEnd();
    }

    private string MoveOn()
    {
        OnMoveOn();
        IsEncounterDone = true;
        return "You move on. You can now freely use items again before continuing your journey.";
    }

    #region General Options

    protected FixedOutcomeOption GetConsumeOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Consume items",
            Description = "Use items from your inventory.",
            Action = ConsumeSlottedItem,
            Sprite = PlayerCharacterRenderer.Instance.Head,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    CustomItemSet = ItemSet.ConsumableItems,
                    CustomItemSetName = "Consumable",
                    IsDestroyingItem = true,
                }
            }
        };
    }
    private string ConsumeSlottedItem()
    {
        Item itemToConsume = Game.ItemUsedInSelectedOption;
        Game.Instance.ConsumeItem(itemToConsume);
        return Game.Instance.CurrentEncounterStep.Text; // Don't change encounter text when consuming items, just consume the item and stay on the same step.
    }

    #endregion

    #region Trading Interface

    protected string InitiateTrade(string text, List<ItemDef> itemsToBuy, List<ItemDef> itemsToSell = null, bool canBuyRumour = false)
    {
        IsTrading = true;
        ItemsToBuy = itemsToBuy;
        ItemsToSell = itemsToSell ?? new List<ItemDef>();
        CanBuyRumour = canBuyRumour;
        return text;
    }

    protected bool IsTrading { get; private set; }
    private List<ItemDef> ItemsToBuy = new List<ItemDef>();
    private List<ItemDef> ItemsToSell = new List<ItemDef>();
    private bool CanBuyRumour;

    private List<EncounterOption> GetTradingOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        foreach (ItemDef itemDef in ItemsToBuy)
        {
            if (itemDef == ItemDefOf.Coin) continue;
            if (itemDef.Value <= 0) continue;
            options.Add(GetBuyItemOption(itemDef));
        }
        foreach (ItemDef itemDef in ItemsToSell)
        {
            if (itemDef == ItemDefOf.Coin) continue;
            if (itemDef.Value <= 0) continue;
            options.Add(GetSellItemOption(itemDef));
        }
        if (CanBuyRumour) options.Add(GetBuyInformationOption());
        options.Add(GetDoneTradingOption());
        return options;
    }
    private EncounterOption GetBuyItemOption(ItemDef itemDef)
    {
        List<ItemSlot> itemSlots = new List<ItemSlot>();
        for (int i = 0; i < itemDef.Value; i++)
        {
            itemSlots.Add(new ItemSlot()
            {
                Item = ItemDefOf.Coin,
                IsRequired = true,
                IsDestroyingItem = true,
            });
        }

        return new FixedOutcomeOption()
        {
            Text = $"Buy {itemDef.Label}.",
            Action = () => BuyItem(itemDef),
            ItemSlots = itemSlots,
        };
    }
    private string BuyItem(ItemDef itemDef)
    {
        Game.AddNewItemToInventory(itemDef);
        return $"You trade {itemDef.Value} {"coin".Pluralize(itemDef.Value)} for {itemDef.Label}.";
    }

    private EncounterOption GetSellItemOption(ItemDef itemDef)
    {
        return new FixedOutcomeOption()
        {
            Text = $"Sell {itemDef.Label}.",
            Action = () => SellItem(itemDef),
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Item = itemDef,
                    IsRequired = true,
                    IsDestroyingItem = true,
                }
            }
        };
    }
    private string SellItem(ItemDef itemDef)
    {
        Game.AddNewItemsToInventory(ItemDefOf.Coin, itemDef.Value);
        return $"You trade {itemDef.Label} for {itemDef.Value} {"coin".Pluralize(itemDef.Value)}.";
    }

    private EncounterOption GetBuyInformationOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Buy information for 3 coins.",
            Action = BuyInformation,
            OncePerDay = true,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsRequired = true,
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsRequired = true,
                    IsDestroyingItem = true,
                },
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsRequired = true,
                    IsDestroyingItem = true,
                },
            }
        };
    }
    private string BuyInformation()
    {
        string rumourText = Game.LearnRumour();
        if (rumourText != null)
            return $"You trade coins for a piece of information.{rumourText}";

        return "You trade coins, but they don't have anything useful to share.";
    }

    private EncounterOption GetDoneTradingOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Done trading",
            Action = DoneTrading,
        };
    }
    private string DoneTrading()
    {
        IsTrading = false;
        return "You finish trading.";
    }

    #endregion

    #region Getters

    public virtual string Label => Def.Label; // Shown on world map

    #endregion

}
