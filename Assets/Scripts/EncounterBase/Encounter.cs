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
    public Quest Mission { get; private set; }
    public int NumVisits { get; private set; }
    protected bool IsFirstVisit => NumVisits == 1;

    // During encounter
    private bool IsEncounterDone;
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
        IsEncounterDone = false;

        string startText = OnStart();

        return GetNextEncounterStep(startText);
    }

    public EncounterStep GetNextEncounterStep(string text)
    {
        RefreshSprites();
        return new EncounterStep(text, _GetOptions());
    }

    public void SetMission(Quest mission)
    {
        Mission = mission;
    }

    private List<EncounterOption> _GetOptions()
    {
        if (IsEncounterDone) return new List<EncounterOption>();
        else
        {
            List<EncounterOption> options = GetOptions();
            if (IsMoveOnOptionAvailable())
            {
                // Ignore
                options.Add(new FixedOutcomeOption()
                {
                    Text = "Move on",
                    Action = () => EndEncounter("You move on.")
                });
            }
            return options;
        }
    }


    /// <summary>
    /// Called exactly once when the encounter is first created, regardless if the player is there or the encounter is starting or not. Used to set up all initial randomized values (like items, etc.).
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// Called whenever this encounter starts, returning the text of the initial step of the encounter. For location events that player can come back to, this is called every time the player enters the location, not just the first time. For encounters that are only played once, this is called after InitializeEncounter.
    /// </summary>
    protected abstract string OnStart();

    /// <summary>
    /// Called every time the player chooses an option. Shows/Hides sprites according to the current state of the encounter.
    /// </summary>
    protected abstract void RefreshSprites();

    /// <summary>
    /// Returns the options that the player can choose from at the current step of the encounter based on the encounters current state. This is called every time the encounter step changes, so it can be used to change the options based on the player's previous choices.
    /// </summary>
    protected abstract List<EncounterOption> GetOptions();

    /// <summary>
    /// If true, there is a "Move On" option available to end the encounter.
    /// </summary>
    protected abstract bool IsMoveOnOptionAvailable();

    /// <summary>
    /// Gets called when the encounter is over.
    /// </summary>
    protected abstract void OnEnd();

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
    protected void ShowPlayerCharacter(bool value) => Game.ShowPlayerCharacter(value);

    /// <summary>
    /// Makes a gameobject belonging to this event invisible.
    /// </summary>
    protected void HideEventSprite(GameObject sprite)
    {
        sprite.gameObject.SetActive(false);
        EventSprites.Remove(sprite);
    }

    /// <summary>
    /// Ends the event. Only called from Game.
    /// </summary>
    public void EndEncounter()
    {
        foreach (GameObject sprite in EventSprites) sprite.gameObject.SetActive(false);
        EventSprites.Clear();
        OnEnd();
    }

    /// <summary>
    /// Called from subclass as the action of an option that leads to the end of the encounter.
    /// </summary>
    public string EndEncounter(string endText)
    {
        IsEncounterDone = true;
        return endText;
    }

    #region Getters

    public virtual string Label => Def.Label; // Shown on world map

    #endregion

}
