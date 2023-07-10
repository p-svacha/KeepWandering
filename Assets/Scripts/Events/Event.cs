using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public Game Game { get; private set; }
    public abstract int Id { get;  }
    public EventStep InitialStep { get; private set; }
    public int DaysSinceLastOccurence;

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
    public void StartEvent()
    {
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
        if (CanOnlyOccurOnce && HasOccuredAlready) return 0f;

        float locationProb = BaseProbability * LocationProbabilityTable[Game.CurrentPosition.Location.Type];
        if (DaysSinceLastOccurence == -1) return locationProb;

        float lastOccurenceModifier = 1f - (5f / (5f + DaysSinceLastOccurence));
        return locationProb * lastOccurenceModifier;
    }

    /// <summary>
    /// Initializes the event by setting up all attributes, setting relevant sprites and items etc.
    /// </summary>
    public abstract void OnEventStart();

    /// <summary>
    /// Sets the first EventStep that appears when the event begins.
    /// </summary>
    public abstract EventStep GetInitialStep();

    /// <summary>
    /// Handles everything that needs to be done when the event is done, like hiding sprites and destroying leftover items.
    /// </summary>
    public abstract void OnEventEnd();
}
