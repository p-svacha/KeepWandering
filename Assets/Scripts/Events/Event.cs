using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
    public Game Game;
    public EventType Type;
    public EventStep InitialStep;

    public bool ItemActionsAllowed; // If false, the player cannot do default item interactions during the event (i.e. eat, drink)

    public Event(Game game, EventType type)
    {
        Game = game;
        Type = type;
        InitEvent();
    }

    /// <summary>
    /// Sets all event attributes and creates the initial EventStep from which all other EventSteps branch out.
    /// </summary>
    public abstract void InitEvent();

    /// <summary>
    /// Handles everything that needs to be done when the event is done, like hiding sprites and destroying items.
    /// </summary>
    public abstract void OnEventEnd();

    #region General Event Options

    protected EventStep Attack(int enemyStrength)
    {

    }

    #endregion
}
