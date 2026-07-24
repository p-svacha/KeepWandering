using ElectionTactics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : Singleton<Game>
{
    public EncounterManager EncounterManager { get; private set; }

    // Game State
    public GameState State { get; private set; }
    public int Day { get; private set; }
    public TimeOfDayDef TimeOfDay { get; private set; }
    public int ItemIdCounter { get; private set; }
    public MorningReport LatestMorningReport { get; private set; }
    public Encounter CurrentEncounter;
    public EncounterStep CurrentEncounterStep;
    public Camp Camp => Camp.Instance;
    public NightEncounter NightEncounter { get; private set; }

    // Encounter Step Outcome
    public EncounterOption SelectedOption { get; private set; } // Last selected encounter option
    public List<Item> ItemsUsedInSelectedOption {  get; private set; } // All items used in the last selected encounter option (including destroyed items)
    public Item ItemUsedInSelectedOption => ItemsUsedInSelectedOption.FirstOrDefault();

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
    public List<Quest> ActiveQuests;
    public string WinGameReason { get; private set; }

    // Elements
    [Header("Main Elements")]
    public Camera MainCamera;
    public GameUI UI;

    [Header("Encounter")]
    public GameObject EncounterContainer;

    [Header("Background")]
    public GameObject BiomeBackgroundContainer;
    public SpriteRenderer Background0;
    public SpriteRenderer Background1;
    public SpriteRenderer Background2;
    public SpriteRenderer Background3;

    [Header("Items")]
    public Item CurrentHoverItem { get; private set; }
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

        WorldMapRenderer.Init(this);
        SpriteOptionInteractionManager.Init();

        HideAllEncounterSprites();
    }

    public void StartNewGame()
    {
        Program.Instance.EnterState(ProgramState.Game);

        EncounterManager = new EncounterManager(this);

        // Init quests
        QuestStates = new Dictionary<QuestDef, QuestState>();
        foreach (QuestDef questDef in DefDatabase<QuestDef>.AllDefs)
        {
            QuestStates.Add(questDef, global::QuestState.Inactive);
        }
        ActiveQuests = new List<Quest>();

        // Init world
        WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 9, numAdditionalTiles: 60, numCities: 3);
        //WorldMap = WorldMapGenerator.GenerateWorld(zoneRadius: 6, numAdditionalTiles: 50, numCities: 2);
        WorldMapCamera.Init(this);
        WorldMapTile startTile = WorldMap.GetTile(Vector2Int.zero);
        SetPosition(startTile);
        startTile.AddVisit();
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
        HideAllEncounterSprites();
        WorldMapRenderer.gameObject.SetActive(false);

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
        EncounterCamera.Instance.SetAmbienceColor(timeOfDay.LightingAmbienceOverlayColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (Program.Instance.State != ProgramState.Game) return;

        bool uiClick = EventSystem.current.IsPointerOverGameObject();

        // Escape - Escape menu
        if (Input.GetKeyDown(KeyCode.Escape)) UI.ToggleEscapeMenu();

        // M - Map
        if (Input.GetKeyDown(KeyCode.M)) UI.ToggleWorldMap();

        // Update per state
        if (State == GameState.InGame)
        {
            // Update hover state first so sprite interactions can see current hovered item
            if (!ItemDragDropManager.IsDragging)
            {
                UpdateHoveredItem();
            }

            // Manager updates
            ItemDragDropManager.Update();
            SpriteOptionInteractionManager.Update();
            ItemHighlightManager.Update();

            // Handle mouse clicks
            if (!ItemDragDropManager.IsDragging)
            {

                // Left Click -> Start Drag
                if (Input.GetMouseButtonDown(0) && !uiClick)
                {
                    if (CurrentHoverItem != null && ItemDragDropManager.CanDragItem(CurrentHoverItem))
                    {
                        ItemDragDropManager.StartDrag(CurrentHoverItem);
                        CurrentHoverItem.Renderer.Unhighlight();
                        CurrentHoverItem = null;
                        UI.HideAllTooltips();
                    }

                    else if (UI_ContextMenu.Instance.gameObject.activeSelf)
                    {
                        UI_ContextMenu.Instance.Hide();
                        CurrentHoverTime = 0f;
                        CurrentInteractionItem = null;
                    }
                }

                // Right Click -> Context Menu
                if (Input.GetMouseButtonDown(1) && !uiClick)
                {
                    if (CurrentHoverItem != null)
                    {
                        // OnItemRightClicked(CurrentHoverItem);
                    }
                    else if (UI_ContextMenu.Instance.gameObject.activeSelf)
                    {
                        UI_ContextMenu.Instance.Hide();
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
        EncounterCamera.Instance.SetCameraPosition(CurrentEncounter.Def.CameraZoomLevel, CurrentEncounter.Def.CameraXOffset);

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
        bool canInteract = (TimeOfDay == TimeOfDayDefOf.Morning || CurrentEncounterStep.IsFinalStep);
        if (!canInteract) return;

        // Get interaction options for item
        List<InteractionOption> options = new List<InteractionOption>();
        Debug.Log($"Clicked on " + item.Label + " with " + options.Count + " interaction options.");

        // If it has any, show context menu
        if (options.Count > 0)
        {
            Debug.Log($"Show context menu for " + CurrentHoverItem.Label);
            CurrentInteractionItem = CurrentHoverItem;
            UI_ContextMenu.Instance.Show(CurrentHoverItem);
            UI.HideAllTooltips();
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
            // Reset hover time for tooltip
            if (newHoveredItem != null) CurrentHoverTime = 0f; 

            // Hide tooltip
            UI.HideAllTooltips();

            // Update current hovered item
            CurrentHoverItem = newHoveredItem;
        }
        else // Still hovering the same item
        {
            if (CurrentHoverItem != null) // Update tooltip
            {
                CurrentHoverTime += Time.deltaTime;
                if (CurrentHoverTime >= GameUI.TOOLTIP_HOVER_TIME && !UI_ContextMenu.Instance.gameObject.activeSelf)
                {
                    UI_ItemTooltip.Instance.Show(CurrentHoverItem);
                }
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
                UI.HideAllTooltips();
                UI_ContextMenu.Instance.Hide();
                ItemDragDropManager.CancelDrag();
                break;
        }

        State = newState;

        switch (newState)
        {
            case GameState.InDayTransition:
                if (Day > 0) EndDay();
                StartMorning();
                if (IntroSequenceManager.Instance != null && IntroSequenceManager.Instance.IsIntroRunning)
                {
                    State = GameState.InGame;
                }
                else
                {
                    UI.HoldBlackTransition(GameUI.TRANSITION_HOLD_TIME);
                }
                break;

            case GameState.DayTransitionFadeOut:
                EncounterCamera.Instance.StartZoomTransition(new Vector2(0f, 1f), CurrentEncounter.Def.CameraZoomLevel, CurrentEncounter.Def.CameraXOffset, GameUI.TRANSITION_FADE_TIME);
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEncounterTransitionIn:
            case GameState.EndMorningTransitionIn:
                UI.FadeInBlackTransition(GameUI.TRANSITION_FADE_TIME);
                UI.BlackTransitionText.text = "";
                break;

            case GameState.EndMorningTransitionOut:
                UI.CloseWorldMap(); // safe to hide now - screen is fully black at this point
                if (DayAction == DayAction.Rest) StartEveningEncounter(); // Resting skips afternoon
                else StartAfternoonEncounter();
                EncounterCamera.Instance.StartZoomTransition(new Vector2(-1.5f, 0f), CurrentEncounter.Def.CameraZoomLevel, CurrentEncounter.Def.CameraXOffset, GameUI.TRANSITION_FADE_TIME);
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEncounterTransitionOut:
                if (TimeOfDay == TimeOfDayDefOf.Evening) StartNightEncounter();
                else StartEveningEncounter();
                EncounterCamera.Instance.StartZoomTransition(new Vector2(-1.5f, 0f), CurrentEncounter.Def.CameraZoomLevel, CurrentEncounter.Def.CameraXOffset, GameUI.TRANSITION_FADE_TIME);
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
        CurrentEncounterStep = step;
        if (step != null)
        {
            UI.EventStepDisplay.Init(step, prevOutcome);
        }

        // Update world map encounter sprites
        WorldMapRenderer.MarkRedrawEncounterSprites();

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
    /// Called when the player selects an encounter step option.
    /// </summary>
    public void SelectEncounterOption(EncounterOption selectedOption)
    {
        SelectedOption = selectedOption;

        // Captured what items were used in this option
        ItemsUsedInSelectedOption = SelectedOption.ItemSlots.Where(slot => slot.IsFilled).Select(slot => slot.FilledItem).ToList();

        if (selectedOption is SkillCheckOption skillCheckOption)
        {
            // Roll now so the animation knows exactly where to land, but only resolve after the animation is done.
            skillCheckOption.RollOutcome();
            UI.EventStepDisplay.PlaySkillCheckRollSequence(skillCheckOption, () => ResolveSelectedOption(selectedOption));
        }
        else ResolveSelectedOption(selectedOption);
    }

    /// <summary>
    /// Applies the actual effects of the selected option: item slot resolution, Execute() (which
    /// applies the already-rolled outcome for skill checks), and displaying the next step. For skill
    /// checks this only runs after the roll animation completes, so nothing is visible before then.
    /// </summary>
    private void ResolveSelectedOption(EncounterOption selectedOption)
    {
        UI.StatPanel.UnhighlightAll();

        // Empty slots of all non-selected options - return items to cart
        foreach (EncounterOption option in CurrentEncounterStep.Options)
        {
            if (option == selectedOption) continue;
            foreach (ItemSlot slot in option.ItemSlots)
            {
                if (slot.IsFilled) slot.Empty();
            }
        }

        // Handle slots of the selected option
        foreach (ItemSlot slot in selectedOption.ItemSlots)
        {
            if (!slot.IsFilled) continue;

            Item item = slot.FilledItem;

            if (slot.IsDestroyingItem) DestroyOwnedItem(item);
            else
            {
                ReduceItemDurability(item);
                if (!item.IsDestroyed)
                {
                    item.Show();
                    DropItemIntoCart(item);
                }
            }
        }

        // Execute the option
        string nextEncounterStepText = selectedOption.Execute(out OptionOutcomeDef outcome);
        if (CurrentEncounter == null) return; // Option may have ended the encounter

        CurrentEncounter.OnOptionChosen(selectedOption);

        EncounterStep nextEncounterStep = CurrentEncounter.GetNextEncounterStep(nextEncounterStepText);
        if (nextEncounterStepText != null) DisplayEncounterStep(nextEncounterStep, outcome);
    }

    public void ForceUnhighlightAllInventoryItems()
    {
        foreach (Item item in Inventory)
        {
            item.Renderer.Unhighlight();
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
        if (IntroSequenceManager.Instance == null || !IntroSequenceManager.Instance.IsIntroRunning)
        {
            EncounterCamera.Instance.SetDefaultZoom();
        }

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
        foreach (WorldMapTile nextPositionTarget in GetNextPositionTiles()) WorldMapRenderer.HighlightTile(nextPositionTarget);

        // Start encounter
        Encounter morningEncounter = EncounterManager.GenerateEncounter(EncounterDefOf.MorningEncounter, CurrentPosition);
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

        // Road bonus: if start, mid, and end tiles all have roads, a 2-tile move is also available
        if (CurrentPosition.HasRoad)
        {
            foreach (WorldMapTile mid in tiles.Where(t => t.HasRoad).ToList())
            {
                foreach (WorldMapTile end in mid.GetAdjacentTiles())
                {
                    if (end == CurrentPosition) continue;
                    if (!end.HasRoad || !end.IsPassable()) continue;
                    if (!tiles.Contains(end)) tiles.Add(end);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Gets called when a tile is clicked on on the world map to move to.
    /// </summary>
    public void SelectTileOnMap(WorldMapTile tile)
    {
        if (!GetNextPositionTiles().Contains(tile)) return;

        // Play sound
        AudioManager.PlaySound("CartRolling");
        AudioManager.PlaySound("Footsteps");

        // Set target
        SetDayAction(DayAction.Move);
        TargetPosition = tile;
        EndMorning();
    }

    public void EndMorning()
    {
        // Reset world map selection
        WorldMap.CanSelectDestination = false;
        WorldMapRenderer.UnhighlightAllTiles();

        // Close other UI immediately, but keep the world map open through the fade so the player marker
        // can visibly move toward the new tile; the world map itself is closed once the fade-in completes
        // (see SwitchState -> EndMorningTransitionOut).
        UI.CloseAllWindowsExceptWorldMap();

        // Animate player marker toward the target tile while the screen fades to black
        if (DayAction == DayAction.Move && TargetPosition != null)
        {
            WorldMapRenderer.StartMovingPlayerMarkerTo(TargetPosition);
        }

        // Switch state
        SwitchState(GameState.EndMorningTransitionIn);
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

        CurrentPosition.AddVisit();

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

        LocationEncounter encounter = EncounterManager.GenerateEncounter(encounterDef, tile) as LocationEncounter;
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
        Encounter eveningBiomeEncounter = EncounterManager.GenerateEncounter(CurrentPosition.Biome.EveningEncounter, CurrentPosition) as Encounter;
        SetCurrentEncounter(eveningBiomeEncounter);
    }

    public void EndEveningEncounter()
    {
        // UI
        UI.CloseAllWindows();

        // Initialize morning report (things happening from here can be part of the report)
        LatestMorningReport = new MorningReport(Day);

        // Select a night encounter that will happen if a night encounter will be rolled
        EncounterDef selectedNightEncounterDef = EncounterManager.SelectNightEncounterDefFor(CurrentPosition);

        // Decide if there should be a night encounter
        int nightEncounterIntensity = CurrentPosition.GetEffectiveDangerLevel().NightEncounterIntensities.GetWeightedRandomElement();

        // Check if night encounter is negated by camp
        if (Camp.Instance.HasFire && selectedNightEncounterDef.AttackType == AttackType.Wildlife) nightEncounterIntensity = 0;

        // If no night encounter is happening, end the day
        if (nightEncounterIntensity == 0) SwitchState(GameState.DayTransitionFadeIn);

        // Night encounter happening
        else
        {
            // Reduce intensity based on traps
            while (nightEncounterIntensity > 0 && Camp.NumTraps > 0)
            {
                nightEncounterIntensity--;
                Camp.UseTrapToDefendNightAttack();
            }

            // If intensity was reduced to 0, mention in morning report and end day
            if (nightEncounterIntensity == 0)
            {
                string trap = Camp.NumTrapsUsedToDefendNightAttack == 1 ? "trap was" : "traps were";
                LatestMorningReport.AddNightEvent($"{Camp.NumTrapsUsedToDefendNightAttack} {trap} used during the night to successfully defend against an attack.");
                SwitchState(GameState.DayTransitionFadeIn);
            }

            // Else start night encounter with remaining intensity
            else
            {
                NightEncounter = EncounterManager.GenerateEncounter(selectedNightEncounterDef, CurrentPosition) as NightEncounter;
                NightEncounter.Init(this, selectedNightEncounterDef, CurrentPosition, nightEncounterIntensity);
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

        // Apply natural healing
        float naturalHealingFactor = 1f;
        if (IsEarlyResting)
        {
            naturalHealingFactor += 0.5f;
            Debug.Log("Early resting bonus! Natural healing increased by 50% for this night.");
        }
        if (Camp.HasBedroll)
        {
            naturalHealingFactor += 0.5f;
            Debug.Log("Bedroll bonus! Natural healing increased by 50% for this night.");
        }
        ApplyNaturalHealing(naturalHealingFactor);
        IsEarlyResting = false;

        // End of day effects of health conditions
        Player.OnEndDay(this, LatestMorningReport);

        // Increase danger level on current tile
        ModifyDangerLevel(+1);

        // Clean up camp
        Camp.Instance.CleanUpCamp(LatestMorningReport);
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

        int initialDurability = Random.Range(itemDef.MinInitialDurability, itemDef.MaxInitialDurability + 1);
        SetItemDurability(item, initialDurability);

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
    public void AddNewItemsToInventory(List<ItemDef> itemDefs)
    {
        foreach (var itemDef in itemDefs) AddNewItemToInventory(itemDef);
    }

    public void DestroyOwnedItem(Item item, bool showOnEventStepDisplay = true)
    {
        if (item.IsDestroyed) return; // Item already destroyed

        if (showOnEventStepDisplay) ItemsRemovedSinceLastStep.Add(item);
        Inventory.Remove(item);
        item.SetIsPlayerOwned(false);
        DestroyItem(item);

        OnGameStateChanged();
    }
    public void RemoveRandomItemFromInventory()
    {
        if (Inventory.Count == 0) return;
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

        ConsumptionProperties consumptionProps = item.Def.ConsumptionProperties;

        // Consumption effects
        Player.ModifyHunger(-consumptionProps.Nutrition);
        Player.ModifyThirst(-consumptionProps.Hydration);
        Player.ReduceRandomNegativeHcSeverity(consumptionProps.SeverityReduction);
        foreach (var statChange in consumptionProps.StatChanges) ModifyStatBaseValue(statChange.Key, statChange.Value);


        if (consumptionProps.AppliedHealthCondition != null)
        {
            string hcSource = $"Consumed {item.Label}";

            if (consumptionProps.AppliedHealthConditionSeverity > 0f) Player.ApplyHealthCondition(consumptionProps.AppliedHealthCondition, hcSource, consumptionProps.AppliedHealthConditionSeverity);
            else Player.ApplyHealthCondition(consumptionProps.AppliedHealthCondition, hcSource); // Apply with default severity if not specified
        }

        DestroyOwnedItem(item, showOnEventStepDisplay: false);

        OnGameStateChanged();
    }

    public void SetItemDurability(Item item, int durability)
    {
        if (durability < 0) throw new System.ArgumentException("Durability cannot be negative.");
        item.SetDurability(durability);
    }

    public void ReduceItemDurability(Item item, int amount = 1)
    {
        item.ModifyDurability(-amount);
        if (item.Durability <= 0)
        {
            if (item.IsPlayerOwned) DestroyOwnedItem(item);
            else DestroyItem(item);
        }
    }

    public bool PlayerHasItem(ItemDef itemDef) => Inventory.Any(item => item.Def == itemDef);

    #endregion

    public void ModifyDangerLevel(int amount) => ModifyTileDangerLevel(CurrentPosition, amount);
    public void ModifyTileDangerLevel(WorldMapTile tile, int amount)
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

    public void ModifyMorale(int value) => ModifyStatBaseValue(StatDefOf.Morale, value);
    public void ModifyStrength(int value) => ModifyStatBaseValue(StatDefOf.Strength, value);
    public void ModifyDexterity(int value) => ModifyStatBaseValue(StatDefOf.Dexterity, value);
    public void ModifySurvival(int value) => ModifyStatBaseValue(StatDefOf.Survival, value);
    public void ModifySocial(int value) => ModifyStatBaseValue(StatDefOf.Social, value);

    public void ApplyNaturalHealing(float healingFactor = 1f)
    {
        List<HealthCondition> healthConditionsCopy = new List<HealthCondition>(Player.HealthConditions);
        foreach (HealthCondition hc in healthConditionsCopy) hc.ApplyNaturalHealing(healingFactor);
    }

    public HealthCondition ApplyHealthCondition(HealthConditionDef hcDef, string source, float initialSeverity = -1f)
    {
        HealthCondition hc = Player.ApplyHealthCondition(hcDef, source, initialSeverity);
        OnGameStateChanged();
        return hc;
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


    public void ApplyRandomFracture(float severity, string source)
    {
        Player.ApplyRandomFracture(severity, source);
        OnGameStateChanged();
    }
    public void ApplyArmFracture(float severity, string source)
    {
        Player.ApplyArmFracture(severity, source);
        OnGameStateChanged();
    }
    public void ApplyLegFracture(float severity, string source)
    {
        Player.ApplyLegFracture(severity, source);
        OnGameStateChanged();
    }
    public void ApplyBloodLoss(float severity, string source)
    {
        Player.ApplyBloodLoss(severity, source);
        OnGameStateChanged();
    }

    public void ApplyRandomDamage(float severity, string source)
    {
        int damageType = Random.Range(0, 2);
        if (damageType == 0) ApplyBruiseDamage(severity, source);
        else ApplyCutDamage(severity, source);
    }
    public void ApplyBruiseDamage(float fractureSeverity, string source)
    {
        ApplyBruiseWound(source);
        ApplyRandomFracture(fractureSeverity, source);
    }
    public void ApplyCutDamage(float bloodLoss, string source)
    {
        ApplyCutWound(source);
        ApplyBloodLoss(bloodLoss, source);
    }

    public void ApplyRandomWound(string source)
    {
        List<HealthConditionDef> possibleWounds = new List<HealthConditionDef>() { HealthConditionDefOf.Bruise, HealthConditionDefOf.Cut };
        HealthConditionDef selectedWound = possibleWounds.RandomElement();
        AddWound(selectedWound, source);
    }
    public void ApplyBruiseWound(string source) => AddWound(HealthConditionDefOf.Bruise, source);
    public void ApplyCutWound(string source) => AddWound(HealthConditionDefOf.Cut, source);
    private void AddWound(HealthConditionDef woundDef, string source)
    {
        // Validate
        if (!woundDef.HealthConditionClass.IsSubclassOf(typeof(Wound))) throw new System.Exception("Trying to add wound with health condition def that is not a wound! " + woundDef.Label);

        // Apply
        Wound newWound = Player.AddWound(woundDef, source);
        WoundsAddedSinceLastStep.Add(newWound);
        OnGameStateChanged();
    }

    public void BandageWound(Wound wound)
    {
        if (wound.IsBandaged) Debug.LogWarning("Bandaging wound that is already bandaged.");
        wound.SetHightlighted(false);
        Player.BandageWound(wound);
        OnGameStateChanged();
    }

    public void TreatInfection(Wound wound)
    {
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");

        wound.SetHightlighted(false);
        Player.TreatInfection(wound);
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

    public void PlaceEveningTrap(Item trap)
    {
        if (TimeOfDay != TimeOfDayDefOf.Evening) throw new System.Exception("Trying to place trap outside of evening.");
        if (trap.Def != ItemDefOf.Trap) throw new System.Exception("Trying to place item that is not a trap.");

        Inventory.Remove(trap);
        trap.SetIsPlayerOwned(false);
        trap.Hide();

        Camp.AddTrap(trap);
        OnGameStateChanged();
    }

    public void SetUpTent(Item tent)
    {
        if (TimeOfDay != TimeOfDayDefOf.Evening) throw new System.Exception("Trying to set up tent outside of evening.");
        if (Camp.Tent != null) throw new System.Exception("Trying to set up tent when one is already set up.");
        if (tent.Def != ItemDefOf.Tent) throw new System.Exception("Trying to set up item that is not a tent.");

        Inventory.Remove(tent);
        tent.SetIsPlayerOwned(false);
        tent.Hide();
        Camp.SetTent(tent);
        OnGameStateChanged();
    }

    public void SetUpBedroll(Item bedroll)
    {
        if (TimeOfDay != TimeOfDayDefOf.Evening) throw new System.Exception("Trying to set up bedroll outside of evening.");
        if (Camp.Bedroll != null) throw new System.Exception("Trying to set up bedroll when one is already set up.");
        if (bedroll.Def != ItemDefOf.Bedroll) throw new System.Exception("Trying to set up item that is not a bedroll.");

        Inventory.Remove(bedroll);
        bedroll.SetIsPlayerOwned(false);
        bedroll.Hide();
        Camp.SetBedroll(bedroll);
        OnGameStateChanged();
    }

    public void MakeFire()
    {
        if (TimeOfDay != TimeOfDayDefOf.Evening) throw new System.Exception("Trying to make fire outside of evening.");
        if (Camp.HasFire) throw new System.Exception("Trying to make fire when one is already made.");

        Camp.MakeFire();
        OnGameStateChanged();
    }

    public bool HasQuestStarted(QuestDef quest) => QuestStates[quest] != QuestState.Inactive;
    public bool IsQuestActive(QuestDef quest) => QuestStates[quest] == QuestState.Active;
    public bool IsQuestCompleted(QuestDef quest) => QuestStates[quest] == QuestState.Completed || QuestStates[quest] == QuestState.Failed;

    /// <summary>
    /// Starts a new quest from the given QuestDef. The quest text is taken from QuestDef.QuestText (or PartialQuestText if partial is true).
    /// <br/>If the QuestDef has a PlacedEncounterDef and no location is provided, an encounter is automatically placed on a nearby empty tile.
    /// <br/>Returns the created Quest instance, or null if the quest could not be started (e.g. no empty tile for auto-placement).
    /// </summary>
    public Quest StartQuest(QuestDef questDef, WorldMapTile location = null, Area area = null, bool partial = false)
    {
        if (!questDef.IsRepeatable && IsQuestCompleted(questDef))
        {
            throw new System.Exception("Trying to add quest that is already completed! " + questDef.Label);
        }

        // Create quest with text from the QuestDef
        string questText = partial ? questDef.PartialQuestText : questDef.QuestText;
        Quest quest = new Quest(questDef, questText, location, area);

        // Auto-place encounter if QuestDef requires it and no location is specified
        if (questDef.PlacedEncounterDef != null && quest.Location == null)
        {
            WorldMapTile targetTile = GetNearbyEmptyTile(questDef.EncounterPlacementRadius);
            if (targetTile == null)
            {
                Debug.LogWarning("No empty tile found for quest encounter placement.");
                return null;
            }
            SetLocationEncounter(targetTile, questDef.PlacedEncounterDef, showInOutcomeNote: true);
            quest.SetLocation(targetTile);
        }

        // Format quest text with location coordinates or area name if available
        if (quest.Location != null)
            quest.FormatText(quest.Location.Coordinates.ToString());
        else if (quest.Area != null)
            quest.FormatText(quest.Area.Name);

        if (!questDef.IsRepeatable)
        {
            // For non-repeatable quests, replace any existing active instance
            ActiveQuests.RemoveAll(q => q.QuestDef == questDef);
        }

        ActiveQuests.Add(quest);
        QuestStates[questDef] = QuestState.Active;
        NumAddedQuestsSinceLastStep++;

        OnGameStateChanged();
        return quest;
    }
    public void CompleteQuest(QuestDef questDef)
    {
        Quest quest = ActiveQuests.Find(q => q.QuestDef == questDef);
        if (quest != null) CompleteQuest(quest);
    }
    public void CompleteQuest(Quest quest)
    {
        ActiveQuests.Remove(quest);

        if (quest.QuestDef.IsRepeatable)
        {
            // Repeatable quests go back to Inactive when no instances remain
            if (!ActiveQuests.Exists(q => q.QuestDef == quest.QuestDef))
                QuestStates[quest.QuestDef] = QuestState.Inactive;
        }
        else
        {
            QuestStates[quest.QuestDef] = QuestState.Completed;
        }

        NumCompletedQuestsSinceLastStep++;
        OnGameStateChanged();
    }

    /// <summary>
    /// Learns a random rumour from the pool and returns the standardized rumour text to append to the encounter outcome text.
    /// Starts the rumour's quest, which may automatically place an encounter on a nearby tile.
    /// Returns null if no rumour could be learned (e.g. no empty tiles nearby).
    /// </summary>
    public string LearnRumour()
    {
        return LearnRumourInternal(partial: false);
    }

    /// <summary>
    /// Learns a rumour partially. The encounter location is still revealed and a quest is created,
    /// but the player does not know what to expect at the location.
    /// Returns null if no rumour could be learned (e.g. no empty tiles nearby).
    /// </summary>
    public string LearnPartialRumour()
    {
        return LearnRumourInternal(partial: true);
    }

    private string LearnRumourInternal(bool partial)
    {
        // Pick a random rumour
        List<RumourDef> candidates = DefDatabase<RumourDef>.AllDefs;
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No rumour defs available.");
            return null;
        }
        RumourDef rumourDef = candidates.RandomElement();

        // Start quest (auto-placement and text formatting handled by StartQuest)
        Quest quest = StartQuest(rumourDef.QuestDef, partial: partial);
        if (quest == null) return null;

        // Format and return rumour text
        string coordinates = quest.Location?.Coordinates.ToString() ?? "";
        string rumourText = string.Format(partial ? rumourDef.PartialRumourText : rumourDef.RumourText, coordinates);
        return $"\n\nYou learned a rumour: {rumourText} A new quest has been added to your quest log.";
    }

    /// <summary>
    /// Finds a random empty tile within the given hex radius of the player's current position.
    /// Returns null if no empty tile is found.
    /// </summary>
    private WorldMapTile GetNearbyEmptyTile(int maxRadius)
    {
        List<WorldMapTile> nearbyTiles = CurrentPosition.GetTilesInHexRadius(maxRadius);
        List<WorldMapTile> candidateTiles = nearbyTiles.Where(t => !t.HasEncounter && t.Biome.IsPassable).ToList();
        if (candidateTiles.Count == 0) return null;
        return candidateTiles.RandomElement();
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
    public void RefreshUI()
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


    private List<ItemDef> RandomItemPool => DefDatabase<ItemDef>.AllDefs.Where(i => !i.IsQuestItem).ToList();
    public ItemDef GetRandomItemDefWithTag(ItemTagDef tag) => RandomItemPool.Where(x => x.HasTag(tag)).ToList().RandomElement();
    public ItemDef GetRandomItemDef() => RandomItemPool.RandomElement();

    #endregion

    #region UI Feedback

    public void OnTransitionFadeInDone()
    {
        if (State == GameState.DayTransitionFadeIn) SwitchState(GameState.InDayTransition);
        else if (State == GameState.EndEncounterTransitionIn) SwitchState(GameState.EndEncounterTransitionOut);
        else if (State == GameState.EndMorningTransitionIn) SwitchState(GameState.EndMorningTransitionOut);
        else if (State == GameState.GameOver) { } // game ended here
        else throw new System.Exception("State " + State.ToString() + " not handled.");
    }

    public void OnTransitionFadeOutDone()
    {
        if (State == GameState.DayTransitionFadeOut ||
            State == GameState.EndEncounterTransitionOut ||
            State == GameState.EndMorningTransitionOut)
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
