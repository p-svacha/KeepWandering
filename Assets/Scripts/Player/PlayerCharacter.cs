using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Visual entity representing the player.
/// </summary>
public class PlayerCharacter
{
    public Game Game { get; private set; }
    public PlayerCharacterRenderer Renderer => PlayerCharacterRenderer.Instance;

    // Constants / Rules
    public const float BASE_BONE_REGEN_PER_DAY = 0.1f;
    public const float BASE_BLOOD_REGEN_PER_DAY = 0.1f;

    public const int POISON_COUNTDOWN_START = 20; // How many days to live when poisoning starts
    public const int REPOISON_STRENGTH = 5; // How much the poison countdown gets reduced when getting poisoned while already poisoned
    public const int EXTREME_POISONING_LIMIT = 3; // At how many days left the poisoning is considered extreme
    public const int MAJOR_POISONING_LIMIT = 10; // At how many days left the poisoning is considered major

    // State
    public List<HealthCondition> HealthConditions = new List<HealthCondition>();

    // Stats
    public Dictionary<StatDef, Stat> Stats { get; private set; }

    // Companions
    public bool HasDog;
    public bool HasParrot;

    public PlayerCharacter(Game game)
    {
        Game = game;

        // Init stats
        Stats = new Dictionary<StatDef, Stat>();
        foreach (StatDef stat in DefDatabase<StatDef>.AllDefs) Stats.Add(stat, new Stat(Game, this, stat));

        // Add instance of each need
        foreach (HealthConditionDef def in DefDatabase<HealthConditionDef>.AllDefs.Where(x => x.IsVital))
        {
            ApplyHealthCondition(def, "", hideInOutcomeNotes: true);
        }
    }

    /// <summary>
    /// Applies a health condition to the player. If the health condition has a maximum amount of instances, and that amount has been reached, the severity will be added to a random existing instance instead of creating a new one. Optionally, an initial severity can be provided. If no severity is provided, the default initial severity from the health condition definition will be used.
    /// </summary>
    public HealthCondition ApplyHealthCondition(HealthConditionDef def, string source, float initialSeverity = -1f, bool hideInOutcomeNotes = false)
    {
        // Validation
        if (initialSeverity > def.MaxSeverity)
        {
            Debug.LogError($"Initial severity {initialSeverity} exceeds max severity {def.MaxSeverity} for health condition {def}. Clamping to max severity.");
            initialSeverity = def.MaxSeverity;
        }

        // Take base initial severity if no severity was provided
        if (initialSeverity < 0) initialSeverity = def.DefaultInitialSeverity;

        // Check if we can apply this health condition (max instances)
        int currentAmount = GetHealthConditionAmount(def);
        bool maxInstancesReached = currentAmount >= def.MaxInstances;
        if (def.IsWound) maxInstancesReached = Renderer.GetUnusedWoundRenderer() == null; // If it's a wound, check if we have an unused renderer instead of checking max instances§

        // Haven't reached max amount => new instance
        if (!maxInstancesReached)
        {
            HealthCondition newHC = (HealthCondition)System.Activator.CreateInstance(def.HealthConditionClass);
            newHC.Init(this, def, initialSeverity);
            HealthConditions.Add(newHC);
            if (!string.IsNullOrEmpty(source)) newHC.Source.Add(source);

            if (!hideInOutcomeNotes) Game.HealthConditionsAddedSinceLastStep.Add(newHC);
            return newHC;
        }

        // Max amount reached => add severity to random existing instance
        else
        {
            if (initialSeverity == 0) return null; // No severity to add, so do nothing

            // Get random existing instance
            List<HealthCondition> existingInstances = HealthConditions.Where(hc => hc.Def == def).ToList();
            if (existingInstances.Count == 0)
            {
                Debug.LogWarning($"Max instances reached for health condition {def}, but no existing instances found, aborting. (This can happen for wounds, if all wound spots are occupied by wounds with another type.");
                return null;
            }

            HealthCondition chosenInstance = existingInstances.RandomElement();
            Game.ModifyHealthConditionSeverity(chosenInstance, initialSeverity, hideInOutcomeNotes);
            if (!string.IsNullOrEmpty(source)) chosenInstance.Source.Add(source);
            return null;
        }
    }

    public void RemoveHealthCondition(HealthCondition condition)
    {
        if (condition.Def.IsVital) throw new System.Exception($"Cannot remove permanent health condition {condition.Def}");

        Debug.Log($"Removing health condition {condition.Def} from player.");
        HealthConditions.Remove(condition);
        Game.HealthConditionsRemovedSinceLastStep.Add(condition);
    }

    /// <summary>
    /// Performs all events that happen during the night and adds them to the morning report.
    /// </summary>
    public void OnEndDay(Game game, MorningReport morningReport)
    {
        // Health conditions
        List<HealthCondition> existingHealthConditions = new List<HealthCondition>(HealthConditions); // Copy list in case it gets modified during the loop
        foreach (HealthCondition hc in existingHealthConditions) hc.ExecuteEndDayEffect(morningReport);

        // New health conditions applied by existing health conditions
        foreach(HealthCondition hc in existingHealthConditions)
        {
            List<(HealthConditionDef Condition, float Chance)> newHcChances = hc.GetCurrentAppliedHealthConditions();
            foreach ((HealthConditionDef condition, float chance) in newHcChances)
            {
                if (Random.value < chance)
                {
                    ApplyHealthCondition(condition, $"{hc.Def.Label}");
                    morningReport.AddNightEvent($"You developed {condition.Label} due to {hc.Def.Label}.");
                }
            }
        }
    }

    public void ModifyHunger(float value) => Game.ModifyHealthConditionSeverity(Hunger, value);
    public void ModifyThirst(float value) => Game.ModifyHealthConditionSeverity(Thirst, value);

