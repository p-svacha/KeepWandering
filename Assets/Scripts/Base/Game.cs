using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    public EncounterManager EncounterManager { get; private set; }
    public GameObject EncounterContainer { get; private set; }
    public GameObject BiomeBackgroundContainer { get; private set; }

    // Game State
    public GameState State { get; private set; }
    public int Day { get; private set; }
    public int ItemIdCounter { get; private set; }
    public MorningReport LatestMorningReport { get; private set; }
    public Encounter CurrentEncounter;
    public EncounterStep CurrentEventStep;

    // Event Step Outcome
    public List<Item> ItemsAddedSinceLastStep = new List<Item>();
    public List<Item> ItemsRemovedSinceLastStep = new List<Item>();
    public List<Wound> WoundsAddedSinceLastStep = new List<Wound>();

    // Position
    public DayAction DayAction { get; private set; } // The type of action the player is doing on the current day.
    public List<WorldMapTile> PathHistory = new List<WorldMapTile>();
    public WorldMapTile CurrentPosition { get; private set; } // Position the player is currently at.
    public WorldMapTile TargetPosition { get; private set; } // Position the player is moving towards.
    public bool PlayerIsOnQuarantinePerimeter => QuarantineZone.IsOnPerimeter(CurrentPosition);

    // Missions
    public Dictionary<MissionId, Mission> Missions = new Dictionary<MissionId, Mission>();

    // Elements
    [Header("Main Elements")]
    public Camera MainCamera;
    public GameUI UI;

    [Header("Items")]
    private Item CurrentHoverItem;
    private float CurrentHoverTime;
    private Item CurrentInteractionItem;
    public List<Item> Inventory = new List<Item>();

    [Header("Characters")]
    public PlayerCharacter Player;
    //public List<Companion> Companions = new List<Companion>();

    [Header("World Map")]
    public WorldMap WorldMap;
    public CameraHandler WorldMapCamera;
    public Area QuarantineZone => WorldMap.QuarantineZone;

    // Debug
    public const bool DEBUG_RANDOM_CHOICES = true;

    #region Game Flow

    void Start()
    {
        State = GameState.Initializing;
        Instance = this;

        EncounterContainer = GameObject.Find("Encounters");
        BiomeBackgroundContainer = GameObject.Find("BiomeBackgrounds");

        ResourceManager.ClearCache();
        DefDatabaseRegistry.InitDefs();

        StartGame();
    }

    private void StartGame()
    {
        // Init world
        WorldMap.Init(this);
        //WorldMap.GenerateWorld(zoneRadius: 18, numAdditionalTiles: 400);
        WorldMap.GenerateWorld(zoneRadius: 2, numAdditionalTiles: 10);
        WorldMapCamera.Init(this);
        SetPosition(WorldMap.GetTile(Vector2Int.zero));
        WorldMap.ResetCamera();

        // Init events
        EncounterManager = new EncounterManager(this);

        // Init player
        PlayerCharacterRenderer.Instance.Init();
        ItemIdCounter = 0;
        Player = new PlayerCharacter(this);

        // Start with 1 food item, 1 drink item, 1 medical item and 1 random item in inventory
        AddNewItemToInventory(GetRandomItemDefWithTag(ItemTagDefOf.Food));
        AddNewItemToInventory(GetRandomItemDefWithTag(ItemTagDefOf.Drink));
        AddNewItemToInventory(GetRandomItemDefWithTag(ItemTagDefOf.Medical));
        AddNewItemToInventory(GetRandomItemDef());

        // Init UI
        UI.Init(this);
        UI.ContextMenu.Init(this);

        SwitchState(GameState.InDayTransition);
    }

    // Update is called once per frame
    void Update()
    {
        bool uiClick = EventSystem.current.IsPointerOverGameObject();

        // Escape - Escape menu
        if (Input.GetKeyDown(KeyCode.Escape)) UI.ToggleEscapeMenu();

        // M - Map
        if (Input.GetKeyDown(KeyCode.M)) UI.ToggleWorldMap();

        // Update per state
        if (State == GameState.InGame)
        {
            ItemDragDropManager.Update();

            if (!ItemDragDropManager.IsDragging)
            {
                UpdateHoveredItem();

                // Left Click -> Start Drag
                if (Input.GetMouseButtonDown(0) && !uiClick)
                {
                    if (CurrentHoverItem != null && ItemDragDropManager.CanDragItem(CurrentHoverItem))
                    {
                        ItemDragDropManager.StartDrag(CurrentHoverItem);
                        CurrentHoverItem.Renderer.Unhighlight();
                        CurrentHoverItem = null;
                        UI.Tooltip.Hide();
                    }
                    else if (UI.ContextMenu.gameObject.activeSelf)
                    {
                        UI.ContextMenu.Hide();
                        CurrentHoverTime = 0f;
                        CurrentInteractionItem = null;
                    }
                }

                // Right Click -> Context Menu
                if (Input.GetMouseButtonDown(1) && !uiClick)
                {
                    if (CurrentHoverItem != null)
                    {
                        OnItemRightClicked(CurrentHoverItem);
                    }
                    else if (UI.ContextMenu.gameObject.activeSelf)
                    {
                        UI.ContextMenu.Hide();
                        CurrentHoverTime = 0f;
                        CurrentInteractionItem = null;
                    }
                }
            }
        }
    }

    private void OnItemRightClicked(Item item)
    {
        // Get interaction options for item
        List<InteractionOption> options = item.GetInteractionOptions();
        Debug.Log($"Clicked on " + item.Label + " with " + options.Count + " interaction options.");

        // If it has any, show context menu
        if (options.Count > 0)
        {
            Debug.Log($"Show context menu for " + CurrentHoverItem.Label);
            CurrentInteractionItem = CurrentHoverItem;
            UI.ContextMenu.Show(CurrentHoverItem);
            UI.Tooltip.Hide();
        }
    }

    /// <summary>
    /// Handles which item is currently hovered by the mouse and updates the visuals accordingly. Also handles showing the tooltip after hovering an item for a certain time.
    /// </summary>
    private void UpdateHoveredItem()
    {
        bool uiClick = EventSystem.current.IsPointerOverGameObject();
        Item prevHoveredItem = CurrentHoverItem;
        Item newHoveredItem = null;

        Vector2 mouseWorldPos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.GetComponent<ItemRenderer>() != null)
        {
            newHoveredItem = hit.collider.GetComponent<ItemRenderer>().Item;
        }


        if (newHoveredItem != prevHoveredItem) // Hovering a new item
        {
            // Disable highlight of previous item
            if (prevHoveredItem != null) prevHoveredItem.Renderer.Unhighlight();

            if (newHoveredItem != null)
            {
                newHoveredItem.Renderer.Highlight(Color.white); // Highlight new item
                CurrentHoverTime = 0f; // Reset hover time for tooltip
            }

            // Hide tooltip
            UI.Tooltip.Hide();

            // Update current hovered item
            CurrentHoverItem = newHoveredItem;
        }
        else // Still hovering the same item
        {
            if (CurrentHoverItem != null) // Update tooltip
            {
                CurrentHoverTime += Time.deltaTime;
                if (CurrentHoverTime >= GameUI.TOOLTIP_HOVER_TIME && !UI.ContextMenu.gameObject.activeSelf)
                    UI.Tooltip.Show(CurrentHoverItem);
            }
        }
    }

    private void SwitchState(GameState newState)
    {
        GameState oldState = State;
        Debug.Log("Switch State " + oldState.ToString() + " --> " + newState.ToString());

        switch (oldState)
        {
            case GameState.InGame:
                UI.Tooltip.Hide();
                UI.ContextMenu.Hide();
                ItemDragDropManager.CancelDrag();
                break;
        }

        State = newState;

        switch (newState)
        {
            case GameState.InDayTransition:
                StartMorningEvent();
                UI.HoldBlackTransition(GameUI.TRANSITION_HOLD_TIME);
                break;

            case GameState.DayTransitionFadeOut:
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEventTransitionIn:
            case GameState.EndMorningReportTransitionIn:
                UI.FadeInBlackTransition(GameUI.TRANSITION_FADE_TIME);
                UI.BlackTransitionText.text = "";
                break;

            case GameState.EndMorningReportTransitionOut:
                if (DayAction == DayAction.Rest) StartEveningEncounter(); // Resting skips afternoon
                else StartAfternoonEncounter();
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEventTransitionOut:
                StartEveningEncounter();
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.DayTransitionFadeIn:
                UI.FadeInBlackTransition(GameUI.TRANSITION_FADE_TIME);
                UI.DayText.text = "Day " + Day;
                UI.BlackTransitionText.text = "Day " + Day;
                break;

            case GameState.GameOver:
                UI.HoldBlackTransition(60f);
                break;
        }

        if (State != GameState.GameOver) CheckGameOver();
    }

    public void DisplayEncounterStep(EncounterStep step, OptionOutcomeDef prevOutcome = null)
    {
        // Unhighlight from previous step
        ForceUnhighlightAllInventoryItems();

        // Display new step
        CurrentEventStep = step;
        if (step != null)
        {
            UI.EventStepDisplay.Init(step, prevOutcome);
            step.HighlightSlottableItems();
        }

        // Clear event step outcome
        ItemsAddedSinceLastStep.Clear();
        ItemsRemovedSinceLastStep.Clear();
        WoundsAddedSinceLastStep.Clear();
    }

    /// <summary>
    /// Called when the player selects an encounter step option. Handles slot item resolution for all options,
    /// then executes the selected option and displays the next step.
    /// </summary>
    public void SelectEncounterOption(EncounterOption selectedOption)
    {
        UI.StatPanel.UnhighlightAll();

        // Empty slots of all non-selected options - return items to cart
        foreach (EncounterOption option in CurrentEventStep.Options)
        {
            if (option == selectedOption) continue;
            foreach (ItemSlot slot in option.ItemSlots)
            {
                if (slot.IsFilled) slot.Empty();
            }
        }

        // Resolve slots of the selected option - apply destruction chance
        foreach (ItemSlot slot in selectedOption.ItemSlots)
        {
            if (!slot.IsFilled) continue;

            Item item = slot.TakeItem();

            if (slot.DestructionChance > 0f && Random.value <= slot.DestructionChance)
            {
                // Item destroyed
                DestroyOwnedItem(item);
            }
            else
            {
                // Item survives - return to cart
                item.Show();
                DropItemIntoCart(item);
            }
        }

        // Execute the option
        EncounterStep nextEventStep = selectedOption.Execute(out OptionOutcomeDef outcome);
        if (nextEventStep == null) throw new System.Exception("Selected option " + selectedOption.Text + " returned null as next event step!");
        DisplayEncounterStep(nextEventStep, outcome);
    }

    public void ForceUnhighlightAllInventoryItems()
    {
        foreach (Item item in Inventory)
        {
            item.Renderer.Unhighlight(removeForced: true);
        }
    }

    public void CheckGameOver()
    {
        if (State == GameState.Initializing) return;

        string gameOver = GetGameOverReason();
        if (gameOver != null)
        {
            UI.BlackTransitionText.text = "Day " + Day + "\n" + gameOver;
            SwitchState(GameState.GameOver);
        }
    }

    private string GetGameOverReason()
    {
        // Lose
        foreach (HealthCondition condition in Player.HealthConditions)
        {
            string deathReason = condition.IsFatal();
            if (deathReason != null && deathReason != "") return deathReason;
        }

        // Win
        if (!QuarantineZone.IsInArea(CurrentPosition)) return "You escaped the quarantine.\nYou win.";
        return null;
    }

    #endregion

    #region Morning

    private void StartMorningEvent()
    {
        UI.DayTimeText.text = "Morning";

        LatestMorningReport = new MorningReport(Day);

        Day++;

        Player.OnEndDay(this, LatestMorningReport);
        /*
        List<Companion> companionsCopy = new List<Companion>();
        foreach (Companion c in Companions) companionsCopy.Add(c);
        foreach (Companion c in companionsCopy) c.OnEndDay(this, LatestMorningReport);
        */
        OnGameStateChanged();

        // Show morning report
        UpdateMorningEvent();

        // Day UI Updates
        UI.BlackTransitionText.text = "Day " + Day;
        UI.DayText.text = "Day " + Day;

        // Enable destination selection of adjacent tiles
        WorldMap.CanSelectDestination = true;
        foreach (WorldMapTile nextPositionTarget in GetNextPositionTiles()) WorldMap.HighlightTileRed(nextPositionTarget);
    }

    /// <summary>
    /// Displays the morning event step.
    /// </summary>
    private void UpdateMorningEvent()
    {
        DisplayEncounterStep(GetMorningEncounter());
    }

    /// <summary>
    /// Creates the morning report event step that contains all information about what happened during the night and the options of what to do that day.
    /// </summary>
    private EncounterStep GetMorningEncounter()
    {
        // Text displaying night events
        string text = "";
        if (Day == 1) text = "After you saw the news you knew that you have to get out of the quarantine zone. You ran outside, grabbed your handcart and so starts your journey.";
        else if (LatestMorningReport.NightEvents.Count == 0) text = "You wake after an uneventful night.";
        else
        {
            text = $"You wake up in the {CurrentPosition.Biome.Label}. The following happened during the night:";
            foreach (string e in LatestMorningReport.NightEvents) text += "\n- " + e;
        }


        // Options
        List<EncounterOption> options = new List<EncounterOption>();

        if (Day == 1)
        {
            options.Add(new FixedOutcomeOption("Start Journey", "Open the map to choose your first location.", OpenMap)); // Move
        }
        else
        {
            string exposureAppendix = "\n\nThis will increase your exposure in this location, increasing the chance for attacks during the night!";
            options.Add(new FixedOutcomeOption("Move", "Open the map to choose your next location.", OpenMap)); // Move
            options.Add(new FixedOutcomeOption("Stay", "Stay in the current location to continue where you left off yesterday." + exposureAppendix, Stay)); // Stay
            options.Add(new FixedOutcomeOption("Rest", "Rest and recover your energy. Skips the afternoon encounter and potentially heals some injuries." + exposureAppendix, Rest)); // Rest
        }

        EncounterStep morningEventStep = new EncounterStep(text, options);
        return morningEventStep;
    }

    private EncounterStep Stay()
    {
        DayAction = DayAction.Stay;
        return EndMorning();
    }

    private EncounterStep Rest()
    {
        DayAction = DayAction.Rest;
        return EndMorning();
    }

    private EncounterStep OpenMap()
    {
        UI.OpenWorldMap();
        return GetMorningEncounter();
    }

    /// <summary>
    /// Returns all tiles the player can select when chosing what to do in the morning.
    /// </summary>
    public List<WorldMapTile> GetNextPositionTiles()
    {
        List<WorldMapTile> tiles = new List<WorldMapTile>();
        tiles.Add(CurrentPosition);
        foreach (Direction dir in HelperFunctions.GetAdjacentHexDirections())
        {
            Vector2Int adjCoord = HelperFunctions.GetAdjacentHexCoordinates(CurrentPosition.Coordinates, dir);
            WorldMapTile adjTile = WorldMap.GetTile(adjCoord);
            if (adjTile.IsPassable()) tiles.Add(adjTile);
        }
        return tiles;
    }

    /// <summary>
    /// Gets called when a tile is clicked on on the world map.
    /// </summary>
    public void SelectTileOnMap(WorldMapTile tile)
    {
        if (!GetNextPositionTiles().Contains(tile)) return;

        DayAction = DayAction.Move;
        TargetPosition = tile;
        EndMorning();
    }

    public EncounterStep EndMorning()
    {
        UI.CloseAllWindows();

        // Reset world map selection
        WorldMap.CanSelectDestination = false;
        WorldMap.UnhighlightAllRedTiles();

        // Switch state
        SwitchState(GameState.EndMorningReportTransitionIn);
        return null;
    }

    #endregion

    #region Afternoon

    private void StartAfternoonEncounter()
    {
        UI.DayTimeText.text = "Afternoon";

        // Move to selected target position
        if (DayAction == DayAction.Move)
        {
            SetPosition(TargetPosition);
            TargetPosition = null;
        }

        // If the tile already has a location encounter set, just take that.
        if (CurrentPosition.Encounter != null)
        {
            CurrentEncounter = CurrentPosition.Encounter;
        }

        // Else generate a new one
        else
        {
            LocationEncounter newEncounter = EncounterManager.GenerateLocationEncounter(CurrentPosition);
            CurrentPosition.SetEncounter(newEncounter);
            CurrentEncounter = newEncounter;
        }

        // Display the encounter
        EncounterStep initialStep = CurrentEncounter.StartEncounter();
        DisplayEncounterStep(initialStep);

        // Update status
        OnGameStateChanged();
    }

    public void EndAfternoonEvent()
    {
        UI.CloseAllWindows();
        SwitchState(GameState.EndEventTransitionIn);
    }

    #endregion

    #region Evening

    private void StartEveningEncounter()
    {
        UI.DayTimeText.text = "Evening";

        // Clear previous encounter
        if (CurrentEncounter != null) CurrentEncounter.EndEncounter();
        CurrentEncounter = null;

        // Display evening event
        DisplayEncounterStep(GetEveningEncounter()); // todo: replace with biome specific evening encounter
    }

    /// <summary>
    /// Creates the morning report event step that contains all information about what happened during the night and the options of what to do that day.
    /// </summary>
    private EncounterStep GetEveningEncounter()
    {
        // Text displaying night events
        string text = $"How would you like to spend your evening in the {CurrentPosition.Biome.Label}?";

        // Dialogue Options
        List<EncounterOption> options = new List<EncounterOption>();

        FixedOutcomeOption sleepOtion = new FixedOutcomeOption("Sleep", "Go to sleep and hope for a calm night.", Sleep);
        options.Add(sleepOtion);

        EncounterStep eveningEventStep = new EncounterStep(text, options);
        return eveningEventStep;
    }

    private EncounterStep Sleep()
    {
        EndEveningEvent();
        return null;
    }

    public void EndEveningEvent()
    {
        UI.CloseAllWindows();
        SwitchState(GameState.DayTransitionFadeIn);
    }

    #endregion

    #region Game Actions

    public Item CreateItem(ItemDef itemDef)
    {
        Item item = new Item(this, ItemIdCounter++, itemDef);
        item.Renderer.Freeze();
        return item;
    }

    public void AddNewItemToInventory(ItemDef itemDef)
    {
        Item item = CreateItem(itemDef);
        AddExistingItemToInventory(item);
    }

    public void AddExistingItemToInventory(Item item)
    {
        if (item.IsPlayerOwned) throw new System.Exception("Can't add item to inventory that is already player owned.");

        item.Renderer.Show();
        item.SetIsPlayerOwned(true);

        DropItemIntoCart(item);

        ItemsAddedSinceLastStep.Add(item);
        Inventory.Add(item);

        OnGameStateChanged();
    }

    public void DropItemIntoCart(Item item)
    {
        if (item.Renderer.IsRenderingAboveUI) item.Renderer.SetRenderAboveUI(false);
        item.Renderer.SetPosition(Random.Range(-8f, -3f), Random.Range(2f, 4f));
        item.Renderer.SetRandomRotation();
        item.Renderer.Unfreeze();
        item.Renderer.ResetVelocity();
    }

    /// <summary>
    /// Adds multiple items to the player of the same type. Returns a list containing the added items.
    /// </summary>
    public List<Item> AddNewItemsToInventory(ItemDef itemDef, int amount)
    {
        List<Item> addedItems = new List<Item>();
        for (int i = 0; i < amount; i++)
        {
            Item item = CreateItem(itemDef);
            AddExistingItemToInventory(item);
            addedItems.Add(item);
        }
        return addedItems;
    }
    public void DestroyOwnedItem(Item item, bool showOnEventStepDisplay = true)
    {
        if (showOnEventStepDisplay) ItemsRemovedSinceLastStep.Add(item);
        Inventory.Remove(item);
        item.SetIsPlayerOwned(false);
        DestroyItem(item);

        OnGameStateChanged();
    }
    /// <summary>
    /// Destroys multiple items of the player of the same type. Returns a list containing the destroyed items.
    /// </summary>
    public List<Item> DestroyOwnedItems(ItemDef itemDef, int amount, bool showOnEventStepDisplay = true)
    {
        List<Item> destroyedItems = new List<Item>();
        for (int i = 0; i < amount; i++)
        {
            Item item = Inventory.First(x => x.Def == itemDef);
            DestroyOwnedItem(item, showOnEventStepDisplay);
            destroyedItems.Add(item);
        }
        return destroyedItems;
    }

    public void DestroyItem(Item item)
    {
        if (item.IsPlayerOwned) throw new System.Exception("Can't use DestroyItem on player owned item. Use DestroyOwnedItem instead.");
        GameObject.Destroy(item.Renderer.gameObject);
    }

    public void EatItem(Item item)
    {
        if (!item.Def.IsEdible) Debug.LogWarning($"Eating item that is not edible! {item.Label}");
        Player.ModifyNutrition(item.Def.OnEatNutrition);
        Player.ModifyHydration(item.Def.OnEatHydration);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);

        OnGameStateChanged();
    }
    public void DrinkItem(Item item)
    {
        if (!item.Def.IsDrinkable) Debug.LogWarning($"Drinking item that is not drinkable! {item.Label}");
        Player.ModifyHydration(item.Def.OnDrinkHydration);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        OnGameStateChanged();
    }

    public void AddBruiseWound() => AddWound(HealthConditionDefOf.Bruise);
    public void AddCutWound() => AddWound(HealthConditionDefOf.Cut);
    private void AddWound(HealthConditionDef woundDef)
    {
        // Validate
        if (!woundDef.HealthConditionClass.IsSubclassOf(typeof(Wound))) throw new System.Exception("Trying to add wound with health condition def that is not a wound! " + woundDef.Label);

        // Check maximum
        int max = woundDef.MaxAmount;
        int current = Player.GetHealthConditionAmount(woundDef);
        if (current > max)
        {
            Debug.Log($"Can't add wound {woundDef.Label} because player already has maximum amount ({max}).");
            return;
        }

        // Apply
        Wound newWound = Player.AddWound(woundDef);
        WoundsAddedSinceLastStep.Add(newWound);
        OnGameStateChanged();
    }

    public void TendWound(Wound wound, Item item)
    {
        if (!item.Def.CanTendWounds) Debug.LogWarning($"Tending wound with an item that can't tend wounds! {item.Label}");
        if (wound.IsTended) Debug.LogWarning("Tending wound that is already tended.");
        wound.SetHightlighted(false);
        Player.TendWound(wound);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        OnGameStateChanged();
    }

    public void RemoveWound(Wound wound)
    {
        Player.RemoveWound(wound);
    }

    public void HealInfection(Wound wound, Item item)
    {
        if (!item.Def.CanHealInfections) Debug.LogWarning($"Healing infection with an item that can't heal infections! {item.Label}");
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");
        wound.SetHightlighted(false);
        Player.HealInfection(wound);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        OnGameStateChanged();
    }

    public void AddMission(Mission mission)
    {
        Missions.Add(mission.Id, mission);

        if (mission.IsLocationBased) mission.Location.SetMission(mission);

        UI.UpdateMissionDisplay();
    }
    public void RemoveMission(MissionId missionId)
    {
        if (!Missions.ContainsKey(missionId)) return;

        Mission mission = Missions[missionId];
        if (mission.IsLocationBased) mission.Location.SetMission(null);
        Missions.Remove(missionId);

        UI.UpdateMissionDisplay();
    }

    /*
    public void AddDog()
    {
        Player.AddDog();
        Companions.Add(ResourceManager_Old.Singleton.Dog);
        ResourceManager_Old.Singleton.Dog.Init(this);
        UpdatePlayerStats();
    }
    public void RemoveDog()
    {
        Player.RemoveDog();
        Companions.Remove(ResourceManager_Old.Singleton.Dog);
        ResourceManager_Old.Singleton.Dog.gameObject.SetActive(false);
        UpdatePlayerStats();
    }

    public void AddParrot()
    {
        Player.AddParrot();
        Companions.Add(ResourceManager_Old.Singleton.Parrot);
        ResourceManager_Old.Singleton.Parrot.Init(this);
        UpdatePlayerStats();
    }
    public void FeedParrot(Item item, float value)
    {
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        ResourceManager_Old.Singleton.Parrot.AddNutrition(value);
        UpdatePlayerStats();
    }
    public void RemoveParrot()
    {
        Player.RemoveParrot();
        Companions.Remove(ResourceManager_Old.Singleton.Parrot);
        ResourceManager_Old.Singleton.Parrot.gameObject.SetActive(false);
        UpdatePlayerStats();
    }
    */

    public void SetPosition(WorldMapTile tile)
    {
        if (CurrentPosition != null) CurrentPosition.Biome.Visuals.SetActive(false);
        CurrentPosition = tile;
        CurrentPosition.Biome.Visuals.SetActive(true);

        PathHistory.Add(CurrentPosition);

        CheckGameOver();
    }



    private void OnGameStateChanged()
    {
        if (State == GameState.Initializing) return;

        UpdateHealthConditions();
        RefreshVisuals();
    }

    private void UpdateHealthConditions()
    {
        foreach (HealthCondition condition in Player.HealthConditions) condition.OnUpdate();
    }

    /// <summary>
    /// Refreshes all visual elements in the game according to the current game state. This includes both UI and world elements. Should be called after every change to the game state.
    /// </summary>
    private void RefreshVisuals()
    {
        PlayerCharacterRenderer.Instance.UpdateSprites();
        //foreach (Companion c in Companions) c.UpdateStatusEffects();

        UI.UpdateHealthReports();
        UI.RefreshStats();
    }

    #endregion

    #region Getters

    public int GetItemAmount(ItemDef itemDef)
    {
        return Inventory.Count(x => x.Def == itemDef);
    }

    public Item RandomInventoryItem => Inventory.RandomElement();
    public bool IsMissionActive(MissionId id)
    {
        return Missions.ContainsKey(id);
    }

    public static Game Instance;

    public ItemDef GetRandomItemDefWithTag(ItemTagDef tag) => DefDatabase<ItemDef>.AllDefs.Where(x => x.HasTag(tag)).ToList().RandomElement();
    public ItemDef GetRandomItemDef() => DefDatabase<ItemDef>.AllDefs.RandomElement();

    #endregion

    #region UI Feedback

    public void OnTransitionFadeInDone()
    {
        if (State == GameState.DayTransitionFadeIn) SwitchState(GameState.InDayTransition);
        else if (State == GameState.EndEventTransitionIn) SwitchState(GameState.EndEventTransitionOut);
        else if (State == GameState.EndMorningReportTransitionIn) SwitchState(GameState.EndMorningReportTransitionOut);
        else if (State == GameState.GameOver) { } // game ended here
        else throw new System.Exception("State " + State.ToString() + " not handled.");
    }

    public void OnTransitionFadeOutDone()
    {
        if (State == GameState.DayTransitionFadeOut ||
            State == GameState.EndEventTransitionOut ||
            State == GameState.EndMorningReportTransitionOut)
        {
            SwitchState(GameState.InGame);
        }
        else throw new System.Exception("State " + State.ToString() + " not handled.");
    }

    public void OnTransitionHoldDone()
    {
        if (State == GameState.InDayTransition) SwitchState(GameState.DayTransitionFadeOut);
        else throw new System.Exception("State " + State.ToString() + " not handled.");
    }

    #endregion
}
