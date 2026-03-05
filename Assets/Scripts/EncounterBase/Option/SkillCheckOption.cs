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
    /// The stats that are relevant for performing this encounter step option, along with the modifier of how much it affects the difficulty of the encounter step option.
    /// </summary>
    public Dictionary<StatDef, float> RelevantStats { get; private set; }

    /// <summary>
    /// Function that gets executed when choosing this option. Function must return the next step in the encounter. The function takes in the outcome of the skill check as a parameter, so different outcomes can lead to different next steps.
    /// </summary>
    public Func<OptionOutcomeDef, EncounterStep> Actions { get; private set; }

    public bool CanPartiallySucceed { get; private set; }
    public bool CanCriticallySucceed { get; private set; }
    public bool CanCriticallyFail { get; private set; }

    public SkillCheckOption(
        string text,
        string description,
        int baseDifficulty,
        Func<OptionOutcomeDef, EncounterStep> actions,
        Dictionary<StatDef, float> relevantStats = null,
        List<ItemSlot> itemSlots = null,
        Dictionary<ItemSlot, int> modifierItemSlots = null,
        bool canPartiallySucceed = false,
        bool canCriticallySucceed = false,
        bool canCriticallyFail = false
        ) : base(text, description, itemSlots)
    {
        BaseDifficulty = baseDifficulty;
        Actions = actions;
        RelevantStats = relevantStats ?? new Dictionary<StatDef, float>();
        CanPartiallySucceed = canPartiallySucceed;
        CanCriticallySucceed = canCriticallySucceed;
        CanCriticallyFail = canCriticallyFail;
    }

    public override EncounterStep Execute()
    {
        OptionOutcomeDef outcome = RollOutcome();
        return Actions.Invoke(outcome);
    }

    /// <summary>
    /// Returns the calculated difficulty of this option, taking into account all modifiers.
    /// </summary>
    public int GetDifficultyValue()
    {
        int difficulty = BaseDifficulty;

        foreach (var modifier in GetDifficultyModifiers()) difficulty += modifier.Value;

        // Clamp to valid range (maximum has no limit)
        if (difficulty < 0) difficulty = 0;

        return difficulty;
    }

    public Dictionary<string, int> GetDifficultyModifiers()
    {
        Dictionary<string, int> modifiers = new Dictionary<string, int>();

        // Player stat modifiers
        foreach (var statEntry in RelevantStats)
        {
            int statValue = Game.Instance.Player.GetStatValue(statEntry.Key);
            int modifierAmount = (int)(statValue * statEntry.Value);
            if (modifierAmount != 0) modifiers.Add(statEntry.Key.LabelCapWord, modifierAmount);
        }

        // Morale modifier
        int moraleValue = Game.Instance.Player.Morale;
        if (moraleValue != 0) modifiers.Add("Morale", moraleValue);

        // Item slots
        foreach (ItemSlot slot in ItemSlots)
        {
            if (slot.IsFilled)
            {
                int modifierAmount = slot.GetDifficultyReduction(slot.FilledItem.Def);
                if (modifierAmount != 0) modifiers.Add($"Using {slot.FilledItem.Def.LabelCapWord}", -modifierAmount);
            }
        }

        return modifiers;
    }

    private OptionOutcomeDef RollOutcome()
    {
        int difficulty = GetDifficultyValue();
        int roll = UnityEngine.Random.Range(1, 101); // 1 to 100 inclusive

        if (roll > difficulty)
        {
            // Success - check for critical success
            if (CanCriticallySucceed)
            {
                // Critical success if roll is in the top 10% of the range above the difficulty value
                float criticalThreshold = 100f - (100f - difficulty) * 0.1f;
                if (roll > criticalThreshold) return OptionOutcomeDefOf.CriticalSuccess;
            }
            return OptionOutcomeDefOf.Success;
        }
        else
        {
            // Failure - check for partial success and critical failure
            if (CanPartiallySucceed && roll > difficulty * 0.5f)
            {
                return OptionOutcomeDefOf.PartialSuccess;
            }
            if (CanCriticallyFail && roll < difficulty * 0.1f)
            {
                return OptionOutcomeDefOf.CriticalFailure;
            }
            return OptionOutcomeDefOf.Failure;
        }
    }

    /// <summary>
    /// Returns a list of all possible outcomes for this skill check with their label, chance, and roll range.
    /// Outcomes with a 0% chance are excluded. Outcomes are ordered from best to worst.
    /// </summary>
    public List<SkillCheckOutcomeChance> GetOutcomeChances()
    {
        List<SkillCheckOutcomeChance> outcomes = new List<SkillCheckOutcomeChance>();
        int difficulty = GetDifficultyValue();

        // Failure range: [1, difficulty]
        if (difficulty > 0)
        {
            int failMin = 1;
            int failMax = Mathf.Min(difficulty, 100);
            int plainFailMin = failMin;
            int plainFailMax = failMax;

            // Critical Failure: roll < difficulty * 0.1
            if (CanCriticallyFail)
            {
                int cfMax = Mathf.CeilToInt(difficulty * 0.1f) - 1;
                if (cfMax >= failMin)
                {
                    cfMax = Mathf.Min(cfMax, failMax);
                    outcomes.Add(new SkillCheckOutcomeChance(OptionOutcomeDefOf.CriticalFailure, failMin, cfMax));
                    plainFailMin = cfMax + 1;
                }
            }

            // Partial Success: roll > difficulty * 0.5 and roll < difficulty
            if (CanPartiallySucceed)
            {
                int psMin = Mathf.FloorToInt(difficulty * 0.5f) + 1;
                if (psMin <= failMax)
                {
                    outcomes.Add(new SkillCheckOutcomeChance(OptionOutcomeDefOf.PartialSuccess, psMin, failMax));
                    plainFailMax = psMin - 1;
                }
            }

            // Plain Failure: remaining failure range
            if (plainFailMax >= plainFailMin)
            {
                outcomes.Add(new SkillCheckOutcomeChance(OptionOutcomeDefOf.Failure, plainFailMin, plainFailMax));
            }
        }

        // Success range: [difficulty + 1, 100]
        if (difficulty < 100)
        {
            int successMin = difficulty + 1;
            int successMax = 100;
            int plainSuccessMax = successMax;

            // Critical Success: roll > criticalThreshold
            if (CanCriticallySucceed)
            {
                float csThreshold = 100f - (100f - difficulty) * 0.1f;
                int csMin = Mathf.FloorToInt(csThreshold) + 1;
                if (csMin <= successMax)
                {
                    csMin = Mathf.Max(csMin, successMin);
                    outcomes.Add(new SkillCheckOutcomeChance(OptionOutcomeDefOf.CriticalSuccess, csMin, successMax));
                    plainSuccessMax = csMin - 1;
                }
            }

            // Plain Success: remaining success range
            if (plainSuccessMax >= successMin)
            {
                outcomes.Add(new SkillCheckOutcomeChance(OptionOutcomeDefOf.Success, successMin, plainSuccessMax));
            }
        }

        // Sort from best to worst outcome
        outcomes.Sort((a, b) => b.Outcome.SuccessLevel.CompareTo(a.Outcome.SuccessLevel));

        return outcomes;
    }


    public void OnHoverStart()
    {
        foreach (StatDef stat in RelevantStats.Keys) Game.Instance.UI.HightlightStat(stat);
    }

    public void OnHoverEnd()
    {
        foreach (StatDef stat in RelevantStats.Keys) Game.Instance.UI.UnhighlightStat(stat);
    }
}
