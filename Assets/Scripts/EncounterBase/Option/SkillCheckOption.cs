using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCheckOption : EncounterOption
{
    public static int MIN_DIFFICULTY = 5;
    public static int MAX_DIFFICULTY = 200;

    public override EncounterOptionType Type => EncounterOptionType.SkillCheck;

    /// <summary>
    /// Base difficulty of the encounter step option, before any modifiers apply.
    /// </summary>
    public int Difficulty { get; init; }

    /// <summary>
    /// The stats that are relevant for performing this encounter step option, along with the modifier of how much it affects the difficulty of the encounter step option.
    /// </summary>
    public Dictionary<StatDef, int> RelevantStats { get; init; } = new Dictionary<StatDef, int>();
    public static int MAX_RELEVANT_START_FACTOR = 5;

    /// <summary>
    /// Fixed difficulty modifiers that apply to this encounter step option, with a label for each modifier to be displayed in the UI.
    /// </summary>
    public Dictionary<string, int> FixedDifficultyModifiers { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Modifiers based on the biome the encounter is taking place in.
    /// </summary>
    public Dictionary<BiomeDef, int> BiomeDifficultyModifiers { get; init; } = new Dictionary<BiomeDef, int>();

    /// <summary>
    /// Function that gets executed when choosing this option. Handles the logic of the outcome and returns the text displayed on the next step. The function takes in the outcome of the skill check as a parameter, so different outcomes can lead to different next steps.
    /// </summary>
    public Func<OptionOutcomeDef, string> Action { get; init; }

    /// <summary>
    /// The raw 1–100 roll from the most recent execution of this option. Used by the roll animation sequence to know exactly where the marker should land.
    /// </summary>
    public int LastRoll { get; private set; }

    /// <summary>
    /// Outcome of the most recent roll for this option.
    /// </summary>
    public OptionOutcomeDef PendingOutcome { get; private set; }

    /// <summary>
    /// The sequence of roll modifiers applied by health conditions during the most recent roll, in the order they triggered, each paired with the roll value the marker should jump to.
    /// </summary>
    public List<(HealthCondition Condition, int NewRollValue)> AppliedRollModifiers { get; private set; } = new();

    public bool CanPartiallySucceed { get; init; } = true;
    public bool CanCriticallySucceed { get; init; } = true;
    public bool CanCriticallyFail { get; init; } = true;

    public override void Init()
    {
        base.Init();

        // Validate
        if (Action == null) throw new Exception($"Actions function cannot be null for SkillCheckOption '{Text}'.");
        if (Difficulty <= MIN_DIFFICULTY) throw new Exception($"Base difficulty must be greater than {MIN_DIFFICULTY} for SkillCheckOption '{Text}'.");
        foreach(var statEntry in RelevantStats)
        {
            if (statEntry.Value < 0) throw new Exception($"Relevant stat modifier for '{statEntry.Key.LabelCapWord}' must be non-negative for SkillCheckOption '{Text}'.");
            if (statEntry.Value > MAX_RELEVANT_START_FACTOR) throw new Exception($"Relevant stat modifier for '{statEntry.Key.LabelCapWord}' must be less than or equal to {MAX_RELEVANT_START_FACTOR} for SkillCheckOption '{Text}'.");
        }
    }


    public override string Execute(out OptionOutcomeDef outcome)
    {
        if (PendingOutcome == null) throw new System.Exception($"No pending outcome for SkillCheckOption '{Text}'. RollOutcome must be called before Execute.");

        outcome = PendingOutcome;
        PendingOutcome = null;
        return Action.Invoke(outcome);
    }

    /// <summary>
    /// Returns the calculated difficulty of this option, taking into account all modifiers.
    /// </summary>
    public int GetDifficultyValue()
    {
        int difficulty = Difficulty;

        foreach (var modifier in GetDifficultyModifiers()) difficulty += modifier.Value;

        // Clamp
        difficulty = Mathf.Clamp(difficulty, MIN_DIFFICULTY, MAX_DIFFICULTY);

        return difficulty;
    }

    public Dictionary<string, int> GetDifficultyModifiers()
    {
        Dictionary<string, int> modifiers = new Dictionary<string, int>();

        // Fixed moidifers
        foreach (var modifier in FixedDifficultyModifiers)
        {
            if (modifier.Value != 0) modifiers.Add(modifier.Key, modifier.Value);
        }

        // Player stat modifiers
        foreach (var statEntry in RelevantStats)
        {
            int statValue = Game.Instance.Player.GetStatValue(statEntry.Key);
            float factor = statEntry.Value;
            int modifierAmount = -(int)(statValue * factor);
            string label = statEntry.Key.LabelCapWord;
            if(modifierAmount != 1f) label += $" (x{factor})";
            if (modifierAmount != 0) modifiers.Add(label, modifierAmount);
        }

        // Morale modifier
        int moraleValue = Game.Instance.Player.Morale;
        if (moraleValue != 0) modifiers.Add("Morale", -moraleValue);

        // Biome modifier
        BiomeDef biome = Game.Instance.CurrentPosition.Biome;
        if (BiomeDifficultyModifiers.TryGetValue(biome, out int biomeModifier) && biomeModifier != 0)
        {
            modifiers.Add($"Being in {biome.LabelCapWord}", biomeModifier);
        }

        // Item slots (only tag-slots that are filled affect difficulty)
        foreach (ItemSlot slot in ItemSlots)
        {
            if (slot.Tag != null && slot.IsFilled)
            {
                int modifierAmount = slot.GetDifficultyReduction(slot.FilledItem.Def);
                if (modifierAmount != 0) modifiers.Add($"Using {slot.FilledItem.Def.LabelCapWord}", -modifierAmount);
            }
        }

        return modifiers;
    }

    /// <summary>
    /// Rolls and returns the outcome for this skill check without applying any effects (Action is
    /// not invoked). Call this immediately on selection, before the roll animation plays. Execute()
    /// will later reuse this exact result rather than rolling again.
    /// </summary>
    public OptionOutcomeDef RollOutcome()
    {
        int difficulty = GetDifficultyValue();
        int roll = UnityEngine.Random.Range(1, 101);
        LastRoll = roll;

        // Precompute which health conditions modify this roll, and by how much, in sequence
        AppliedRollModifiers.Clear();
        int currentRoll = roll;
        foreach (HealthCondition hc in Game.Instance.Player.HealthConditions)
        {
            var modifier = hc.GetCurrentSkillCheckModifier();
            if (!modifier.HasValue) continue;

            if (UnityEngine.Random.value < modifier.Value.Chance)
            {
                currentRoll = Mathf.Clamp(currentRoll + modifier.Value.Modifier, 1, 100);
                AppliedRollModifiers.Add((hc, currentRoll));
            }
        }

        OptionOutcomeDef result = DetermineOutcome(currentRoll, difficulty);
        PendingOutcome = result;
        return result;
    }

    private OptionOutcomeDef DetermineOutcome(int roll, int difficulty)
    {
        if (roll > difficulty)
        {
            if (CanCriticallySucceed)
            {
                float criticalThreshold = 100f - (100f - difficulty) * 0.1f;
                if (roll > criticalThreshold) return OptionOutcomeDefOf.CriticalSuccess;
            }
            return OptionOutcomeDefOf.Success;
        }
        else
        {
            if (CanPartiallySucceed && roll > difficulty * 0.5f) return OptionOutcomeDefOf.PartialSuccess;
            if (CanCriticallyFail && roll < difficulty * 0.1f) return OptionOutcomeDefOf.CriticalFailure;
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
}