    public void ApplyBloodLoss(float severity, string source) => ApplyHealthCondition(HealthConditionDefOf.BloodLoss, source, severity);

    public void ApplyRandomFracture(float severity, string source)
    {
        if (Random.value < 0.5f) ApplyLegFracture(severity, source);
        else ApplyArmFracture(severity, source);
    }
    public void ApplyLegFracture(float severity, string source)
    {
        bool isRightLeg = Random.value < 0.5f;

        // If that side already has a fracture, add severity to it instead of applying a new one
        List<HealthCondition> existingFractures = HealthConditions.Where(hc => hc.Def == HealthConditionDefOf.LegFracture).ToList();
        foreach (HC_Fracture existingFracture in existingFractures)
        {
            if (existingFracture.IsRightSide == isRightLeg)
            {
                Game.ModifyHealthConditionSeverity(existingFracture, severity);
                return;
            }
        }

        // If no existing fracture on that side, apply new fracture
        HC_Fracture newFracture = (HC_Fracture)ApplyHealthCondition(HealthConditionDefOf.LegFracture, source, severity);
        newFracture.SetSide(isRightLeg);
    }
    public void ApplyArmFracture(float severity, string source)
    {
        bool isRightArm = Random.value < 0.5f;

        // If that side already has a fracture, add severity to it instead of applying a new one
        List<HealthCondition> existingFractures = HealthConditions.Where(hc => hc.Def == HealthConditionDefOf.ArmFracture).ToList();
        foreach (HC_Fracture existingFracture in existingFractures)
        {
            if (existingFracture.IsRightSide == isRightArm)
            {
                Game.ModifyHealthConditionSeverity(existingFracture, severity);
                return;
            }
        }

        // If no existing fracture on that side, apply new fracture
        HC_Fracture newFracture = (HC_Fracture)ApplyHealthCondition(HealthConditionDefOf.ArmFracture, source, severity);
        newFracture.SetSide(isRightArm);
    }


    public void AddWound(HealthConditionDef woundDef, string source)
    {
        WoundRenderer woundRenderer = Renderer.GetUnusedWoundRenderer();
        if (woundRenderer == null)
        {
            Debug.LogWarning($"No unused wound renderer available. Cannot add wound {woundDef.Label}.");
            return;
        }

        Wound wound = (Wound)ApplyHealthCondition(woundDef, source);
        if (wound != null)
        {
            woundRenderer.SetWound(wound);
            wound.SetRenderer(woundRenderer);
        }
    }

    public void BandageWound(Wound wound)
    {
        wound.Bandage();
        wound.Renderer.Refresh();
    }
    public void TreatInfection(Wound wound)
    {
        wound.Treat();
        wound.Renderer.Refresh();
    }

    /// <summary>
    /// Reduces the severity of a random negative health condition by the specified amount, without fully healing it.
    /// </summary>
    public void ReduceRandomNegativeHcSeverity(float amount)
    {
        if (amount <= 0) return;

        List<HealthCondition> candidates = HealthConditions.Where(hc => hc.IsNegative && !hc.IsSimpleBinaryCondition()).ToList();
        if (candidates.Count == 0) return;

        HealthCondition hc = candidates.RandomElement();
        Game.ModifyHealthConditionSeverity(hc, amount, avoidFullHeal: true);
    }

    public void AddDog()
    {
        HasDog = true;
    }
    public void RemoveDog()
    {
        HasDog = false;
    }

    public void AddParrot()
    {
        HasParrot = true;
    }
    public void RemoveParrot()
    {
        HasParrot = false;
    }

    #region Getters

    // Stats
    public int GetStatValue(StatDef statDef) => Stats[statDef].GetValue();
    public int Morale => GetStatValue(StatDefOf.Morale);


    // Health conditions
    public int GetHealthConditionAmount(HealthConditionDef def) => HealthConditions.Count(hc => hc.Def == def);

    public HealthCondition Hunger => HealthConditions.First(hc => hc.Def == HealthConditionDefOf.Hunger);
    public bool IsWellFed => Hunger.ActiveStageIndex == 0;
    public bool IsVeryHungry => Hunger.ActiveStageIndex >= 3;

    public HealthCondition Thirst => HealthConditions.First(hc => hc.Def == HealthConditionDefOf.Thirst);
    public HealthCondition Bloodloss => HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.BloodLoss);

    public HC_Fracture RightArmFracture => (HC_Fracture)HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.ArmFracture && ((HC_Fracture)hc).IsRightSide);
    public HC_Fracture LeftArmFracture => (HC_Fracture)HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.ArmFracture && !((HC_Fracture)hc).IsRightSide);
    public HC_Fracture RightLegFracture => (HC_Fracture)HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.LegFracture && ((HC_Fracture)hc).IsRightSide);
    public HC_Fracture LeftLegFracture => (HC_Fracture)HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.LegFracture && !((HC_Fracture)hc).IsRightSide);

    // Wounds
    public List<Wound> Wounds => HealthConditions.Where(hc => hc is Wound w).Select(hc => (Wound)hc).ToList();
    public List<Wound> BandagableWounds => Wounds.Where(w => !w.IsBandaged).ToList();
    public List<Wound> TreatableWounds => Wounds.Where(w => w.IsInfected && !w.IsTreated).ToList();

    public List<HC_BruiseWound> BruiseWounds => Wounds.Where(w => w is HC_BruiseWound).Select(w => (HC_BruiseWound)w).ToList();
    public List<HC_BruiseWound> UnbandagedBruiseWounds => BruiseWounds.Where(w => !w.IsBandaged).ToList();

    #endregion
}
