using System.Collections.Generic;
using UnityEngine;

public class BiomeEncounter_Fallback : Encounter
{
    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        FixedOutcomeOption sleepOption = new FixedOutcomeOption()
        {
            Text = "Sleep",
            Description = "Go to sleep and hope for a calm night.",
            Action = Sleep
        };
        options.Add(sleepOption);
        return options;
    }

    private string Sleep()
    {
        Game.EndEveningEncounter();
        return "";
    }

    protected override void OnEnd() { }
    protected override void OnInitialize() { }
    protected override string OnStart() => $"How would you like to spend your evening in the {Game.CurrentPosition.Biome.Label}?";
    protected override void RefreshSprites() { }
    protected override bool IsMoveOnOptionAvailable() => false;
}
