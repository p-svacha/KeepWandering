using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorningReport
{
    public int Day;
    public List<string> NightEvents { get; private set; }

    public MorningReport(int day)
    {
        Day = day;
        NightEvents = new List<string>();
    }

    public void AddNightEvent(string nightEvent)
    {
        NightEvents.Add(nightEvent);
    }
}
