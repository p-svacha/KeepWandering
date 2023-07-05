using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public Game Game { get; private set; }
    public EventStep InitialStep { get; private set; }

    public Event(Game game)
    {
        Game = game;
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

    /// <summary>
    /// Returns a value that determines how likely this event appears at a specific game state.
    /// </summary>
    public abstract float GetEventProbability();

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
