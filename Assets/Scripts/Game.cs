using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [Header("UI Elements")]
    public Text DayText;
    public UI_EventDisplay EventStepDisplay;
    public GameObject StatusEffectsContainer;
    public UI_StatusEffect StatusEffectPrefab;
    public Text StatusEffectTitlePrefab;
    public UI_Missions MissionsDisplay;
    public UI_EscapeMenu EscapeMenu;

    [Header("Description Box")]
    public UI_DescriptionBox DescriptionBox;
    private Item CurrentHoverItem;
    private const float HoverTimeForDescription = 1f;
    private float CurrentHoverTime;

    [Header("Interaction Box")]
    public UI_InteractionBox InteractionBox;
    public Item CurrentInteractionItem;

    [Header("Day Transition")]
    public Image BlackTransitionImage;
    public Text BlackTransitionText;
    private const float InTransitionTime = 3f;
    private const float TransitionFadeTime = 1f;
    private float CurrentTransitionTime;

    [Header("Game State")]
    public GameState State;
    public int Day;
    public Event CurrentEvent;
    public EventStep CurrentEventStep;

    public Location CurrentLocation;
    public Location NextDayLocation;
    public List<Item> Inventory = new List<Item>();
    public Dictionary<MissionId, Mission> Missions = new Dictionary<MissionId, Mission>();

    // Forced events (god mode)
    public bool HasForcedEvent;
    public EventType ForcedEventType;
    public bool HasForcedLocationEvent;
    public LocationEventType ForcedLocationEventType;

    [Header("Items")]
    public List<Item> ItemPrefabs;

    [Header("Characters")]
    public PlayerCharacter Player;
    public List<Companion> Companions = new List<Companion>();

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
        Player.Init(this);
        EscapeMenu.Init(this);

        AddItemToInventory(GetItemInstance(ItemType.Beans));
        AddItemToInventory(GetItemInstance(ItemType.WaterBottle));
        AddItemToInventory(GetItemInstance(HelperFunctions.GetWeightedRandomElement(StartItemTable)));

        NextDayLocation = ResourceManager.Singleton.LOC_Suburbs;

        BlackTransitionText.text = "Day " + Day;
        SwitchState(GameState.InDayTransition);
    }

    // Update is called once per frame
    void Update()
    {
        bool uiClick = EventSystem.current.IsPointerOverGameObject();

        // Escape menu
        if (Input.GetKeyDown(KeyCode.Escape)) EscapeMenu.gameObject.SetActive(!EscapeMenu.gameObject.activeSelf);

        // Update per state
        switch (State) {

            case GameState.DayTransitionFadeIn:
            case GameState.EndEventTransitionIn:
            case GameState.EndMorningReportTransitionIn:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= TransitionFadeTime)
                {
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, 1f);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, 1f);
                    if(State == GameState.DayTransitionFadeIn) SwitchState(GameState.InDayTransition);
                    if(State == GameState.EndEventTransitionIn) SwitchState(GameState.EndEventTransitionOut);
                    if(State == GameState.EndMorningReportTransitionIn) SwitchState(GameState.EndMorningReportTransitionOut);
                }
                else
                {
                    float alpha = CurrentTransitionTime / TransitionFadeTime;
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, alpha);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, alpha);
                }
                break;

            case GameState.InDayTransition:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= InTransitionTime)
                    SwitchState(GameState.DayTransitionFadeOut);
                break;

            case GameState.DayTransitionFadeOut:
            case GameState.EndEventTransitionOut:
            case GameState.EndMorningReportTransitionOut:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= TransitionFadeTime)
                {
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, 0f);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, 0f);
                    State = GameState.InGame;
                }
                else
                {
                    float alpha = 1f - (CurrentTransitionTime / TransitionFadeTime);
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, alpha);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, alpha);
                }
                break;

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

                        HideDescriptionBox();
                    }
                    else
                    {
                        CurrentHoverTime += Time.deltaTime;
                        if (CurrentHoverTime >= HoverTimeForDescription && !InteractionBox.gameObject.activeSelf) ShowDescriptionBox(CurrentHoverItem);
                    }
                }
                else
                {
                    if (CurrentHoverItem != null)
                    {
                        if(!CurrentHoverItem.ForceGlow) CurrentHoverItem.GetComponent<SpriteRenderer>().material.SetFloat("_IsGlowing", 0);
                        CurrentHoverItem = null;
                        DescriptionBox.gameObject.SetActive(false);
                    }
                }

                // Click -> Interact
                if (Input.GetMouseButtonDown(0) && !uiClick)
                {
                    if (InteractionBox.gameObject.activeSelf && (CurrentHoverItem == null || CurrentInteractionItem == CurrentHoverItem))
                    {
                        CurrentInteractionItem = null;
                        HideInteractionBox();
                        CurrentHoverTime = 0f;
                    }
                    else if (CurrentHoverItem != null && CurrentHoverItem.CanInteract())
                    {
                        CurrentInteractionItem = CurrentHoverItem;
                        ShowInteractionBox(CurrentHoverItem);
                        HideDescriptionBox();
                    }
                }
                if (InteractionBox.gameObject.activeSelf) InteractionBox.UpdatePosition(CurrentInteractionItem);
                break;
        }
    }

    private void SwitchState(GameState newState)
    {
        GameState oldState = State;
        // Debug.Log("Switch State " + oldState.ToString() + " --> " + newState.ToString());

        switch (oldState)
        {
            case GameState.InGame:
                DescriptionBox.gameObject.SetActive(false);
                break;
        }

        switch(newState)
        {
            case GameState.InDayTransition:
                StartNewDay();

                string gameOver = GetGameOverReason();
                if(gameOver != null)
                {
                    BlackTransitionText.text = "Day " + Day + "\n" + gameOver;
                    SwitchState(GameState.GameOver);
                    return;
                }

                CurrentTransitionTime = 0f;
                BlackTransitionImage.color = new Color(0f, 0f, 0f, 1f);
                BlackTransitionText.color = new Color(1f, 1f, 1f, 1f);
                BlackTransitionText.text = "Day " + Day;
                DayText.text = "Day " + Day;
                break;

            case GameState.DayTransitionFadeOut:
                CurrentTransitionTime = 0f;
                break;

            case GameState.EndEventTransitionIn:
            case GameState.EndMorningReportTransitionIn:
                CurrentTransitionTime = 0f;
                BlackTransitionText.text = "";
                break;

            case GameState.EndMorningReportTransitionOut:
                StartDayEvent();
                CurrentTransitionTime = 0f;
                break;

            case GameState.EndEventTransitionOut:
                StartLocationEvent();
                CurrentTransitionTime = 0f;
                break;

            case GameState.GameOver:
                BlackTransitionImage.color = new Color(0f, 0f, 0f, 1f);
                BlackTransitionText.color = new Color(1f, 1f, 1f, 1f);
                break;

            case GameState.DayTransitionFadeIn:
                CurrentTransitionTime = 0f;
                DayText.text = "Day " + Day;
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
        UpdateStatusEffects();

        // Location switch
        SetLocation(NextDayLocation);

        // Show morning reports
        EventStepDisplay.DisplayMorningReport(morningReport);
    }

    public void EndMorningReport()
    {
        SwitchState(GameState.EndMorningReportTransitionIn);
    }

    private void StartDayEvent()
    {
        // Chose an event for the day
        Dictionary<EventType, float> eventTable = new Dictionary<EventType, float>();
        foreach (EventType type in System.Enum.GetValues(typeof(EventType))) eventTable.Add(type, GetEventProbability(type));
        EventType chosenEventType = HelperFunctions.GetWeightedRandomElement<EventType>(eventTable);

        if (HasForcedEvent && eventTable[ForcedEventType] != 0) chosenEventType = ForcedEventType;

        CurrentEvent = GetEventInstance(chosenEventType);

        // Display the event
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
            EventStepDisplay.Init(step);
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
        // Clear day event
        if (CurrentEvent != null)
        {
            foreach (GameObject sprite in CurrentEvent.EventSprites)
                sprite.gameObject.SetActive(false);
            foreach (Item item in CurrentEvent.EventItems)
                GameObject.Destroy(item.gameObject);
        }
        CurrentEvent = null;

        // Get a location event
        Dictionary<LocationEventType, float> eventTable = new Dictionary<LocationEventType, float>();
        foreach (LocationEventType type in System.Enum.GetValues(typeof(LocationEventType))) eventTable.Add(type, GetLocationEventProbability(type));
        LocationEventType chosenEventType = HelperFunctions.GetWeightedRandomElement<LocationEventType>(eventTable);

        if (HasForcedLocationEvent && eventTable[ForcedLocationEventType] != 0) chosenEventType = ForcedLocationEventType;

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

    #region Getters

    private float GetEventProbability(EventType type)
    {
        switch(type)
        {
            case EventType.E001_Crate: return E001_Crate.GetProbability(this);
            case EventType.E002_Dog: return E002_Dog.GetProbability(this);
            case EventType.E003_EvilGuy: return E003_EvilGuy.GetProbability(this);
            case EventType.E004_ParrotWoman: return E004_ParrotWoman.GetProbability(this);
            case EventType.E005_ParrotWomanReunion: return E005_ParrowWomanReunion.GetProbability(this);
            case EventType.E006_WoodsBunker: return E006_WoodsBunker.GetProbability(this);
            case EventType.E007_Trader: return E007_Trader .GetProbability(this);

            default: throw new System.Exception("Probability not handled for EventType " + type.ToString());
        }
    }

    private Event GetEventInstance(EventType type)
    {
        switch (type)
        {
            case EventType.E001_Crate: return E001_Crate.GetEventInstance(this);
            case EventType.E002_Dog: return E002_Dog.GetEventInstance(this);
            case EventType.E003_EvilGuy: return E003_EvilGuy.GetEventInstance(this);
            case EventType.E004_ParrotWoman: return E004_ParrotWoman.GetEventInstance(this);
            case EventType.E005_ParrotWomanReunion: return E005_ParrowWomanReunion.GetEventInstance(this);
            case EventType.E006_WoodsBunker: return E006_WoodsBunker.GetEventInstance(this);
            case EventType.E007_Trader: return E007_Trader.GetEventInstance(this);

            default: throw new System.Exception("Instancing not handled for EventType " + type.ToString());
        }
    }

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
        if(CurrentEvent != null) CurrentEvent.EventItems.Remove(item);
        item.transform.position = new Vector3(Random.Range(-8f, -3f), Random.Range(2f, 4f), 0f);
        item.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        item.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        item.GetComponent<SpriteRenderer>().enabled = true;
        item.IsOwned = true;
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
        UpdateStatusEffects();
    }
    public void DrinkItem(Item item)
    {
        if (!item.IsDrinkable) Debug.LogWarning("Drinking item that is not edible! " + item.Name);
        Player.AddHydration(item.OnDrinkHydration);
        DestroyOwnedItem(item);
        UpdateStatusEffects();
    }

    public void AddBruiseWound()
    {
        Player.AddBruiseWound();
        UpdateStatusEffects();
    }
    public void AddCutWound()
    {
        Player.AddCutWound();
        UpdateStatusEffects();
    }

    public void TendWound(Wound wound, Item item)
    {
        if (!item.CanTendWounds) Debug.LogWarning("Tending wound with an item that can't tend wounds! " + item.Name);
        if (wound.IsTended) Debug.LogWarning("Tending wound that is already tended.");
        wound.SetHightlight(false);
        Player.TendWound(wound);
        DestroyOwnedItem(item);
        UpdateStatusEffects();
    }
    public void HealInfection(Wound wound, Item item)
    {
        if(!item.CanHealInfections) Debug.LogWarning("Healing infection with an item that can't heal infections! " + item.Name);
        if (wound.InfectionStage == InfectionStage.None) Debug.LogWarning("Healing infection of wound that is not infected.");
        wound.SetHightlight(false);
        Player.HealInfection(wound);
        DestroyOwnedItem(item);
        UpdateStatusEffects();
    }

    public void AddDog()
    {
        Player.AddDog();
        Companions.Add(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.Init();
        UpdateStatusEffects();
    }
    public void RemoveDog()
    {
        Player.RemoveDog();
        Companions.Remove(ResourceManager.Singleton.Dog);
        ResourceManager.Singleton.Dog.gameObject.SetActive(false);
        UpdateStatusEffects();
    }

    public void AddParrot()
    {
        Player.AddParrot();
        Companions.Add(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.Init();
        UpdateStatusEffects();
    }
    public void FeedParrot(Item item, float value)
    {
        DestroyOwnedItem(item);
        ResourceManager.Singleton.Parrot.AddNutrition(value);
        UpdateStatusEffects();
    }
    public void RemoveParrot()
    {
        Player.RemoveParrot();
        Companions.Remove(ResourceManager.Singleton.Parrot);
        ResourceManager.Singleton.Parrot.gameObject.SetActive(false);
        UpdateStatusEffects();
    }

    public void SetNextDayLocation(Location location)
    {
        NextDayLocation = location;
    }

    #endregion

    #region UI Elements

    public void ShowDescriptionBox(Item item)
    {
        DescriptionBox.gameObject.SetActive(true);
        DescriptionBox.Init(item);
    }
    public void HideDescriptionBox()
    {
        DescriptionBox.gameObject.SetActive(false);
    }

    public void ShowInteractionBox(Item item)
    {
        InteractionBox.gameObject.SetActive(true);
        InteractionBox.Init(item);
    }
    public void HideInteractionBox()
    {
        InteractionBox.gameObject.SetActive(false);
        InteractionBox.Clear();
    }

    public void StartHover(Item item)
    {
        Debug.Log("hve");
        CurrentHoverItem = item;
        CurrentHoverTime = 0f;
    }

    public void EndHover(Item item)
    {
        CurrentHoverItem = null;
        DescriptionBox.gameObject.SetActive(false);
    }


    private void UpdateStatusEffects()
    {
        // Update and Get
        List<StatusEffect> statusEffects = new List<StatusEffect>();

        Player.UpdateSpritesAndStatusEffects();
        statusEffects.AddRange(Player.StatusEffects);

        foreach (Companion c in Companions)
        {
            c.UpdateStatusEffects();
            statusEffects.AddRange(c.StatusEffects);
        }

        Dictionary<string, List<StatusEffect>> groupedStatusEffects = statusEffects.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.ToList());

        // Display
        foreach (Transform t in StatusEffectsContainer.transform) Destroy(t.gameObject);

        foreach (KeyValuePair<string, List<StatusEffect>> kvp in groupedStatusEffects)
        {
            foreach (StatusEffect statusEffect in kvp.Value)
            {
                UI_StatusEffect effectObject = Instantiate(StatusEffectPrefab, StatusEffectsContainer.transform);
                effectObject.Init(statusEffect);
            }
            Text title = Instantiate(StatusEffectTitlePrefab, StatusEffectsContainer.transform);
            title.text = kvp.Key;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(StatusEffectsContainer.GetComponent<RectTransform>());

        CheckGameOver();
    }

    public void AddOrUpdateMission(MissionId missionId, string text)
    {
        if (Missions.ContainsKey(missionId)) Missions[missionId].Text = text;
        else Missions.Add(missionId, new Mission(missionId, text));
        MissionsDisplay.UpdateList(Missions.Values.ToList());
    }
    public void RemoveMission(MissionId missionId)
    {
        if (Missions.ContainsKey(missionId)) Missions.Remove(missionId);
        MissionsDisplay.UpdateList(Missions.Values.ToList());
    }

    #endregion
}
