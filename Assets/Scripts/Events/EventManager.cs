using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The EventManager is responsible for chosing which events will appear and then creating them. 
/// </summary>
public class EventManager
{
    private Game Game;
    public Dictionary<int, Event> DummyEvents;

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
        List<Event> eventList = new List<Event>()
        {
            new E001_Crate(Game),
            new E002_Dog(Game),
            new E003_EvilGuy(Game),
            new E004_ParrotWoman(Game),
            new E005_ParrotWomanReunion(Game),
            new E006_WoodsBunker(Game),
            new E007_Trader(Game),
            new E008_DistressedPerson(Game),
            new E009_AbandondedShelter(Game),
            new E010_QuarantineFence(Game),
            new E011_SurvivorNeedsItemFromLocation(Game),
            new E012_ItemStash(Game),
        };

        DummyEvents = new Dictionary<int, Event>();
        foreach (Event e in eventList) DummyEvents.Add(e.Id, e);
    }

    /// <summary>
    /// Choses and returns an element given their probabilities during the current game state.
    /// </summary>
    public Event GetAfternoonEvent()
    {
        // Forced event
        if (ForcedEvent != null)
        {
            Event forcedEventInstance = ForcedEvent.GetEventInstance;
            ForcedEvent = null;
            return forcedEventInstance;
        }

        // Chose an event for the day
        Dictionary<Event, float> eventTable = new Dictionary<Event, float>();
        foreach (Event dummyEvent in DummyEvents.Values) eventTable.Add(dummyEvent, dummyEvent.GetEventProbability());
        Event chosenEvent = HelperFunctions.GetWeightedRandomElement(eventTable);

        return chosenEvent.GetEventInstance;
    }

    public void UpdateDaysSinceLastOccurence(Event dayEvent)
    {
        foreach(Event e in DummyEvents.Values)
        {
            if (e.Id == dayEvent.Id) DummyEvents[e.Id].DaysSinceLastOccurence = 0;
            else if (DummyEvents[e.Id].DaysSinceLastOccurence != -1) DummyEvents[e.Id].DaysSinceLastOccurence++;
        }
    }

    public bool HasEncounteredEvent(int eventId)
    {
        return DummyEvents[eventId].DaysSinceLastOccurence != -1;
    }

    public int DaysSinceLastEventOccurence(int eventId)
    {
        return DummyEvents[eventId].DaysSinceLastOccurence;
    }

    public void ForceEvent(int eventId)
    {
        if (!DummyEvents.ContainsKey(eventId)) ForcedEvent = null;
        else ForcedEvent = DummyEvents[eventId];
    }
}
