using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public Game Game { get; private set; }
    public abstract int Id { get;  }
    public EventStep InitialStep { get; private set; }
    public Mission Mission { get; private set; }
    public int DaysSinceLastOccurence;
    private List<GameObject> EventSprites = new List<GameObject>();

    public Event(Game game)
    {
        Game = game;
        DaysSinceLastOccurence = -1;
    }

    /// <summary>
    /// Returns the modified version of a loot table taking in account the current location.
    /// </summary>
    protected LootTable GetLocationLootTable(LootTable table)
    {
        return table.Union(Game.CurrentLocation.LootTable);
    }

    /// <summary>
    /// Returns a new instance of the event.
    /// </summary>
    public abstract Event GetEventInstance { get; }

    /// <summary>
    /// Initializes and starts the event, making it visible on screen and playable.
    /// </summary>
    public void StartEvent(Mission mission = null)
    {
        Mission = mission;
        OnEventStart();
        InitialStep = GetInitialStep();
    }

    protected virtual float BaseProbability { get; }
    protected virtual Dictionary<LocationType, float> LocationProbabilityTable { get; }
    protected virtual bool CanOnlyOccurOnce => false;

    /// <summary>
    /// Returns a value that determines how likely this event appears at a specific game state.
    /// </summary>
    public virtual float GetEventProbability()
    {
        return GetDefaultEventProbability();
    }

    /// <summary>
    /// Returns if an event of this type has occured already in this game.
    /// </summary>
    /// <returns></returns>
    protected bool HasOccuredAlready => Game.EventManager.HasEncounteredEvent(Id);

    /// <summary>
    /// Returns the default calculation of event probability taking in account the base probability, the location and days since it last occured.
    /// </summary>
    /// <returns></returns>
    protected float GetDefaultEventProbability()
    {
        if (BaseProbability == 0f) return 0f;
        if (CanOnlyOccurOnce && HasOccuredAlready) return 0f;
        if (LocationProbabilityTable != null && !LocationProbabilityTable.ContainsKey(Game.CurrentPosition.Location.Type)) return 0f;

        float probability = BaseProbability;
        if (LocationProbabilityTable != null) probability *= LocationProbabilityTable[Game.CurrentPosition.Location.Type];
        if (DaysSinceLastOccurence == -1) return probability;

        float lastOccurenceModifier = 1f - (5f / (5f + DaysSinceLastOccurence));
        probability *= lastOccurenceModifier;

        return probability;
    }

    /// <summary>
    /// Initializes the event by setting up all attributes, setting relevant sprites and items etc.
    /// </summary>
    protected abstract void OnEventStart();

    /// <summary>
    /// Sets the first EventStep that appears when the event begins.
    /// </summary>
    protected abstract EventStep GetInitialStep();

    /// <summary>
    /// Makes a gameobject belonging to this event visible. The gameobject will be hidden when the event ends.
    /// </summary>
    protected void ShowEventSprite(GameObject sprite)
    {
        sprite.gameObject.SetActive(true);
        EventSprites.Add(sprite);
    }

    /// <summary>
    /// Makes a gameobject belonging to this event invisible.
    /// </summary>
    protected void HideEventSprite(GameObject sprite)
    {
        sprite.gameObject.SetActive(false);
        EventSprites.Remove(sprite);
    }

    /// <summary>
    /// Ends the event.
    /// </summary>
    public void EndEvent()
    {
        foreach (GameObject sprite in EventSprites) sprite.gameObject.SetActive(false);
        EventSprites.Clear();
        OnEventEnd();
    }

    /// <summary>
    /// Handles everything that needs to be done when the event is done, like hiding sprites and destroying leftover items.
    /// </summary>
    protected virtual void OnEventEnd() { }
}
