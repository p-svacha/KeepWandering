using System.Collections.Generic;
using UnityEngine;

public class Encounter_HomeOfR : LocationEncounter
{
    protected override void OnInitialize()
    {
        throw new System.NotImplementedException();
    }

    protected override string OnStart()
    {
        throw new System.NotImplementedException();
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Building", true);
    }

    protected override List<EncounterOption> GetOptions()
    {
        throw new System.NotImplementedException();
    }
    protected override bool IsMoveOnOptionAvailable()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnEnd()
    {
        throw new System.NotImplementedException();
    }
}
