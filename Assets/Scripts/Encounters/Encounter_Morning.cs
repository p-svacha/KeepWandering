using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

public class Encounter_Morning : Encounter
{
    public string GetMorningText()
    {
        string text = "";
        if (Game.Day == 1) text = "After you saw the news you knew that you have to get out of the quarantine zone. You grabbed everything you could find and left. Your journey begins...";
        else if (Game.LatestMorningReport.NightEvents.Count == 0) text = "You wake after an uneventful night.";
        else
        {
            text = $"You wake up in the {Game.CurrentPosition.Biome.Label}. The following happened during the night:";
            foreach (string e in Game.LatestMorningReport.NightEvents) text += "\n- " + e;
        }
        return text;
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (Game.Day == 1)
        {
            options.Add(new FixedOutcomeOption()
            {
                Text = "Start Journey",
                Description = "Open the map to choose your first location.",
                Action = OpenMap
            });
        }
        else
        {
            string exposureAppendix = ResourceManager.WarningText("\nThis will increase your exposure in this location, increasing the chance for attacks during the night!");

            options.Add(new FixedOutcomeOption()
            {
                Text = "Move",
                Description = "Open the map to choose a location to move to.",
                Action = OpenMap
            });
            options.Add(new FixedOutcomeOption()
            {
                Text = "Stay",
                Description = "Stay in the current location to continue where you left off yesterday." + exposureAppendix,
                Action = Stay
            });
            options.Add(new FixedOutcomeOption()
            {
                Text = "Rest",
                Description = "Rest and recover your energy. Skips the afternoon encounter and potentially heals some injuries." + exposureAppendix,
                Action = Rest
            });
        }

        return options;
    }
    private string Stay()
    {
        Game.SetDayAction(DayAction.Stay);
        Game.EndMorning();
        return "";
    }

    private string Rest()
    {
        Game.SetDayAction(DayAction.Rest);
        Game.EndMorning();
        return "";
    }

    private string OpenMap()
    {
        Game.UI.OpenWorldMap();
        return GetMorningText();
    }

    protected override void OnInitialize() { }
    protected override string OnStart() => GetMorningText();
    protected override void RefreshSprites() { }
    protected override bool IsMoveOnOptionAvailable() => false;
}
