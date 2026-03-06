using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An instance of an encounter.
/// </summary>
public abstract class Encounter
{
    public Game Game { get; private set; }
    public WorldMap WorldMap => WorldMap.Instance;
    public EncounterDef Def { get; private set; }
    public Mission Mission { get; private set; }
    public int NumVisits { get; private set; }
    protected bool IsFirstVisit => NumVisits == 1;

    // During encounter
    private List<GameObject> EventSprites = new List<GameObject>();

    public Encounter() { } // Empty constructor for activator
    public void Init(Game game, EncounterDef def)
    {
        Game = game;
        Def = def;
        NumVisits = 0;
        OnInitialize();
    }

    /// <summary>
    /// Returns the modified version of a loot table taking in account the current biome.
    /// </summary>
    protected LootTable GetBiomeLootTable(LootTable table)
    {
        return table.Union(Game.CurrentPosition.Biome.LootTable);
    }

    public EncounterStep StartEncounter()
    {
        NumVisits++;
        return OnStart();
    }

    public void SetMission(Mission mission)
    {
        Mission = mission;
    }


    /// <summary>
    /// Called exactly once when the encounter is first created, regardless if the player is there or the encounter is starting or not. Used to set up all initial randomized values (like items, etc.).
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// Called whenever this encounter starts, returning the initial step of the encounter. For location events that player can come back to, this is called every time the player enters the location, not just the first time. For encounters that are only played once, this is called after InitializeEncounter.
    /// </summary>
    protected abstract EncounterStep OnStart();

    /// <summary>
    /// Handles everything that needs to be done when the event is done, like hiding sprites and destroying leftover items.
    /// </summary>
    protected virtual void OnEnd() { }

    /// <summary>
    /// Makes a gameobject belonging to this event visible. The gameobject will be hidden when the event ends.
    /// <br/>In the Unity hierarchy, the GameObject needs to be placed in GameScreen/Encounters/{EncounterDefName}/{spriteName}.
    /// </summary>
    protected void ShowEncounterSprite(string spriteName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        spriteObj.gameObject.SetActive(true);

        EventSprites.Add(spriteObj);
    }
    protected void HideEncounterSprite(string spriteName)
    {
        GameObject spriteObj = Game.EncounterContainer.transform.Find($"{Def.DefName}/{spriteName}").gameObject;
        spriteObj.gameObject.SetActive(false);
        EventSprites.Remove(spriteObj);
    }
    protected void SetEncounterSpriteVisibility(string spriteName, bool show)
    {
        if (show) ShowEncounterSprite(spriteName);
        else HideEncounterSprite(spriteName);
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
    public void EndEncounter()
    {
        foreach (GameObject sprite in EventSprites) sprite.gameObject.SetActive(false);
        EventSprites.Clear();
        OnEnd();
    }

    protected EncounterStep EndEncounter(string text)
    {
        return new EncounterStep(text);
    }

    #region Getters

    public virtual string Label => Def.Label; // Shown on world map

    #endregion

}
