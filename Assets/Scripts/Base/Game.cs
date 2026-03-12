using System.Collections.Generic;
using System.ComponentModel;
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
    public TimeOfDayDef TimeOfDay { get; private set; }
    public int ItemIdCounter { get; private set; }
    public MorningReport LatestMorningReport { get; private set; }
    public Encounter CurrentEncounter;
    public EncounterStep CurrentEventStep;
    public int NumEveningTraps {  get; private set; }
    public NightEncounter NightEncounter { get; private set; }

    // Encounter Step Outcome
    public List<Item> ItemsUsedInOption = new List<Item>();

    public List<Item> ItemsAddedSinceLastStep = new List<Item>();
    public List<Item> ItemsRemovedSinceLastStep = new List<Item>();
    public List<Wound> WoundsAddedSinceLastStep = new List<Wound>();
    public Dictionary<StatDef, int> StatChangesSinceLastStep = new Dictionary<StatDef, int>();
    public int NumRevealedLocationEncountersSinceLastStep = 0;
    public int NumAddedQuestsSinceLastStep = 0;
    public int NumCompletedQuestsSinceLastStep = 0;
    public int NumFailedQuestsSinceLastStep = 0;

    // Position
    public DayAction DayAction { get; private set; } // The type of action the player is doing on the current day.
    public bool IsEarlyResting; // If true, some extra natural healing is applied when going to sleep.
    public List<WorldMapTile> PathHistory = new List<WorldMapTile>();
    public WorldMapTile CurrentPosition { get; private set; } // Position the player is currently at.
    public WorldMapTile TargetPosition { get; private set; } // Position the player is moving towards.
    public bool PlayerIsOnQuarantinePerimeter => QuarantineZone.IsOnPerimeter(CurrentPosition);

    // Quests
    public Dictionary<QuestDef, QuestState> QuestStates;
    public Dictionary<QuestDef, Quest> ActiveQuests;
    public string WinGameReason { get; private set; }

    // Elements
    [Header("Main Elements")]
    public Camera MainCamera;
    public GameUI UI;

    [Header("Background")]
    public SpriteRenderer Background0;
    public SpriteRenderer Background1;
    public SpriteRenderer Background2;
    public SpriteRenderer Background3;
    public SpriteRenderer AmbienceOverlay;

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
    public WorldMapRenderer WorldMapRenderer;
    public WorldMapCameraHandler WorldMapCamera;
    public Area QuarantineZone => WorldMap.QuarantineZone;

    // Debug
    public const bool DEBUG_RANDOM_CHOICES = true;

    #region Initialize

    void Start()
    {
        State = GameState.Initializing;
        Instance = this;

        EncounterContainer = GameObject.Find("Encounters");
        BiomeBackgroundContainer = GameObject.Find("BiomeBackgrounds");

        ResourceManager.ClearCache();
        DefDatabaseRegistry.InitDefs();
        WorldMapRenderer.Init(this);

        StartGame();
    }

    private void StartGame()
    {
        EncounterManager = new EncounterManager(this);

        // Init quests
        QuestStates = new Dictionary<QuestDef, QuestState>();
        foreach (QuestDef questDef in DefDatabase<QuestDef>.AllDefs)
        {
            QuestStates.Add(questDef, global::QuestState.Inactive);
        }
        ActiveQuests = new Dictionary<QuestDef, Quest>();

        // Init world
        WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 10, numAdditionalTiles: 340, numCities: 5);
        //WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 6, numAdditionalTiles: 50, numCities: 2);
        WorldMapCamera.Init(this);
        SetPosition(WorldMap.GetTile(Vector2Int.zero));
        WorldMapRenderer.ResetCamera();

        // Init story
        StoryManager.OnGameStarted();

        // Init player
        PlayerCharacterRenderer.Instance.Init();
        ItemIdCounter = 0;
        Player = new PlayerCharacter(this);

        // Start with 1 food item, 1 drink item, 1 medical item and 1 random item in inventory
        LootTables.Food.AddItemToInventory();
        LootTables.Drinks.AddItemToInventory();
        LootTables.Medical.AddItemToInventory();
        AddNewItemToInventory(GetRandomItemDef());

        // Init UI
        UI.Init(this);
        UI.ContextMenu.Init(this);
        HideAllEncounterSprites();

        SwitchState(GameState.InDayTransition);
    }

    #endregion

    #region Game Flow

    public void SetTimeOfDay(TimeOfDayDef timeOfDay)
    {
        TimeOfDay = timeOfDay;
        UI.DayTimeText.text = timeOfDay.LabelCapWord;

        // Lighting
        EncounterCamera.Instance.SetBackgroundColor(timeOfDay.SkyColor);
        AmbienceOverlay.color = timeOfDay.LightingAmbienceOverlayColor;
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

    private void SetCurrentEncounter(Encounter encounter)
    {
        // Set the encounter
        CurrentEncounter = encounter;

        // Display the encounter
        EncounterStep initialStep = CurrentEncounter.StartEncounter();
        DisplayEncounterStep(initialStep);

        // Set zoom according to encounter
        EncounterCamera.Instance.SetZoom(CurrentEncounter.Def.CameraZoomLevel);

        // Update status
        OnGameStateChanged();
    }

    private void EndCurrentEncounter()
    {
        CurrentEncounter.EndEncounter();
        CurrentEncounter = null;
    }

    private void OnItemRightClicked(Item item)
    {
        // Check if interactions are currently allowed
        bool canInteract = (TimeOfDay == TimeOfDayDefOf.Morning || CurrentEventStep.IsFinalStep);
        if (!canInteract) return;

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
                if (Day > 0) EndDay();
                StartMorning();
                UI.HoldBlackTransition(GameUI.TRANSITION_HOLD_TIME);
                break;

            case GameState.DayTransitionFadeOut:
                EncounterCamera.Instance.StartZoomTransition(new Vector2(0f, 1f), CurrentEncounter.Def.CameraZoomLevel, GameUI.TRANSITION_FADE_TIME);
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEncounterTransitionIn:
            case GameState.EndMorningReportTransitionIn:
                UI.FadeInBlackTransition(GameUI.TRANSITION_FADE_TIME);
                UI.BlackTransitionText.text = "";
                break;

            case GameState.EndMorningReportTransitionOut:
                if (DayAction == DayAction.Rest) StartEveningEncounter(); // Resting skips afternoon
                else StartAfternoonEncounter();
                EncounterCamera.Instance.StartZoomTransition(new Vector2(-1.5f, 0f), CurrentEncounter.Def.CameraZoomLevel, GameUI.TRANSITION_FADE_TIME);
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEncounterTransitionOut:
                if (TimeOfDay == TimeOfDayDefOf.Evening) StartNightEncounter();
                else StartEveningEncounter();
                EncounterCamera.Instance.StartZoomTransition(new Vector2(-1.5f, 0f), CurrentEncounter.Def.CameraZoomLevel, GameUI.TRANSITION_FADE_TIME);
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

    public void HideAllEncounterSprites()
    {
        foreach (SpriteRenderer sprite in EncounterContainer.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.gameObject.SetActive(false);
        }
    }
    public void DisplayEncounterStep(EncounterStep step, OptionOutcomeDef prevOutcome = null)
    {
        // Validate and initialize options
        foreach (EncounterOption option in step.Options)
        {
            option.Init();
        }

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
        StatChangesSinceLastStep.Clear();
        NumRevealedLocationEncountersSinceLastStep = 0;
        NumAddedQuestsSinceLastStep = 0;
        NumCompletedQuestsSinceLastStep = 0;
        NumFailedQuestsSinceLastStep = 0;
    }

    /// <summary>
    /// Called when the player selects an encounter step option. Handles slot item resolution for all options,
    /// then executes the selected option and displays the next step.
    /// </summary>
    public void SelectEncounterOption(EncounterOption selectedOption)
    {
        UI.StatPanel.UnhighlightAll();
        ItemsUsedInOption.Clear();

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
                ItemsUsedInOption.Add(item);
            }
        }

        // Execute the option
        string nextEncounterStepText = selectedOption.Execute(out OptionOutcomeDef outcome);
        if (CurrentEncounter == null) return; // Option may have ended the encounter

        // Inform encounter about chosen option
        CurrentEncounter.OnOptionChosen(selectedOption);

        // Generate and display next step
        EncounterStep nextEncounterStep = CurrentEncounter.GetNextEncounterStep(nextEncounterStepText);
        if (nextEncounterStepText != null) DisplayEncounterStep(nextEncounterStep, outcome); // Can be null on time of day transitions
    }

    public void ForceUnhighlightAllInventoryItems()
    {
        foreach (Item item in Inventory)
        {
            item.Renderer.Unhighlight(removeForced: true);
        }
    }

    public void WinGame(string text)
    {
        WinGameReason = text;
        OnGameStateChanged();
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
        if (WinGameReason != "") return WinGameReason;
        if (!QuarantineZone.ContainsTile(CurrentPosition)) return "You escaped the quarantine.\nYou win.";
        return null;
    }

    #endregion



    #region Morning

    private void StartMorning()
    {
        SetTimeOfDay(TimeOfDayDefOf.Morning);
        EncounterCamera.Instance.SetDefaultZoom();

        // Start next day
        SetBackground(CurrentPosition.Biome.BackgroundSprite); // Reset background
        Day++;
        Debug.Log($"--- Start Day {Day} ---");

        OnGameStateChanged();

        // Day UI Updates
        UI.BlackTransitionText.text = "Day " + Day;
        UI.DayText.text = "Day " + Day;

        // Enable destination selection of adjacent tiles
        WorldMap.CanSelectDestination = true;
        foreach (WorldMapTile nextPositionTarget in GetNextPositionTiles()) WorldMapRenderer.HighlightTileRed(nextPositionTarget);

        // Start encounter
        Encounter morningEncounter = EncounterManager.GenerateEncounter(EncounterDefOf.MorningEncounter);
        SetCurrentEncounter(morningEncounter);
    }

    public void SetDayAction(DayAction dayAction) => DayAction = dayAction;

    /// <summary>
    /// Returns all tiles the player can select when chosing what to do in the morning.
    /// </summary>
    public List<WorldMapTile> GetNextPositionTiles()
    {
        List<WorldMapTile> tiles = new List<WorldMapTile>();
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

        SetDayAction(DayAction.Move);
        TargetPosition = tile;
        EndMorning();
    }

    public void EndMorning()
    {
        // Reset world map selection
        WorldMap.CanSelectDestination = false;
        WorldMapRenderer.UnhighlightAllRedTiles();

        // UI
        UI.CloseAllWindows();

        // Switch state
        SwitchState(GameState.EndMorningReportTransitionIn);
    }

    #endregion

    #region Afternoon

    private void StartAfternoonEncounter()
    {
        // End previous encounter
        EndCurrentEncounter();

        // Set time of day
        SetTimeOfDay(TimeOfDayDefOf.Afternoon);

        // Move to selected target position
        if (DayAction == DayAction.Move)
        {
            SetPosition(TargetPosition);
            TargetPosition = null;
        }

        // If the tile already has a location encounter set, just take that and reveal it.
        if (CurrentPosition.Encounter != null)
        {
            RevealEncounter(CurrentPosition, showInOutcomeNote: false);
            SetCurrentEncounter(CurrentPosition.Encounter);
        }

        // Else generate a new one
        else
        {
            EncounterDef newEncounterDef = EncounterManager.SelectLocationEncounterDefFor(CurrentPosition);
            LocationEncounter newEncounter = SetLocationEncounter(CurrentPosition, newEncounterDef);
            SetCurrentEncounter(newEncounter);
        }
    }

    /// <summary>
    /// Sets the encounter on a tile according to the given def. Does not start the encounter.
    /// </summary>
    public LocationEncounter SetLocationEncounter(WorldMapTile tile, EncounterDef encounterDef, bool showInOutcomeNote = false, bool hidden = false)
    {
        if (tile.Encounter != null) throw new System.Exception("Trying to set encounter for tile that already has an encounter!");
        if (encounterDef == null) throw new System.Exception("Trying to set null encounter on tile " + tile.Coordinates);

        LocationEncounter encounter = EncounterManager.GenerateEncounter(encounterDef) as LocationEncounter;
        encounter.Init(this, encounterDef, tile);
        if (!hidden) RevealEncounter(tile, showInOutcomeNote);

        Debug.Log($"Set encounter {encounter.Def.Label} on tile {tile.Coordinates}");
        return encounter;
    }

    public void EndAfternoonEncounter()
    {
        // UI
        UI.CloseAllWindows();

        SwitchState(GameState.EndEncounterTransitionIn);
    }

    #endregion

    #region Evening

    private void StartEveningEncounter()
    {
        // End previous encounter
        EndCurrentEncounter();

        // If player rested, apply healing
        if (DayAction == DayAction.Rest) ApplyNaturalHealing();

        // Time of Day
        SetTimeOfDay(TimeOfDayDefOf.Evening);

        // Start encounter
        Encounter eveningBiomeEncounter = EncounterManager.GenerateEncounter(CurrentPosition.Biome.EveningEncounter) as Encounter;
        SetCurrentEncounter(eveningBiomeEncounter);
    }

    public void EndEveningEncounter()
    {
        // UI
        UI.CloseAllWindows();

        // Initialize morning report (things happening from here can be part of the report)
        LatestMorningReport = new MorningReport(Day);

        // Decide if there should be a night encounter
        int nightEncounterIntensity = CurrentPosition.DangerLevel.NightEncounterIntensities.GetWeightedRandomElement();
        if (nightEncounterIntensity == 0)
        {
            // No night encounter happening -> End day
            SwitchState(GameState.DayTransitionFadeIn);
        }

        // Night encounter happening
        else
        {
            // Reduce intensity based on traps
            int numTrapsUsedToDefend = 0;
            while (nightEncounterIntensity > 0 && NumEveningTraps > 0)
            {
                nightEncounterIntensity--;
                NumEveningTraps--;
                numTrapsUsedToDefend++;
            }

            // If intensity was reduced to 0, mention in morning report and end day
            if (nightEncounterIntensity == 0)
            {
                string trap = numTrapsUsedToDefend == 1 ? "trap was" : "traps were";
                LatestMorningReport.AddNightEvent($"{numTrapsUsedToDefend} {trap} used during the night to successfully defend against an attack.");
                SwitchState(GameState.DayTransitionFadeIn);
            }

            // Else start night encounter with remaining intensity
            else
            {
                EncounterDef def = EncounterManager.SelectNightEncounterDefFor(CurrentPosition);
                NightEncounter = EncounterManager.GenerateEncounter(def) as NightEncounter;
                NightEncounter.Init(this, def, nightEncounterIntensity);
                SwitchState(GameState.EndEncounterTransitionIn);
            }
        }
    }

    #endregion

    #region Night

    private void StartNightEncounter()
    {
        // End previous encounter
        EndCurrentEncounter();

        // Time of Day
        SetTimeOfDay(TimeOfDayDefOf.Night);

        // Start encounter
        SetCurrentEncounter(NightEncounter);
    }

    public void EndNightEncounter()
    {
        // UI
        UI.CloseAllWindows();

        // Start day transition 
        SwitchState(GameState.DayTransitionFadeIn);
    }

    public void EndDay()
    {
        // End previous encounter
        EndCurrentEncounter();
        NightEncounter = null;

        // Trigger remaining traps
        int numTriggeredTraps = 0;
        for(int i = 0; i < NumEveningTraps; i++)
        {
            // Chance for triggering on wildlife
            bool triggeredOnWildlife = Random.value < CurrentPosition.Biome.TrapTriggerChance;
            if (triggeredOnWildlife)
            {
                ItemDef item = LootTables.TrapLoot.Resolve();
                LatestMorningReport.AddNightEvent($"A trap was triggered during the night. You found {item.Label}.");
                numTriggeredTraps++;
                continue;
            }

            // Chance for breaking
            float breakChance = 0.2f;
            if (Random.value < breakChance)
            {
                LatestMorningReport.AddNightEvent($"A trap was triggered during the night but didn't catch anything.");
                numTriggeredTraps++;
                continue;
            }
        }

        // Add remaining traps to inventory
        int remainingTraps = NumEveningTraps - numTriggeredTraps;
        AddNewItemsToInventory(ItemDefOf.Trap, remainingTraps);
        string trap = remainingTraps == 1 ? "trap was" : "traps were";
        LatestMorningReport.AddNightEvent($"{remainingTraps} {trap} set during the evening were not triggered. You collect them.");

        NumEveningTraps = 0;

        // Apply natural healing
        float naturalHealingFactor = 1f;
        if (IsEarlyResting)
        {
            naturalHealingFactor += 0.5f;
            Debug.Log("Early resting bonus! Natural healing increased by 50% for this night.");
        }
        ApplyNaturalHealing(naturalHealingFactor);
        IsEarlyResting = false;

        // End of day effects of health conditions
        Player.OnEndDay(this, LatestMorningReport);

        /*
        List<Companion> companionsCopy = new List<Companion>();
        foreach (Companion c in Companions) companionsCopy.Add(c);
        foreach (Companion c in companionsCopy) c.OnEndDay(this, LatestMorningReport);
        */

        // Increase danger level on current tile
        ModifyDangerLevel(CurrentPosition, +1);
    }

    #endregion

    #region Game Actions

    #region Items

    public Item CreateItem(ItemTagDef itemTag, bool hidden = false, bool frozen = true)
    {
        ItemDef itemDef = GetRandomItemDefWithTag(itemTag);
        return CreateItem(itemDef, hidden, frozen);
    }
    public Item CreateItem(ItemDef itemDef, bool hidden = false, bool frozen = true)
    {
        Item item = new Item(this, ItemIdCounter++, itemDef);
        if (hidden) item.Renderer.Hide();
        if (frozen) item.Renderer.Freeze();
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
    public void RemoveRandomItemFromInventory()
    {
        Item item = Inventory.RandomElement();
        DestroyOwnedItem(item);
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
        item.Destroy();
    }

    public void ConsumeItem(Item item)
    {
        if(!item.Def.IsConsumable) Debug.LogWarning($"Consuming item that is not edible! {item.Label}");

        Player.ModifyHunger(-item.Def.OnConsumptionNutrition);
        Player.ModifyThirst(-item.Def.OnConsumptionHydration);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);

        OnGameStateChanged();
    }

    #endregion

    public void ModifyDangerLevel(WorldMapTile tile, int amount)
    {
        tile.ModifyDangerLevel(amount);
        OnGameStateChanged();
    }

    public void ModifyRandomStat(int minModifyAmount, int maxModifyAmount, params StatDef[] possibleStats)
    {
        int value = Random.Range(minModifyAmount, maxModifyAmount + 1);
        ModifyRandomStat(value, possibleStats);
    }
    public void ModifyRandomStat(int value, params StatDef[] possibleStats)
    {
        StatDef stat = possibleStats.ToList().RandomElement();
        ModifyStatBaseValue(stat, value);
    }
    public void ModifyStatBaseValue(StatDef stat, int value)
    {
        if (value == 0) return;

        Player.Stats[stat].ModifyBaseValue(value);
        StatChangesSinceLastStep.Increment(stat, value);

        OnGameStateChanged();
    }

    public void ApplyNaturalHealing(float healingFactor = 1f)
    {
        List<HealthCondition> healthConditionsCopy = new List<HealthCondition>(Player.HealthConditions);
        foreach (HealthCondition hc in healthConditionsCopy) hc.ApplyNaturalHealing(healingFactor);
    }

    public void ModifyHunger(float value)
    {
        Player.ModifyHunger(value);
        OnGameStateChanged();
    }
    public void ModifyThirst(float value)
    {
        Player.ModifyThirst(value);
        OnGameStateChanged();
    }


    public void ApplyRandomFracture(float severity)
    {
        Player.ApplyRandomFracture(severity);
        OnGameStateChanged();
    }
    public void ApplyArmFracture(float severity)
    {
        Player.ApplyArmFracture(severity);
        OnGameStateChanged();
    }
    public void ApplyLegFracture(float severity)
    {
        Player.ApplyLegFracture(severity);
        OnGameStateChanged();
    }
    public void ApplyBloodLoss(float severity)
    {
        Player.ApplyBloodLoss(severity);
        OnGameStateChanged();
    }

    public void ApplyBruiseDamage(float fractureSeverity)
    {
        ApplyBruiseWound();
        ApplyRandomFracture(fractureSeverity);
    }
    public void ApplyCutDamage(float bloodLoss)
    {
        ApplyCutWound();
        ApplyBloodLoss(bloodLoss);
    }

    public void ApplyRandomWound()
    {
        List<HealthConditionDef> possibleWounds = new List<HealthConditionDef>() { HealthConditionDefOf.Bruise, HealthConditionDefOf.Cut };
        HealthConditionDef selectedWound = possibleWounds.RandomElement();
        AddWound(selectedWound);
    }
    public void ApplyBruiseWound() => AddWound(HealthConditionDefOf.Bruise);
    public void ApplyCutWound() => AddWound(HealthConditionDefOf.Cut);
    private void AddWound(HealthConditionDef woundDef)
    {
        // Validate
        if (!woundDef.HealthConditionClass.IsSubclassOf(typeof(Wound))) throw new System.Exception("Trying to add wound with health condition def that is not a wound! " + woundDef.Label);

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

    public void TreatWound(Wound wound, Item item)
    {
        if (!item.Def.CanHealInfections) Debug.LogWarning($"Healing infection with an item that can't heal infections! {item.Label}");
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");

        wound.SetHightlighted(false);
        Player.TreatWound(wound);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        OnGameStateChanged();
    }

    public void RevealEncounter(WorldMapTile tile, bool showInOutcomeNote)
    {
        // Tile is already revealed, do nothing
        if (tile.Encounter != null && !tile.Encounter.IsHidden) return;

        // If tile does not have encounter yet, generate one
        if (tile.Encounter == null)
        {
            EncounterDef newEncounterDef = EncounterManager.SelectLocationEncounterDefFor(tile);
            SetLocationEncounter(tile, newEncounterDef, showInOutcomeNote);
        }

        // Reveal
        else
        {
            tile.Encounter.Reveal();
            if (showInOutcomeNote) NumRevealedLocationEncountersSinceLastStep++;
        }
    }

    public void RevealLocationEncountersAround(WorldMapTile tile)
    {
        List<WorldMapTile> adjacentTiles = tile.GetAdjacentTiles();
        foreach (WorldMapTile adjTile in adjacentTiles) RevealEncounter(adjTile, showInOutcomeNote: true);
    }
    public void RevealRandomNearbyLocationEncounter()
    {
        int maxRadius = 3;
        List<WorldMapTile> nearbyTiles = CurrentPosition.GetTilesInHexRadius(maxRadius);
        List<WorldMapTile> candidateTiles = nearbyTiles.Where(t => !t.HasEncounter).ToList();
        WorldMapTile chosenTile = candidateTiles.RandomElement();
        RevealEncounter(chosenTile, showInOutcomeNote: true);
    }

    public void PlaceEveningTrap()
    {
        NumEveningTraps++;
        OnGameStateChanged();
    }

    public bool HasQuestStarted(QuestDef quest) => QuestStates[quest] != QuestState.Inactive;
    public bool IsQuestActive(QuestDef quest) => QuestStates[quest] == QuestState.Active;
    public bool IsQuestCompleted(QuestDef quest) => QuestStates[quest] == QuestState.Completed || QuestStates[quest] == QuestState.Failed;
    public void StartQuest(Quest quest)
    {
        if (IsQuestCompleted(quest.QuestDef))
        {
            Debug.LogWarning("Trying to add quest that is already completed! " + quest.QuestDef.Label);
            return;
        }

        ActiveQuests[quest.QuestDef] = quest; // This also replaces the quest if it's already active, which is useful for updating the quest text or other properties without having to remove and re-add the quest.
        QuestStates[quest.QuestDef] = QuestState.Active;
        NumAddedQuestsSinceLastStep++;

        OnGameStateChanged();
    }
    public void CompleteQuest(QuestDef quest)
    {
        QuestStates[quest] = QuestState.Completed;

        if (ActiveQuests.ContainsKey(quest)) ActiveQuests.Remove(quest);

        OnGameStateChanged();
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
        // Background
        SetBackground(tile.Biome.BackgroundSprite);

        if (CurrentPosition != null) CurrentPosition.Biome.Visuals.SetActive(false);
        CurrentPosition = tile;
        CurrentPosition.Biome.Visuals.SetActive(true);

        PathHistory.Add(CurrentPosition);

        CheckGameOver();
    }

    public void SetBackground(Sprite sprite)
    {
        Background0.sprite = sprite;
        Background1.sprite = sprite;
        Background2.sprite = sprite;
        Background3.sprite = sprite;
    }

    private void OnGameStateChanged()
    {
        if (State == GameState.Initializing) return;

        RefreshUI();
        CheckGameOver();
    }

    /// <summary>
    /// Refreshes all UI elements.
    /// </summary>
    private void RefreshUI()
    {
        UI.UpdateDayPanel();
        UI.UpdateHealthReports();
        UI.RefreshStats();
        UI.UpdateQuestDisplay();
    }

    public void ShowPlayerCharacter(bool value) => Player.Renderer.gameObject.SetActive(value);

    #endregion

    #region Getters

    public int GetItemAmount(ItemDef itemDef)
    {
        return Inventory.Count(x => x.Def == itemDef);
    }

    public Item RandomInventoryItem => Inventory.RandomElement();

    public static Game Instance;

    private List<ItemDef> RandomItemPool => DefDatabase<ItemDef>.AllDefs.Where(i => !i.IsQuestItem).ToList();
    public ItemDef GetRandomItemDefWithTag(ItemTagDef tag) => RandomItemPool.Where(x => x.HasTag(tag)).ToList().RandomElement();
    public ItemDef GetRandomItemDef() => RandomItemPool.RandomElement();

    #endregion

    #region UI Feedback

    public void OnTransitionFadeInDone()
    {
        if (State == GameState.DayTransitionFadeIn) SwitchState(GameState.InDayTransition);
        else if (State == GameState.EndEncounterTransitionIn) SwitchState(GameState.EndEncounterTransitionOut);
        else if (State == GameState.EndMorningReportTransitionIn) SwitchState(GameState.EndMorningReportTransitionOut);
        else if (State == GameState.GameOver) { } // game ended here
        else throw new System.Exception("State " + State.ToString() + " not handled.");
    }

    public void OnTransitionFadeOutDone()
    {
        if (State == GameState.DayTransitionFadeOut ||
            State == GameState.EndEncounterTransitionOut ||
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
