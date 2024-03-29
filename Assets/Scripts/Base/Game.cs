using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public EventManager EventManager { get; private set; }

    // Game State
    public GameState State { get; private set; }
    public int Day { get; private set; }
    public MorningReport LatestMorningReport { get; private set; }
    public Event CurrentEvent;
    public EventStep CurrentEventStep;

    // Event Step Outcome
    public List<Item> ItemsAddedSinceLastStep = new List<Item>();
    public List<Item> ItemsRemovedSinceLastStep = new List<Item>();
    public List<Injury> InjuriesAddedSinceLastStep = new List<Injury>();

    // Position
    /// <summary>
    /// The type of action the player is doing on the current day.
    /// </summary>
    public DayAction DayAction { get; private set; }
    public List<WorldMapTile> PathHistory = new List<WorldMapTile>();
    /// <summary>
    /// Position the player is currently at.
    /// </summary>
    public WorldMapTile CurrentPosition { get; private set; }
    /// <summary>
    /// Position the player is moving towards.
    /// </summary>
    public WorldMapTile TargetPosition { get; private set; }
    public bool PlayerIsOnQuarantinePerimeter => QuarantineZone.IsOnPerimeter(CurrentPosition);

    // Stats
    public Dictionary<StatId, Stat> Stats { get; private set; }

    // Missions
    public Dictionary<MissionId, Mission> Missions = new Dictionary<MissionId, Mission>();

    // Elements
    [Header("Main Elements")]
    public Camera MainCamera;
    public GameUI UI;

    [Header("Items")]
    public List<Item> ItemPrefabs;
    private Item CurrentHoverItem;
    private float CurrentHoverTime;
    private Item CurrentInteractionItem;
    public List<Item> Inventory = new List<Item>();

    [Header("Characters")]
    public PlayerCharacter Player;
    public List<Companion> Companions = new List<Companion>();

    [Header("World Map")]
    public WorldMap WorldMap;
    public CameraHandler WorldMapCamera;
    public Area QuarantineZone => WorldMap.QuarantineZone;

    // Debug
    public const bool DEBUG_RANDOM_CHOICES = true;

    // Rules
    private static LootTable StartItemTable = new LootTable(
        new(ItemType.Beans, 8),
        new(ItemType.WaterBottle, 8),
        new(ItemType.Bandage, 5),
        new(ItemType.Bone, 3),
        new(ItemType.Knife, 2),
        new(ItemType.Antibiotics, 2)
    );

    #region Game Flow

    void Start()
    {
        Singleton = this;
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
        EventManager = new EventManager(this);

        // Init stats
        Stats = new Dictionary<StatId, Stat>();
        Stats.Add(StatId.Moving, new Stat_Moving(this));
        Stats.Add(StatId.Fighting, new Stat_Fighting(this));
        Stats.Add(StatId.Dexterity, new Stat_Dexterity(this));
        Stats.Add(StatId.Charisma, new Stat_Charisma(this));

        // Init UI
        UI.Init(this);
        UI.ContextMenu.Init(this);

        // Init player
        Player.Init(this);

        AddItemToInventory(GetItemInstance(ItemType.Beans));
        AddItemToInventory(GetItemInstance(ItemType.WaterBottle));
        StartItemTable.AddItemToInventory();

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
        switch (State) {

            case GameState.InGame:

                // Update Hovered Elements
                Vector2 mouseWorldPos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                if (hit.collider != null && hit.collider.GetComponent<Item>() != null)
                {
                    if (hit.collider.GetComponent<Item>() != CurrentHoverItem)
                    {
                        if(CurrentHoverItem != null && !CurrentHoverItem.ForceGlow) CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        CurrentHoverItem = hit.collider.GetComponent<Item>();
                        CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 1);
                        CurrentHoverTime = 0f;

                        UI.Tooltip.Hide();
                    }
                    else
                    {
                        CurrentHoverTime += Time.deltaTime;
                        if (CurrentHoverTime >= GameUI.TOOLTIP_HOVER_TIME && !UI.ContextMenu.gameObject.activeSelf) 
                            UI.Tooltip.Show(CurrentHoverItem);
                    }
                }
                else
                {
                    if (CurrentHoverItem != null)
                    {
                        if(!CurrentHoverItem.ForceGlow) CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        CurrentHoverItem = null;
                        UI.Tooltip.Hide();
                    }
                }

                // Click -> Interact
                if (Input.GetMouseButtonDown(0) && !uiClick)
                {
                    if (UI.ContextMenu.gameObject.activeSelf && (CurrentHoverItem == null || CurrentInteractionItem == CurrentHoverItem))
                    {
                        CurrentInteractionItem = null;
                        UI.ContextMenu.Hide();
                        CurrentHoverTime = 0f;
                    }
                    else if (CurrentHoverItem != null && CurrentHoverItem.CanInteract)
                    {
                        CurrentInteractionItem = CurrentHoverItem;
                        UI.ContextMenu.Show(CurrentHoverItem);
                        UI.Tooltip.Hide();
                    }
                }
                break;
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
                break;
        }

        switch(newState)
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
                StartAfternoonEvent();
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEventTransitionOut:
                StartEveningEvent();
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

        State = newState;

        if(State != GameState.GameOver) CheckGameOver();
    }

    public void DisplayEventStep(EventStep step)
    {
        if (CurrentEventStep != null)
        {
            foreach (EventItemOption eventItemOption in CurrentEventStep.EventItemOptions)
            {
                foreach (Item item in Inventory)
                {
                    if (item.Type == eventItemOption.RequiredItemType)
                    {
                        item.ForceGlow = false;
                        item.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        item.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", Color.white);
                    }
                }
            }
        }

        CurrentEventStep = step;
        if (step != null)
        {
            UI.EventStepDisplay.Init(step);
            foreach (EventItemOption eventItemOption in CurrentEventStep.EventItemOptions)
            {
                foreach (Item item in Inventory)
                {
                    if (item.Type == eventItemOption.RequiredItemType)
                    {
                        item.ForceGlow = true;
                        item.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 1);
                        item.GetComponent<SpriteRenderer>().material.SetColor("_GlowColor", Color.red);
                    }
                }
            }
        }

        ItemsAddedSinceLastStep.Clear();
        ItemsRemovedSinceLastStep.Clear();
        InjuriesAddedSinceLastStep.Clear();
    }

    public void CheckGameOver()
    {
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
        if (Player.Nutrition <= 0f) return "You starved";
        if (Player.Hydration <= 0f) return "You died of dehydration";
        if (Player.BoneHealth <= 0f) return "You died due to extreme fractures";
        if (Player.BloodAmount <= 0f) return "You bled out";
        if (Player.PoisonCountdown <= 0) return "You died of poisoning";
        if (Player.ActiveWounds.Any(x => x.InfectionStage == InfectionStage.Fatal)) return "You died of an infection";

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
        List<Companion> companionsCopy = new List<Companion>();
        foreach (Companion c in Companions) companionsCopy.Add(c);
        foreach (Companion c in companionsCopy) c.OnEndDay(this, LatestMorningReport);
        UpdatePlayerStats();

        // Show morning report
        UpdateMorningEvent();

        // Day UI Updates
        UI.BlackTransitionText.text = "Day " + Day;
        UI.DayText.text = "Day " + Day;

        // Enable destination selection of adjacent tiles
        WorldMap.CanSelectDestination = true;
        foreach(WorldMapTile nextPositionTarget in GetNextPositionTiles()) WorldMap.HighlightTileRed(nextPositionTarget);
    }

    /// <summary>
    /// Displays the morning event step.
    /// </summary>
    private void UpdateMorningEvent()
    {
        DisplayEventStep(GetMorningEvent());
    }

    /// <summary>
    /// Creates the morning report event step that contains all information about what happened during the night and the options of what to do that day.
    /// </summary>
    private EventStep GetMorningEvent()
    {
        // Text displaying night events
        string text = "";
        if (Day == 1) text = "After you saw the news you knew that you have to get out of the quarantine zone. You ran outside, grabbed your handcart and so starts your journey.";
        else if (LatestMorningReport.NightEvents.Count == 0) text = "You wake after an uneventful night.";
        else
        {
            text = "You wake up in the " + CurrentPosition.Location.Name + ". The following happened during the night:";
            foreach (string e in LatestMorningReport.NightEvents) text += "\n- " + e;
        }

        // Dialogue Option - Open Map
        List<EventDialogueOption> options = new List<EventDialogueOption>();
        EventDialogueOption startTravelingOption = new EventDialogueOption("Make plans for the day", OpenMap);
        options.Add(startTravelingOption);

        EventStep morningEventStep = new EventStep(text, options, null);
        return morningEventStep;
    }

    private EventStep OpenMap()
    {
        UI.OpenWorldMap();
        return GetMorningEvent();
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

        List<InteractionOption> options = new List<InteractionOption>();

        // Stay
        if (tile == CurrentPosition) options.Add(new InteractionOption("Stay and explore", () => SelectDayAction(tile, DayAction.Stay)));

        // Go there
        if (tile != CurrentPosition && WorldMap.QuarantineZone.IsInArea(tile)) options.Add(new InteractionOption("Go there", () => SelectDayAction(tile, DayAction.Move)));

        // Enter mission event
        if (tile.Mission != null) options.Add(new InteractionOption(tile.Mission.MapText, () => SelectDayAction(tile, DayAction.EnterMission)));

        // Approach fence
        if (!WorldMap.QuarantineZone.IsInArea(tile)) options.Add(new InteractionOption("Approach fence", () => SelectDayAction(tile, DayAction.ApproachFence)));

        UI.ContextMenu.Show(tile.Location.Name, options);
    }

    /// <summary>
    /// Selects where to go and the type of action for that day. Ends the morning event.
    /// </summary>
    private void SelectDayAction(WorldMapTile tile, DayAction action)
    {
        // Set position
        TargetPosition = tile;

        // Set action
        DayAction = action;

        // End morning event
        EndMorningEvent();
    }

    public EventStep EndMorningEvent()
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

    private void StartAfternoonEvent()
    {
        UI.DayTimeText.text = "Afternoon";

        switch(DayAction)
        {
            case DayAction.Move:
                SetPosition(TargetPosition);
                TargetPosition = null;
                break;

            case DayAction.EnterMission:
                EventManager.ForceMission(TargetPosition.Mission);
                if (TargetPosition != null)
                {
                    SetPosition(TargetPosition);
                    TargetPosition = null;
                }
                break;

            case DayAction.ApproachFence:
                EventManager.ForceEvent(eventId: 10);
                break;
        }

        // Chose an event for the afternoon
        CurrentEvent = EventManager.GetAfternoonEvent();
        EventManager.UpdateDaysSinceLastOccurence(CurrentEvent);

        // Display the event
        CurrentEvent.StartEvent();
        DisplayEventStep(CurrentEvent.InitialStep);

        // Update status
        UpdatePlayerStats();
    }

    public void EndAfternoonEvent()
    {
        UI.CloseAllWindows();
        SwitchState(GameState.EndEventTransitionIn);
    }

    #endregion

    #region Evening

    private void StartEveningEvent()
    {
        UI.DayTimeText.text = "Evening";

        // Clear day event
        if (CurrentEvent != null) CurrentEvent.EndEvent();
        CurrentEvent = null;

        // Display evening event
        DisplayEventStep(GetEveningEvent());
    }

    /// <summary>
    /// Creates the morning report event step that contains all information about what happened during the night and the options of what to do that day.
    /// </summary>
    private EventStep GetEveningEvent()
    {
        // Text displaying night events
        string text = "You arrived at a good camp spot in " + CurrentPosition.Location.Name + " just as planned. You are tired and can't wait to get some rest.";

        // Dialogue Options
        List<EventDialogueOption> options = new List<EventDialogueOption>();

        EventDialogueOption sleepOtion = new EventDialogueOption("Sleep", Sleep);
        options.Add(sleepOtion);

        EventStep eveningEventStep = new EventStep(text, options, null);
        return eveningEventStep;
    }

    private EventStep Sleep()
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

    public void AddItemToInventory(Item item)
    {
        item.Show();
        item.IsPlayerOwned = true;

        item.transform.position = new Vector3(Random.Range(-8f, -3f), Random.Range(2f, 4f), 0f);
        item.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        item.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        ItemsAddedSinceLastStep.Add(item);
        Inventory.Add(item);

        UpdatePlayerStats();
    }
    /// <summary>
    /// Adds multiple items to the player of the same type. Returns a list containing the added items.
    /// </summary>
    public List<Item> AddItemsToInventory(ItemType type, int amount)
    {
        List<Item> addedItems = new List<Item>();
        for(int i = 0; i < amount; i++)
        {
            Item item = GetItemInstance(type);
            AddItemToInventory(item);
            addedItems.Add(item);
        }
        return addedItems;
    }
    public void DestroyOwnedItem(Item item, bool showOnEventStepDisplay = true)
    {
        if(showOnEventStepDisplay) ItemsRemovedSinceLastStep.Add(item);
        Inventory.Remove(item);
        Destroy(item.gameObject);

        UpdatePlayerStats();
    }
    /// <summary>
    /// Destroys multiple items of the player of the same type. Returns a list containing the destroyed items.
    /// </summary>
    public List<Item> DestroyOwnedItems(ItemType type, int amount, bool showOnEventStepDisplay = true)
    {
        List<Item> destroyedItems = new List<Item>();
        for(int i = 0; i < amount; i++)
        {
            Item item = Inventory.First(x => x.Type == type);
            DestroyOwnedItem(item, showOnEventStepDisplay);
            destroyedItems.Add(item);
        }
        return destroyedItems;
    }

    public void DestroyItem(Item item)
    {
        if (item.IsPlayerOwned) throw new System.Exception("Can't use DestroyItem on player owned item. Use DestroyOwnedItem instead.");
        Destroy(item.gameObject);
    }

    public void EatItem(Item item)
    {
        if (!item.IsEdible) Debug.LogWarning("Eating item that is not edible! " + item.Name);
        Player.AddNutrition(item.OnEatNutrition);
        Player.AddHydration(item.OnEatHydration);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);

        UpdatePlayerStats();
    }
    public void DrinkItem(Item item)
    {
        if (!item.IsDrinkable) Debug.LogWarning("Drinking item that is not edible! " + item.Name);
        Player.AddHydration(item.OnDrinkHydration);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        UpdatePlayerStats();
    }

    public void AddBruiseWound()
    {
        Injury injury = Player.AddBruiseWound();
        InjuriesAddedSinceLastStep.Add(injury);
        UpdatePlayerStats();
    }
    public void AddCutWound()
    {
        Injury injury = Player.AddCutWound();
        InjuriesAddedSinceLastStep.Add(injury);
        UpdatePlayerStats();
    }

    public void TendInjury(Injury wound, Item item)
    {
        if (!item.CanTendWounds) Debug.LogWarning("Tending wound with an item that can't tend wounds! " + item.Name);
        if (wound.IsTended) Debug.LogWarning("Tending wound that is already tended.");
        wound.SetHightlight(false);
        Player.TendWound(wound);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        UpdatePlayerStats();
    }

    public void RemoveInjury(Injury injury)
    {
        Player.RemoveInjury(injury);
    }

    public void HealInfection(Injury wound, Item item)
    {
        if(!item.CanHealInfections) Debug.LogWarning("Healing infection with an item that can't heal infections! " + item.Name);
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");
        wound.SetHightlight(false);
        Player.HealInfection(wound);
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        UpdatePlayerStats();
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

    public void AddDog()
    {
        Player.AddDog();
        Companions.Add(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.Init(this);
        UpdatePlayerStats();
    }
    public void RemoveDog()
    {
        Player.RemoveDog();
        Companions.Remove(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.gameObject.SetActive(false);
        UpdatePlayerStats();
    }

    public void AddParrot()
    {
        Player.AddParrot();
        Companions.Add(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.Init(this);
        UpdatePlayerStats();
    }
    public void FeedParrot(Item item, float value)
    {
        DestroyOwnedItem(item, showOnEventStepDisplay: false);
        ResourceManager.Singleton.Parrot.AddNutrition(value);
        UpdatePlayerStats();
    }
    public void RemoveParrot()
    {
        Player.RemoveParrot();
        Companions.Remove(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.gameObject.SetActive(false);
        UpdatePlayerStats();
    }

    public void SetPosition(WorldMapTile tile)
    {
        if (CurrentPosition != null) CurrentPosition.Location.Sprite.gameObject.SetActive(false);
        CurrentPosition = tile;
        CurrentPosition.Location.Sprite.gameObject.SetActive(true);

        PathHistory.Add(CurrentPosition);

        CheckGameOver();
    }

    private void UpdatePlayerStats()
    {
        Player.UpdateStatusEffects();
        Player.UpdateSprites();
        foreach (Companion c in Companions) c.UpdateStatusEffects();

        UI.UpdateHealthReports();
        UI.UpdateStats();
    }

    #endregion

    #region Getters

    public Location CurrentLocation => CurrentPosition.Location;

    public Item GetItemInstance(ItemType type)
    {
        Item item = Instantiate(ItemPrefabs.FirstOrDefault(x => x.Type == type));
        if (item == null) throw new System.Exception("No item prefab found with type " + type.ToString());
        item.Init(this);
        return item;
    }

    public System.Array GetAllItemTypes()
    {
        return System.Enum.GetValues(typeof(ItemType));
    }

    public int GetItemTypeAmount(ItemType type)
    {
        return Inventory.Count(x => x.Type == type);
    }

    /// <summary>
    /// Returns a LootTable containing all items with equal chances.
    /// </summary>
    public LootTable GetStandardLootTable()
    {
        Dictionary<ItemType, float> dic = new Dictionary<ItemType, float>();
        foreach (Item item in ItemPrefabs) dic.Add(item.Type, 1f);
        return new LootTable(dic);
    }

    public Item RandomInventoryItem => Inventory[Random.Range(0, Inventory.Count)];
    public bool IsMissionActive(MissionId id)
    {
        return Missions.ContainsKey(id);
    }

    public static Game Singleton;

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
