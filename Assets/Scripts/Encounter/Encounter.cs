using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instance of an encounter.
/// </summary>
public abstract class Encounter
{
    public Game Game { get; private set; }
    public EncounterDef Def { get; private set; }
    public EncounterStep InitialStep { get; private set; }
    public Mission Mission { get; private set; }

    // During encounter
    private List<GameObject> EventSprites = new List<GameObject>();

    public Encounter() { } // Empty constructor for activator
    public void Init(Game game, EncounterDef def)
    {
        Game = game;
        Def = def;
    }

    /// <summary>
    /// Returns the modified version of a loot table taking in account the current biome.
    /// </summary>
    protected LootTable GetBiomeLootTable(LootTable table)
    {
        return table.Union(Game.CurrentPosition.Biome.LootTable);
    }

    /// <summary>
    /// Initializes and starts the event, making it visible on screen and playable.
    /// </summary>
    public void StartEncounter()
    {
        OnEventStart();
        InitialStep = GetInitialStep();
    }
    public void SetMission(Mission mission)
    {
        Mission = mission;
    }


    /// <summary>
    /// Initializes the event by setting up all attributes, setting relevant sprites and items etc.
    /// </summary>
    protected abstract void OnEventStart();

    /// <summary>
    /// Sets the first EventStep that appears when the event begins.
    /// </summary>
    protected abstract EncounterStep GetInitialStep();

    /// <summary>
    /// Makes a gameobject belonging to this event visible. The gameobject will be hidden when the event ends.
    /// <br/>In the Unity hierarchy, the GameObject needs to be placed in GameScreen/Encounters/{EncounterDefName}/{spriteName}.
    /// </summary>
    protected void ShowEventSprite(string spriteName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        spriteObj.gameObject.SetActive(true);

        EventSprites.Add(spriteObj);
    }

    /// <summary>
    /// Makes a gameobject belonging to this event invisible.
    /// </summary>
    protected void HideEventSprite(GameObject sprite)
    {
        sprite.gameObject.SetActive(false);
        EventSprites.Remove(sprite);
    }

    /// <summary>
    /// Ends the event.
    /// </summary>
    public void EndEvent()
    {
        foreach (GameObject sprite in EventSprites) sprite.gameObject.SetActive(false);
        EventSprites.Clear();
        OnEventEnd();
    }

    /// <summary>
    /// Handles everything that needs to be done when the event is done, like hiding sprites and destroying leftover items.
    /// </summary>
    protected virtual void OnEventEnd() { }
}
