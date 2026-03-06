using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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

    // Encounter Step Outcome
    public List<Item> ItemsUsedInOption = new List<Item>();

    public List<Item> ItemsAddedSinceLastStep = new List<Item>();
    public List<Item> ItemsRemovedSinceLastStep = new List<Item>();
    public List<Wound> WoundsAddedSinceLastStep = new List<Wound>();
    public Dictionary<StatDef, int> StatChangesSinceLastStep = new Dictionary<StatDef, int>();

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

    [Header("Background")]
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

    #region Game Flow

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

        // Init world
        WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 18, numAdditionalTiles: 400);
        //WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 3, numAdditionalTiles: 20);
        WorldMapCamera.Init(this);
        SetPosition(WorldMap.GetTile(Vector2Int.zero));
        WorldMapRenderer.ResetCamera();

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
        HideAllEncounterSprites();

        SwitchState(GameState.InDayTransition);
    }

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
                StartMorning();
                UI.HoldBlackTransition(GameUI.TRANSITION_HOLD_TIME);
                break;

            case GameState.DayTransitionFadeOut:
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
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEncounterTransitionOut:
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

        UI.CloseAllWindows();
    }

    #region Morning

    private void StartMorning()
    {
        SetTimeOfDay(TimeOfDayDefOf.Morning);
        EncounterCamera.Instance.SetDefaultZoom();

        LatestMorningReport = new MorningReport(Day);

        Day++;

        Player.OnEndDay(this, LatestMorningReport);
        /*
        List<Companion> companionsCopy = new List<Companion>();
        foreach (Companion c in Companions) companionsCopy.Add(c);
        foreach (Companion c in companionsCopy) c.OnEndDay(this, LatestMorningReport);
        */
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
        EndCurrentEncounter();

        // Reset world map selection
        WorldMap.CanSelectDestination = false;
        WorldMapRenderer.UnhighlightAllRedTiles();

        // Switch state
        SwitchState(GameState.EndMorningReportTransitionIn);
    }

    #endregion

    #region Afternoon

    private void StartAfternoonEncounter()
    {
        SetTimeOfDay(TimeOfDayDefOf.Afternoon);

        // Move to selected target position
        if (DayAction == DayAction.Move)
        {
            SetPosition(TargetPosition);
            TargetPosition = null;
        }

        // If the tile already has a location encounter set, just take that.
        if (CurrentPosition.Encounter != null)
        {
            SetCurrentEncounter(CurrentPosition.Encounter);
        }

        // Else generate a new one
        else
        {
            EncounterDef newEncounterDef = EncounterManager.SelectRandomLocationEncounterDefFor(CurrentPosition);
            LocationEncounter newEncounter = SetLocationEncounter(CurrentPosition, newEncounterDef);
            SetCurrentEncounter(newEncounter);
        }
    }

    public LocationEncounter SetLocationEncounter(WorldMapTile tile, EncounterDef encounterDef)
    {
        if (tile.Encounter != null) throw new System.Exception("Trying to set encounter for tile that already has an encounter!");
        if (encounterDef == null) throw new System.Exception("Trying to set null encounter on tile " + tile.Coordinates);

        LocationEncounter encounter = EncounterManager.GenerateEncounter(encounterDef) as LocationEncounter;
        tile.SetEncounter(encounter);
        return encounter;
    }

    public void EndAfternoonEncounter()
    {
        EndCurrentEncounter();
        SwitchState(GameState.EndEncounterTransitionIn);
    }

    #endregion

    #region Evening

    private void StartEveningEncounter()
    {
        SetTimeOfDay(TimeOfDayDefOf.Evening);
        EncounterCamera.Instance.SetDefaultZoom();

        // Start encounter
        Encounter eveningBiomeEncounter = EncounterManager.GenerateEncounter(CurrentPosition.Biome.EveningEncounter) as Encounter;
        SetCurrentEncounter(eveningBiomeEncounter);
    }

    public void EndEveningEncounter()
    {
        EndCurrentEncounter();
        SwitchState(GameState.DayTransitionFadeIn);
    }

    #endregion

    #region Game Actions

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

    public void ModifyStatBaseValue(StatDef stat, int value)
    {
        if (value == 0) return;

        Player.Stats[stat].ModifyBaseValue(value);
        StatChangesSinceLastStep.Increment(stat, value);

        OnGameStateChanged();
    }

    public void DecreaseArmBoneHealth(float value) => Player.ModifyArmBoneHealth(-value);
    public void DecreaseLegBoneHealth(float value) => Player.ModifyLegBoneHealth(-value);
    public void ApplyBruiseDamage() // Adds a bruise wound and some damage to either arm or leg bone health
    {
        AddBruiseWound();

        float damage = Random.Range(0.1f, 0.2f);

        if(Random.value < 0.5f) Player.ModifyArmBoneHealth(-damage);
        else Player.ModifyLegBoneHealth(-damage);
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
        OnGameStateChanged();
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
        // Background
        Background1.sprite = tile.Biome.BackgroundSprite;
        Background2.sprite = tile.Biome.BackgroundSprite;
        Background3.sprite = tile.Biome.BackgroundSprite;

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

    public void ShowPlayerCharacter(bool value) => Player.Renderer.gameObject.SetActive(value);

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
