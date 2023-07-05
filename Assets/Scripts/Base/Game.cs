using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public EventManager EventManager { get; private set; }

    [Header("UI")]
    public GameUI UI;

    [Header("Game State")]
    public GameState State;
    public int Day { get; private set; }
    public Event CurrentEvent;
    public EventStep CurrentEventStep;

    public Location CurrentLocation;
    public Location NextDayLocation;
    public List<Item> Inventory = new List<Item>();
    public Dictionary<MissionId, Mission> Missions = new Dictionary<MissionId, Mission>();

    [Header("Items")]
    public List<Item> ItemPrefabs;
    private Item CurrentHoverItem;
    private float CurrentHoverTime;
    private Item CurrentInteractionItem;

    [Header("Characters")]
    public PlayerCharacter Player;
    public List<Companion> Companions = new List<Companion>();

    [Header("World Map")]
    public WorldMap WorldMap;

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
        StartGame();
    }
    private void StartGame()
    {
        WorldMap.GenerateWorld(100);
        EventManager = new EventManager(this);

        UI.Init(this);
        Player.Init(this);

        AddItemToInventory(GetItemInstance(ItemType.Beans));
        AddItemToInventory(GetItemInstance(ItemType.WaterBottle));
        AddItemToInventory(GetItemInstance(HelperFunctions.GetWeightedRandomElement(StartItemTable)));

        NextDayLocation = ResourceManager.Singleton.LOC_Suburbs;

        SwitchState(GameState.InDayTransition);
    }

    // Update is called once per frame
    void Update()
    {
        bool uiClick = EventSystem.current.IsPointerOverGameObject();

        // Escape menu
        if (Input.GetKeyDown(KeyCode.Escape)) UI.ToggleEscapeMenu();

        // Update per state
        switch (State) {

            case GameState.InGame:

                // Update Hovered Elements
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
                if (hit.collider != null && hit.collider.GetComponent<Item>() != null)
                {
                    if (hit.collider.GetComponent<Item>() != CurrentHoverItem)
                    {
                        if(CurrentHoverItem != null && !CurrentHoverItem.ForceGlow) CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        CurrentHoverItem = hit.collider.GetComponent<Item>();
                        CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 1);
                        CurrentHoverTime = 0f;

                        UI.HideDescriptionBox();
                    }
                    else
                    {
                        CurrentHoverTime += Time.deltaTime;
                        if (CurrentHoverTime >= GameUI.TOOLTIP_HOVER_TIME && !UI.InteractionBox.gameObject.activeSelf) UI.ShowDescriptionBox(CurrentHoverItem);
                    }
                }
                else
                {
                    if (CurrentHoverItem != null)
                    {
                        if(!CurrentHoverItem.ForceGlow) CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        CurrentHoverItem = null;
                        UI.DescriptionBox.gameObject.SetActive(false);
                    }
                }

                // Click -> Interact
                if (Input.GetMouseButtonDown(0) && !uiClick)
                {
                    if (UI.InteractionBox.gameObject.activeSelf && (CurrentHoverItem == null || CurrentInteractionItem == CurrentHoverItem))
                    {
                        CurrentInteractionItem = null;
                        UI.HideInteractionBox();
                        CurrentHoverTime = 0f;
                    }
                    else if (CurrentHoverItem != null && CurrentHoverItem.CanInteract())
                    {
                        CurrentInteractionItem = CurrentHoverItem;
                        UI.ShowInteractionBox(CurrentHoverItem);
                        UI.HideDescriptionBox();
                    }
                }
                if (UI.InteractionBox.gameObject.activeSelf) UI.InteractionBox.UpdatePosition(CurrentInteractionItem);
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
                UI.DescriptionBox.gameObject.SetActive(false);
                break;
        }

        switch(newState)
        {
            case GameState.InDayTransition:
                StartNewDay();

                string gameOver = GetGameOverReason();
                if(gameOver != null)
                {
                    UI.BlackTransitionText.text = "Day " + Day + "\n" + gameOver;
                    SwitchState(GameState.GameOver);
                    return;
                }

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
                StartDayEvent();
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.EndEventTransitionOut:
                StartLocationEvent();
                UI.FadeOutBlackTransition(GameUI.TRANSITION_FADE_TIME);
                break;

            case GameState.DayTransitionFadeIn:
                UI.FadeInBlackTransition(GameUI.TRANSITION_FADE_TIME);
                UI.DayText.text = "Day " + Day;
                break;
        }

        State = newState;
    }

    private void SetLocation(Location location)
    {
        if(CurrentLocation != null) CurrentLocation.gameObject.SetActive(false);
        CurrentLocation = location;
        CurrentLocation.gameObject.SetActive(true);
    }

    private void StartNewDay()
    {
        MorningReport morningReport = new MorningReport(Day);

        Day++;

        Player.OnEndDay(this, morningReport);
        List<Companion> companionsCopy = new List<Companion>();
        foreach (Companion c in Companions) companionsCopy.Add(c);
        foreach (Companion c in companionsCopy) c.OnEndDay(this, morningReport);
        UpdateAllStatusEffects();

        // Location switch
        SetLocation(NextDayLocation);

        // Show morning reports
        UI.EventStepDisplay.DisplayMorningReport(morningReport);

        // Day UI Updates
        UI.BlackTransitionText.text = "Day " + Day;
        UI.DayText.text = "Day " + Day;
        UI.DayTimeText.text = "Morning";
    }

    public void EndMorningReport()
    {
        SwitchState(GameState.EndMorningReportTransitionIn);
    }

    private void StartDayEvent()
    {
        UI.DayTimeText.text = "Afternoon";

        // Chose an event for the day
        CurrentEvent = EventManager.GetDayEvent();

        // Display the event
        CurrentEvent.StartEvent();
        DisplayEventStep(CurrentEvent.InitialStep);
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

    public void EndEvent()
    {
        SwitchState(GameState.EndEventTransitionIn);
    }

    private void StartLocationEvent()
    {
        UI.DayTimeText.text = "Evening";

        // Clear day event
        if (CurrentEvent != null) CurrentEvent.OnEventEnd();
        CurrentEvent = null;

        // Get a location event
        Dictionary<LocationEventType, float> eventTable = new Dictionary<LocationEventType, float>();
        foreach (LocationEventType type in System.Enum.GetValues(typeof(LocationEventType))) eventTable.Add(type, GetLocationEventProbability(type));
        LocationEventType chosenEventType = HelperFunctions.GetWeightedRandomElement<LocationEventType>(eventTable);

        //if (HasForcedLocationEvent && eventTable[ForcedLocationEventType] != 0) chosenEventType = ForcedLocationEventType;

        LocationEvent locationEvent = GetLocationEventInstance(chosenEventType);
        DisplayEventStep(locationEvent.EventStep);
    }

    public void EndDay()
    {
        SwitchState(GameState.DayTransitionFadeIn);
    }

    public void CheckGameOver()
    {
        if (GetGameOverReason() != null) SwitchState(GameState.DayTransitionFadeIn);
    }

    private string GetGameOverReason()
    {
        if (Player.Nutrition <= 0f) return "You starved";
        if (Player.Hydration <= 0f) return "You died of dehydration";
        if (Player.BoneHealth <= 0f) return "You died due to extreme fractures";
        if (Player.BloodAmount <= 0f) return "You bled out";
        if (Player.ActiveWounds.Any(x => x.InfectionStage == InfectionStage.Fatal)) return "You died (Infection)";
        if (E006_WoodsBunker.HasEnteredBunker) return "You are safe.";
        return null;
    }

    #endregion

    #region UI Feedback

    public void OnTransitionFadeInDone()
    {
        if (State == GameState.DayTransitionFadeIn) SwitchState(GameState.InDayTransition);
        else if (State == GameState.EndEventTransitionIn) SwitchState(GameState.EndEventTransitionOut);
        else if (State == GameState.EndMorningReportTransitionIn) SwitchState(GameState.EndMorningReportTransitionOut);
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

    #region Getters

    private float GetLocationEventProbability(LocationEventType type)
    {
        switch (type)
        {
            case LocationEventType.LE001_SuburbsToCity: return LE001_SuburbsToCity.GetProbability(this);
            case LocationEventType.LE002_SuburbsStay: return LE002_SuburbsStay.GetProbability(this);
            case LocationEventType.LE003_CityToSuburbs: return LE003_CityToSuburbs.GetProbability(this);
            case LocationEventType.LE004_CityStay: return LE004_CityStay.GetProbability(this);
            case LocationEventType.LE005_SuburbsToWoodsForce: return LE005_SuburbsToWoodsForce.GetProbability(this);
            case LocationEventType.LE006_WoodsStay: return LE006_WoodsStay.GetProbability(this);
            case LocationEventType.LE007_WoodsToSuburbsForce: return LE007_WoodsToSuburbsForce.GetProbability(this);

            default: throw new System.Exception("Probability not handled for LocationEventType " + type.ToString());
        }
    }
    private LocationEvent GetLocationEventInstance(LocationEventType type)
    {
        switch (type)
        {
            case LocationEventType.LE001_SuburbsToCity: return new LE001_SuburbsToCity(this);
            case LocationEventType.LE002_SuburbsStay: return new LE002_SuburbsStay(this);
            case LocationEventType.LE003_CityToSuburbs: return new LE003_CityToSuburbs(this);
            case LocationEventType.LE004_CityStay: return new LE004_CityStay(this);
            case LocationEventType.LE005_SuburbsToWoodsForce: return new LE005_SuburbsToWoodsForce(this);
            case LocationEventType.LE006_WoodsStay: return new LE006_WoodsStay(this);
            case LocationEventType.LE007_WoodsToSuburbsForce: return new LE007_WoodsToSuburbsForce(this);

            default: throw new System.Exception("Instancing not handled for LocationEventType " + type.ToString());
        }
    }

    public Item GetItemInstance(ItemType type)
    {
        Item item = Instantiate(ItemPrefabs.FirstOrDefault(x => x.Type == type));
        if(item == null) throw new System.Exception("No item prefab found with type " + type.ToString());
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

    #endregion

    #region Game Actions

    public void AddItemToInventory(Item item)
    {
        item.transform.position = new Vector3(Random.Range(-8f, -3f), Random.Range(2f, 4f), 0f);
        item.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        item.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        item.GetComponent<SpriteRenderer>().enabled = true;
        item.IsPlayerOwned = true;
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
        ResourceManager.Singleton.Dog.Init();
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
        ResourceManager.Singleton.Parrot.Init();
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

    public void SetNextDayLocation(Location location)
    {
        NextDayLocation = location;
    }

    private void UpdateAllStatusEffects()
    {
        Player.UpdateStatusEffects();
        foreach (Companion c in Companions) c.UpdateStatusEffects();
        UI.UpdateHealthReports();

        CheckGameOver();
    }

    #endregion
}
