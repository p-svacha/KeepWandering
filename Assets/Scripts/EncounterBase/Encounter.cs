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

    // Practical getters
    public BiomeDef Biome => Tile.Biome;
    public LootTable BiomeLootTable => Tile.Biome.LootTable;
    public List<Item> ItemsUsedInOption => Game.ItemsUsedInSelectedOption;

    /// <summary>
    /// Reference to the first item that was used in the previously selected option, with durability / destruction logic already handled. So them might have IsDestroyed = true if the item was destroyed by the option, or if durability ran out.
    /// </summary>
    public Item ItemUsedInOption => Game.ItemUsedInSelectedOption;

    // Persistence
    private List<Item> EncounterItems = new List<Item>();

    // During encounter
    protected bool IsEncounterDone; // If set to true, the next step will have no more options (will default to "continue journey" (or similar based on time of day))
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

        // Activate encounter sprite container
        Transform encounterContainer = Game.EncounterContainer.transform.Find($"{Def.DefName}");
        if(encounterContainer == null) throw new System.Exception($"Encounter container for {Def.DefName} not found in GameScreen/Encounters.");
        encounterContainer.gameObject.SetActive(true);

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

        bool isFinalStep = IsEncounterDone;
        return new EncounterStep(text, _GetOptions(), isFinalStep);
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


    /// <summary>
    /// Base logic of what options are available to the player at the current step of the encounter.
    /// </summary>
    private List<EncounterOption> _GetOptions()
    {
        // If encounter is done, only allow options to use inventory items and to continue the day.
        if (IsEncounterDone)
        {
            List<EncounterOption> options = new List<EncounterOption>();

            // Item use options
            options.AddRange(GetGeneralItemUseOptions());

            // Continue day option
            string endEncounterOptionText;
            string endEncounterOptionDesc;

            if (Game.TimeOfDay == TimeOfDayDefOf.Afternoon)
            {
                endEncounterOptionText = "Keep Wandering";
                endEncounterOptionDesc = "Continue your day.";
            }
            else if (Game.TimeOfDay == TimeOfDayDefOf.Evening)
            {
                endEncounterOptionText = "Sleep";
                endEncounterOptionDesc = "Go to sleep and hope for a calm night.";
            }
            else if (Game.TimeOfDay == TimeOfDayDefOf.Night)
            {
                endEncounterOptionText = "Sleep";
                endEncounterOptionDesc = "Go back to sleep and hope for a calm rest of the night.";
            }
            else throw new System.Exception("Unexpected time of day for encounter end option.");

            FixedOutcomeOption endEncounterOption = new FixedOutcomeOption()
            {
                Text = endEncounterOptionText,
                Description = endEncounterOptionDesc,
                Action = EndCurrentTimeOfDay
            };
            options.Add(endEncounterOption);

            return options;
        }

        // Currently trading, show trading options
        else if (IsTrading) return GetTradingOptions();

        // Otherwise, show the encounter's options
        else
        {
            List<EncounterOption> options = GetOptions();
            options.RemoveAll(o => (o.OncePerDay && UsedOncePerDayOptions.Contains(o.Text)) || (o.OnceEver && UsedOnceEverOptions.Contains(o.Text)));

            // Move on
            if (IsMoveOnOptionAvailable())
            {
                options.Add(new FixedOutcomeOption()
                {
                    Text = "Move on",
                    Action = MoveOn
                });
            }

            return options;
        }
    }

    private string EndCurrentTimeOfDay()
    {
        if (Game.CurrentEncounter.Def.Type == EncounterType.Evening)
        {
            Game.EndEveningEncounter();
        }
        else if (Game.CurrentEncounter.Def.Type == EncounterType.Night)
        {
            Game.EndNightEncounter();
        }
        else // It is afternoon
        {
            Game.EndAfternoonEncounter();
        }
        return "";
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
    /// Sets the visibility of a gameobject belonging to this encounter. The gameobject will be hidden when the encounter ends.
    /// <br/>In the Unity hierarchy, the GameObject needs to be placed in GameScreen/Encounters/{EncounterDefName}/{spriteName}.
    /// </summary>
    protected void SetObjectVisibility(string spriteName, bool show)
    {
        GameObject obj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        obj.SetActive(show);
    }

    protected void SetSpriteVisibility(SpriteRenderer renderer, bool show)
    {
        renderer.gameObject.SetActive(show);
    }

    /// <summary>
    /// Sets the sprite of the GameObject named <paramref name="objectName"/> to the sprite named <paramref name="spriteName"/>.
    /// <br/>The GameObject must be placed in GameScreen/Encounters/{EncounterDefName}/{objectName} in the Unity hierarchy.
    /// <br/>The sprite must be placed in Resources/Encounters/{EncounterDefName}/{EncounterDefName}_{spriteName}.png.
    /// </summary>
    protected void SetSprite(string objectName, string spriteName)
    {
        SpriteRenderer renderer = Game.EncounterContainer.transform.Find($"{Def.DefName}/{objectName}").gameObject.GetComponent<SpriteRenderer>();
        SetSprite(renderer, spriteName);
    }

    /// <summary>
    /// Sets the sprite of the given <paramref name="renderer"/> to the sprite named <paramref name="spriteName"/>.
    /// <br/>The sprite must be placed in Resources/Encounters/{EncounterDefName}/{EncounterDefName}_{spriteName}.png.
    /// </summary>
    public void SetSprite(SpriteRenderer renderer, string spriteName)
    {
        Sprite sprite = ResourceManager.LoadSprite($"Encounters/{Def.DefName}/{Def.DefName}_{spriteName}");
        renderer.sprite = sprite;
    }

    /// <summary>
    /// Returns the SpriteRenderer of the GameObject named <paramref name="objectName"/>.
    /// <br/>The GameObject must be placed in GameScreen/Encounters/{EncounterDefName}/{objectName} in the Unity hierarchy.
    /// </summary>
    public SpriteRenderer GetSprite(string objectName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{objectName}").gameObject;
        return spriteObj.GetComponent<SpriteRenderer>();
    }
    protected void SetBackground(string backgroundName)
    {
        Sprite sprite = ResourceManager.LoadSprite($"Backgrounds/{backgroundName}");
        Game.SetBackground(sprite);
    }
    protected void ShowPlayerCharacter(bool value) => Game.ShowPlayerCharacter(value);

    /// <summary>
    /// Ends the encounter. This is only allowed to be called from Game.
    /// </summary>
    public void EndEncounter()
    {
        // Hide encounter items
        foreach (Item item in EncounterItems.Where(i => !i.IsPlayerOwned && !i.IsDestroyed)) item.Hide();

        // Hide encounter sprite container
        Game.EncounterContainer.transform.Find($"{Def.DefName}").gameObject.SetActive(false);

        // Reset some state flags
        IsTrading = false;

        OnEnd();
    }

    private string MoveOn()
    {
        OnMoveOn();
        IsEncounterDone = true;
        return "You move on. You can now freely use items again before continuing your journey.";
    }

    #region General Options

    /// <summary>
    /// Returns all options, that are about using items in the players inventory on the player character, like consuming items, applying medical items to wounds/fractures, etc..
    /// These options are available in the morning and at the end of other encounters.
    /// </summary>
    protected List<EncounterOption> GetGeneralItemUseOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        options.Add(GetConsumeItemOption());
        options.AddRange(GetMedicalItemUseOptions());
        return options;
    }

    private FixedOutcomeOption GetConsumeItemOption(bool torso = false)
    {
        return new FixedOutcomeOption()
        {
            Text = "Consume Item",
            Description = "Use items from your inventory.",
            Action = ConsumeItem,
            Sprite = torso ? PlayerCharacterRenderer.Instance.RightArm.Renderer : PlayerCharacterRenderer.Instance.Head,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    CustomItemSet = ItemSets.ConsumableItems,
                    IsDestroyingItem = true,
                }
            },
        };
    }
    private string ConsumeItem()
    {
        Item itemToConsume = Game.ItemUsedInSelectedOption;
        Game.Instance.ConsumeItem(itemToConsume);
        string verb = itemToConsume.Def.ConsumptionProperties.ConsumptionType.Verb;
        return $"You {verb} the {itemToConsume.Def.Label}.";
    }

    private List<EncounterOption> GetMedicalItemUseOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        options.AddRange(GetBandageWoundOptions());
        options.AddRange(GetTreatInfectionOptions());
        return options;
    }

    private List<EncounterOption> GetBandageWoundOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        foreach (Wound wound in Game.Player.BandagableWounds)
        {
            options.Add(new SkillCheckOption()
            {
                Text = $"Bandage {wound.Def.LabelCap}",
                Description = $"Use an item to tend the {wound.Def.Label}.",
                Difficulty = 100,
                Action = (outcome) => TryBandageWound(outcome, wound),
                CanPartiallySucceed = false,
                CanCriticallySucceed = true,
                CanCriticallyFail = true,
                Sprite = wound.Renderer.WoundSpriteRenderer,
                ItemSlots = new List<ItemSlot>()
                {
                    new ItemSlot()
                    {
                        IsRequired = true,
                        Tag = ItemTagDefOf.WoundBandaging,
                        RequiredTagLevel = 5,
                    }
                },
            });
        }
        return options;
    }

    private string TryBandageWound(OptionOutcomeDef outcome, Wound wound)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.BandageWound(wound);
            Game.ModifySurvival(+1);
            return $"You successfully bandage the {wound.Def.Label}, and improve your survival skills.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.BandageWound(wound);
            return $"You successfully bandage the {wound.Def.Label}.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return $"You fail to properly bandage the {wound.Def.Label}.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            if(!ItemUsedInOption.IsDestroyed && Random.value < 0.5f)
            {
                Game.DestroyItem(ItemUsedInOption);
                return $"You fumble while trying to bandage the {wound.Def.Label}, failing and destroying the {ItemUsedInOption.Def.Label} in the process.";
            }
            else
            {
                wound.ModifySeverity(1f);
                return $"You fail to properly bandage the {wound.Def.Label}. By tampering with it, you make it worse!";
            }
        }

        throw new OutcomeNotHandledException(outcome);
    }

    private List<EncounterOption> GetTreatInfectionOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        foreach (Wound wound in Game.Player.TreatableWounds)
        {
            options.Add(new SkillCheckOption()
            {
                Text = $"Treat Infected {wound.Def.LabelCap}",
                Description = $"Use an item to treat the {wound.Def.Label}.",
                Difficulty = 100,
                Action = (outcome) => TryTreatInfection(outcome, wound),
                CanCriticallyFail = false,
                CanPartiallySucceed = false,
                CanCriticallySucceed = false,
                Sprite = wound.Renderer.WoundSpriteRenderer,
                ItemSlots = new List<ItemSlot>()
                {
                    new ItemSlot()
                    {
                        IsRequired = true,
                        Tag = ItemTagDefOf.InfectionTreatment,
                    }
                }
            });
        }
        return options;
    }

    private string TryTreatInfection(OptionOutcomeDef outcome, Wound wound)
    {
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.TreatInfection(wound);
            return $"You successfully treat the infected {wound.Def.Label}, which should help the wound to heal.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return $"You fail to properly treat the infected {wound.Def.Label}.";
        }
        throw new OutcomeNotHandledException(outcome);
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
        OnTradingDone();
        return "You finish trading.";
    }

    /// <summary>
    /// Called when the player has finished trading and is moving on to the next step of the encounter.
    /// </summary>
    protected virtual void OnTradingDone() { }

    #endregion

    #region Getters

    public virtual string Label => Def.Label; // Shown on world map
    public virtual Sprite GetWorldMapSprite() => ResourceManager.LoadSprite("EncounterMarker/" + Def.DefName);

    #endregion

}
