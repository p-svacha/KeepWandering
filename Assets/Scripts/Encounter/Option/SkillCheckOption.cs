using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillCheckOption : EncounterStepOption
{
    public override EncounterStepOptionType Type => EncounterStepOptionType.SkillCheck;

    /// <summary>
    /// Base difficulty of the encounter step option, before any modifiers apply.
    /// </summary>
    public int BaseDifficulty { get; private set; }

    /// <summary>
    /// List of item slots that can be optionally filled with items to reduce the difficulty of this option.
    /// </summary>
    public Dictionary<ItemSlot, int> ModifierItemSlots { get; private set; }

    /// <summary>
    /// The stats that are relevant for performing this encounter step option, along with the modifier of how much it affects the difficulty of the encounter step option.
    /// </summary>
    public Dictionary<StatDef, float> AssociatedStats { get; private set; }

    /// <summary>
    /// Function that gets executed when choosing this option. Function must return the next step in the encounter. The function takes in the outcome of the skill check as a parameter, so different outcomes can lead to different next steps.
    /// </summary>
    public Func<SkillCheckOutcome, EncounterStep> Actions { get; private set; }

    public SkillCheckOption(
        string text,
        int baseDifficulty,
        Func<SkillCheckOutcome, EncounterStep> actions,
        List<ItemSlot> requirementSlots = null,
        Dictionary<StatDef, float> associatedStats = null,
        Dictionary<ItemSlot, int> modifierItemSlots = null
        ) : base(text, requirementSlots)
    {
        BaseDifficulty = baseDifficulty;
        Actions = actions;
        ModifierItemSlots = modifierItemSlots ?? new Dictionary<ItemSlot, int>();
        AssociatedStats = associatedStats ?? new Dictionary<StatDef, float>();
    }

    public override EncounterStep Execute()
    {
        SkillCheckOutcome outcome = RollOutCome();
        return Actions.Invoke(outcome);
    }

    private SkillCheckOutcome RollOutCome()
    {
        return SkillCheckOutcome.Success;
    }


    public void OnHoverStart()
    {
        foreach (StatDef stat in AssociatedStats.Keys) Game.Instance.UI.HightlightStat(stat);
    }

    public void OnHoverEnd()
    {
        foreach (StatDef stat in AssociatedStats.Keys) Game.Instance.UI.UnhighlightStat(stat);
    }
}
