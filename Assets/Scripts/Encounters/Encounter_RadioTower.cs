using System.Collections.Generic;
using UnityEngine;

public class Encounter_RadioTower : LocationEncounter
{
    private WorldMapTile HomeOfR;
    private Area CityOfR => HomeOfR.City;

    private bool IsNoteTaken;

    protected override void OnInitialize()
    {
        HomeOfR = WorldMap.GetRandomEmptyTileOfBiome(BiomeDefOf.City);
    }

    protected override EncounterStep OnStart()
    {
        string text = "A radio tower. The light at the top is still blinking. A note is taped to the door.";

        List<EncounterOption> options = new List<EncounterOption>()
        {
            GetTakeNoteOption()
        };

        RefreshSprites();
        return new EncounterStep(text, options);
    }

    private void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Tower", true);
        SetEncounterSpriteVisibility("Note", !IsNoteTaken);
    }


    private EncounterOption GetTakeNoteOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Take the note",
            Description = "Read and take the note on the door.",
            Action = ReadNote
        };
    }
    private EncounterStep ReadNote()
    {
        string text = $"The note reads:\n\"Still transmitting. If you can hear this, the fence has a weak point. Find me in {CityOfR.Name}. - R'\"";
        Game.ModifyStatBaseValue(StatDefOf.Morale, +1);
        Game.AddMission(new Mission(MissionId.FindRadioTowerR, $"Find R in {CityOfR.Name}"));

        List<EncounterOption> options = new List<EncounterOption>();

        RefreshSprites();
        return new EncounterStep(text, options);
    }
}
