using System.Collections.Generic;
using UnityEngine;

public abstract class HealthCondition
{
    public PlayerCharacter Player { get; private set; }
    public HealthConditionDef Def { get; private set; }
    public HealthConditionStage ActiveStage { get; private set; }
    public virtual bool IsActive => Def.IsPermanent ? ActiveStage != null : true;

    public HealthCondition() { } // Empty constructor for Activator

    public UI_StatusEffect UiDisplayElement { get; set; }

    public void Init(PlayerCharacter player, HealthConditionDef def)
    {
        Player = player;
        Def = def;
        OnInit();
    }

    protected void SetActiveStage(int stageIndex)
    {
        if (stageIndex == -1) ActiveStage = null;
        else ActiveStage = Def.Stages[stageIndex];
    }
    protected void SetActiveStage(HealthConditionStage stage)
    {
        ActiveStage = stage;
    }

    /// <summary>
    /// Gets called once when the health condition is added to the player.
    /// </summary>
    protected abstract void OnInit();

    /// <summary>
    /// Gets called after every action in the game.
    /// </summary>
    public abstract void OnUpdate();

    /// <summary>
    /// Gets called at the end of each day. Performs all events that happen during the night and returns a list of them for the morning report.
    /// </summary>
    public abstract void OnEndDay(Game game, MorningReport morningReport);

    /// <summary>
    /// Determines whether the current state represents a fatal condition that ends the game.
    /// <br/>Returns the reason of death if fatal, null otherwise.
    /// </summary>
    public abstract string IsFatal();

    #region Getters
    public string Label => (ActiveStage != null && ActiveStage.Label != "") ? ActiveStage.Label : Def.Label;
    public string LabelCapWord => Label.CapitalizeEachWord();
    public string Description => (ActiveStage != null && ActiveStage.Description != "") ? ActiveStage.Description : Def.Description;

    public virtual string GetReportLabel() => LabelCapWord;
    public virtual string GetReportDescription() => Description;
    public virtual Color GetReportTextColor() => ResourceManager.Color_Text_Default;
    public virtual Color GetReportBackgroundColor() => Color.clear;

    // Stat modifiers
    public Dictionary<StatDef, int> GetCurrentModifiers()
    {
        if (ActiveStage != null) return ActiveStage.StatModifiers;
        else return Def.StatModifiers;
    }

    public int GetStatModifierFor(StatDef stat)
    {
        Dictionary<StatDef, int> modifiers = GetCurrentModifiers();
        if (modifiers.ContainsKey(stat)) return modifiers[stat];
        else return 0;
    }

    #endregion
}
