using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public EventManager EventManager { get; private set; }

    [Header("Main Elements")]
    public Camera MainCamera;
    public GameUI UI;

    // Game State
    public GameState State { get; private set; }
    public int Day { get; private set; }
    public MorningReport LatestMorningReport { get; private set; }
    public Event CurrentEvent;
    public EventStep CurrentEventStep;

    public List<WorldMapTile> PathHistory = new List<WorldMapTile>();
    public WorldMapTile CurrentPosition;
    private WorldMapTile NextPosition;
    public bool PlayerIsOnQuarantinePerimeter => QuarantineZone.IsOnPerimeter(CurrentPosition);

    public Dictionary<MissionId, Mission> Missions = new Dictionary<MissionId, Mission>();

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
    private static Dictionary<ItemType, float> StartItemTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 8 },
        { ItemType.WaterBottle, 8 },
        { ItemType.Bandage, 5 },
        { ItemType.Bone, 3 },
        { ItemType.Knife, 2 },
        { ItemType.Antibiotics, 2 },
    };

    #region Game Flow

    void Start()
    {
        Singleton = this;
        StartGame();
    }

    private void StartGame()
    {
        WorldMap.Init(this);
        WorldMap.GenerateWorld(zoneRadius: 18, numAdditionalTiles: 400);
        //WorldMap.GenerateWorld(zoneRadius: 2, numAdditionalTiles: 10);
        WorldMapCamera.Init(this);

        EventManager = new EventManager(this);

        UI.Init(this);
        UI.ContextMenu.Init(this);
        Player.Init(this);

        SetPosition(WorldMap.GetTile(Vector2Int.zero));
        WorldMap.ResetCamera();

        AddItemToInventory(GetItemInstance(ItemType.Beans));
        AddItemToInventory(GetItemInstance(ItemType.WaterBottle));
        AddItemToInventory(GetItemInstance(HelperFunctions.GetWeightedRandomElement(StartItemTable)));

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
                    else if (CurrentHoverItem != null && CurrentHoverItem.CanInteract())
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
                break;

            case GameState.GameOver:
                UI.HoldBlackTransition(60f);
                break;
        }

        State = newState;
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
        if (Player.ActiveWounds.Any(x => x.InfectionStage == InfectionStage.Fatal)) return "You died (Infection)";

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
        UpdateAllStatusEffects();

        // Show morning report
        UpdateMorningEvent();

        // Day UI Updates
        UI.BlackTransitionText.text = "Day " + Day;
        UI.DayText.text = "Day " + Day;

        // Enable destination selection of adjacent tiles
        WorldMap.CanSelectDestination = true;
        foreach(Direction dir in HelperFunctions.GetAdjacentHexDirections())
        {
            Vector2Int adjCoord = HelperFunctions.GetAdjacentHexCoordinates(CurrentPosition.Coordinates, dir);
            WorldMapTile adjTile = WorldMap.GetTile(adjCoord);
            if (adjTile.IsPassable())
            {
                if(QuarantineZone.IsInArea(adjTile)) WorldMap.HighlightTileGreen(adjTile);
                else WorldMap.HighlightTileRed(adjTile);
            }
        }
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
            foreach (string e in LatestMorningReport.NightEvents) text += "\n" + e;
        }
        text += "\nWhat would you like to do today?";

        // Dialogue Options
        List<EventDialogueOption> options = new List<EventDialogueOption>();

        // Stay
        EventDialogueOption pickTargetLocationOption = new EventDialogueOption("Stay and explore", ExploreThisLocation);
        options.Add(pickTargetLocationOption);

        // Explore other location
        EventDialogueOption startTravelingOption = new EventDialogueOption("Go somewhere else", OpenMap);
        options.Add(startTravelingOption);

        EventStep morningEventStep = new EventStep(text, LatestMorningReport.AddedItems, LatestMorningReport.RemovedItems, options, null);
        return morningEventStep;
    }

    private EventStep OpenMap()
    {
        UI.OpenWorldMap();
        return GetMorningEvent();
    }
    private EventStep ExploreThisLocation()
    {
        EndMorningEvent();
        return null;
    }

    /// <summary>
    /// Gets called when a position is selected on the world map.
    /// </summary>
    public void SelectPositionOnMap(WorldMapTile tile)
    {
        // Set position
        NextPosition = tile;

        // End morning event
        EndMorningEvent();
    }

    public EventStep EndMorningEvent()
    {
        UI.CloseAllWindows();

        // Reset world map selection
        WorldMap.CanSelectDestination = false;
        WorldMap.UnhighlightAllGreenTiles();
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

        // Switch location if a new one was set in the morning
        if(NextPosition != null)
        {
            SetPosition(NextPosition);
            NextPosition = null;
        }

        // Chose an event for the afternoon
        CurrentEvent = EventManager.GetAfternoonEvent();

        // Display the event
        CurrentEvent.StartEvent();
        DisplayEventStep(CurrentEvent.InitialStep);

        // Update status
        UpdateAllStatusEffects();
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
        if (CurrentEvent != null) CurrentEvent.OnEventEnd();
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

        EventStep eveningEventStep = new EventStep(text, null, null, options, null);
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
        
        Inventory.Add(item);
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
    public void DestroyOwnedItem(Item item)
    {
        Inventory.Remove(item);
        Destroy(item.gameObject);
    }
    /// <summary>
    /// Destroys multiple items of the player of the same type. Returns a list containing the destroyed items.
    /// </summary>
    public List<Item> DestroyOwnedItems(ItemType type, int amount)
    {
        List<Item> destroyedItems = new List<Item>();
        for(int i = 0; i < amount; i++)
        {
            Item item = Inventory.First(x => x.Type == type);
            DestroyOwnedItem(item);
            destroyedItems.Add(item);
        }
        return destroyedItems;
    }

    public void EatItem(Item item)
    {
        if (!item.IsEdible) Debug.LogWarning("Eating item that is not edible! " + item.Name);
        Player.AddNutrition(item.OnEatNutrition);
        Player.AddHydration(item.OnEatHydration);
        DestroyOwnedItem(item);

        UpdateAllStatusEffects();
    }
    public void DrinkItem(Item item)
    {
        if (!item.IsDrinkable) Debug.LogWarning("Drinking item that is not edible! " + item.Name);
        Player.AddHydration(item.OnDrinkHydration);
        DestroyOwnedItem(item);
        UpdateAllStatusEffects();
    }

    public void AddBruiseWound()
    {
        Player.AddBruiseWound();
        UpdateAllStatusEffects();
    }
    public void AddCutWound()
    {
        Player.AddCutWound();
        UpdateAllStatusEffects();
    }

    public void TendWound(Wound wound, Item item)
    {
        if (!item.CanTendWounds) Debug.LogWarning("Tending wound with an item that can't tend wounds! " + item.Name);
        if (wound.IsTended) Debug.LogWarning("Tending wound that is already tended.");
        wound.SetHightlight(false);
        Player.TendWound(wound);
        DestroyOwnedItem(item);
        UpdateAllStatusEffects();
    }
    public void HealInfection(Wound wound, Item item)
    {
        if(!item.CanHealInfections) Debug.LogWarning("Healing infection with an item that can't heal infections! " + item.Name);
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");
        wound.SetHightlight(false);
        Player.HealInfection(wound);
        DestroyOwnedItem(item);
        UpdateAllStatusEffects();
    }

    public void AddOrUpdateMission(MissionId missionId, string text)
    {
        if (Missions.ContainsKey(missionId)) Missions[missionId].Text = text;
        else Missions.Add(missionId, new Mission(missionId, text));
        UI.UpdateMissionDisplay();
    }
    public void RemoveMission(MissionId missionId)
    {
        if (Missions.ContainsKey(missionId)) Missions.Remove(missionId);
        UI.UpdateMissionDisplay();
    }

    public void AddDog()
    {
        Player.AddDog();
        Companions.Add(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.Init(this);
        UpdateAllStatusEffects();
    }
    public void RemoveDog()
    {
        Player.RemoveDog();
        Companions.Remove(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.gameObject.SetActive(false);
        UpdateAllStatusEffects();
    }

    public void AddParrot()
    {
        Player.AddParrot();
        Companions.Add(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.Init(this);
        UpdateAllStatusEffects();
    }
    public void FeedParrot(Item item, float value)
    {
        DestroyOwnedItem(item);
        ResourceManager.Singleton.Parrot.AddNutrition(value);
        UpdateAllStatusEffects();
    }
    public void RemoveParrot()
    {
        Player.RemoveParrot();
        Companions.Remove(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.gameObject.SetActive(false);
        UpdateAllStatusEffects();
    }

    public void SetPosition(WorldMapTile tile)
    {
        if (CurrentPosition != null) CurrentPosition.Location.Sprite.gameObject.SetActive(false);
        CurrentPosition = tile;
        CurrentPosition.Location.Sprite.gameObject.SetActive(true);

        PathHistory.Add(CurrentPosition);
    }

    private void UpdateAllStatusEffects()
    {
        Player.UpdateStatusEffects();
        foreach (Companion c in Companions) c.UpdateStatusEffects();
        UI.UpdateHealthReports();

        CheckGameOver();
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
