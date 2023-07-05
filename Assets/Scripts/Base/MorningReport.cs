using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorningReport
{
    public int Day;
    public List<string> NightEvents;
    public List<Item> AddedItems;
    public List<Item> RemovedItems;

    public MorningReport(int day)
    {
        Day = day;
        NightEvents = new List<string>();
        AddedItems = new List<Item>();
        RemovedItems = new List<Item>();
    }
}
