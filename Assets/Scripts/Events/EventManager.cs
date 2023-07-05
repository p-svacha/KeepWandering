using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The EventManager is responsible for chosing which events will appear and then creating them. 
/// </summary>
public class EventManager
{
    private Game Game;
    public List<Event> DummyEvents;

    // Forced (god mode)
    private Event ForcedEvent;

    public EventManager(Game game)
    {
        Game = game;
        InitDummyEvents();
    }

    /// <summary>
    /// Creates a dummy instance for each event that is responsible for returning its probability and returning an instance of itself.
    /// </summary>
    private void InitDummyEvents()
    {
        DummyEvents = new List<Event>()
        {
            new E001_Crate(Game),
            new E002_Dog(Game),
            new E003_EvilGuy(Game),
            new E004_ParrotWoman(Game),
            new E005_ParrowWomanReunion(Game),
            new E006_WoodsBunker(Game),
            new E007_Trader(Game),
            new E008_DistressedPerson(Game)
        };
    }

    /// <summary>
    /// Choses and returns an element given their probabilities during the current game state.
    /// </summary>
    public Event GetDayEvent()
    {
        // Forced event
        if (ForcedEvent != null)
        {
            ForcedEvent = null;
            return ForcedEvent.GetEventInstance;
        }

        // Chose an event for the day
        Dictionary<Event, float> eventTable = new Dictionary<Event, float>();
        foreach (Event dummyEvent in DummyEvents) eventTable.Add(dummyEvent, dummyEvent.GetEventProbability());
        Event chosenEvent = HelperFunctions.GetWeightedRandomElement(eventTable);

        return chosenEvent.GetEventInstance;
    }

    public void SetForcedEvent(Event forcedEvent)
    {
        ForcedEvent = forcedEvent;
    }
}
