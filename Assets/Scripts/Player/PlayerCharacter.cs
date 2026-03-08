using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Visual entity representing the player.
/// </summary>
public class PlayerCharacter
{
    public Game Game { get; private set; }
    public PlayerCharacterRenderer Renderer => PlayerCharacterRenderer.Instance;

    // Constants / Rules
    public const float HUNGER_INCREASE_PER_DAY = 1f;
    public const float THIRST_INCREASE_PER_DAY = 1f;

    public const float BASE_BONE_REGEN_PER_DAY = 0.1f;
    public const float BASE_BLOOD_REGEN_PER_DAY = 0.1f;

    public const int POISON_COUNTDOWN_START = 20; // How many days to live when poisoning starts
    public const int REPOISON_STRENGTH = 5; // How much the poison countdown gets reduced when getting poisoned while already posioned
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
        foreach(HealthConditionDef def in DefDatabase<HealthConditionDef>.AllDefs.Where(x => x.IsNeed))
        {
            ApplyHealthCondition(def);
        }
    }

    private HealthCondition ApplyHealthCondition(HealthConditionDef def, float initialSeverity = -1f)
    {
        // Take base initial severity if no severity was provided
        if (initialSeverity < 0) initialSeverity = def.InitialSeverity;

        // Check if we can apply this health condition (max instances)
        int currentAmount = GetHealthConditionAmount(def);

        // Haven't reached max amount => new instance
        if(currentAmount < def.MaxInstances)
        {
            HealthCondition newHC = (HealthCondition)System.Activator.CreateInstance(def.HealthConditionClass);
            newHC.Init(this, def, initialSeverity);
            HealthConditions.Add(newHC);
            return newHC;
        }

        // Max amount reached => add severity to random existing instance
        else
        {
            if (initialSeverity == 0) return null; // No severity to add, so do nothing

            // Get random existing instance
            List<HealthCondition> existingInstances = HealthConditions.Where(hc => hc.Def == def).ToList();
            HealthCondition chosenInstance = existingInstances.RandomElement();
            chosenInstance.ModifySeverity(initialSeverity);
            return null;
        }
    }

    public void RemoveHealthCondition(HealthCondition condition)
    {
        if (condition.Def.IsNeed) throw new System.Exception($"Cannot remove permanent health condition {condition.Def}");

        Debug.Log($"Removing health condition {condition.Def} from player.");
        HealthConditions.Remove(condition);
    }

    /// <summary>
    /// Performs all events that happen during the night and adds them to the morning report.
    /// </summary>
    public void OnEndDay(Game game, MorningReport morningReport)
    {
        // Health conditions
        List<HealthCondition> healthConditions = new List<HealthCondition>(HealthConditions); // Copy list in case it gets modified during the loop
        foreach (HealthCondition hc in healthConditions) hc.ExecuteEndDayEffect(morningReport);
    }

    public void ModifyNutrition(float value) => Hunger.ModifySeverity(value);
    public void ModifyHydration(float value) => Thirst.ModifySeverity(value);

    public void ApplyBloodLoss(float severity) => ApplyHealthCondition(HealthConditionDefOf.BloodLoss, severity);

    public void ApplyRandomFracture(float severity)
    {
        if (Random.value < 0.5f) ApplyLegFracture(severity);
        else ApplyArmFracture(severity);
    }
    public void ApplyLegFracture(float severity)
    {
        bool isRightLeg = Random.value < 0.5f;

        // If that side already has a fracture, add severity to it instead of applying a new one
        List<HealthCondition> existingFractures = HealthConditions.Where(hc => hc.Def == HealthConditionDefOf.LegFracture).ToList();
        foreach (HC_LegFracture existingFracture in existingFractures)
        {
            if (existingFracture.IsRightLeg == isRightLeg)
            {
                existingFracture.ModifySeverity(severity);
                return;
            }
        }

        // If no existing fracture on that side, apply new fracture
        HC_LegFracture newFracture = (HC_LegFracture)ApplyHealthCondition(HealthConditionDefOf.LegFracture, severity);
        newFracture.SetSide(isRightLeg);
    }
    public void ApplyArmFracture(float severity)
    {
        bool isRightArm = Random.value < 0.5f;

        // If that side already has a fracture, add severity to it instead of applying a new one
        List<HealthCondition> existingFractures = HealthConditions.Where(hc => hc.Def == HealthConditionDefOf.ArmFracture).ToList();
        foreach (HC_ArmFracture existingFracture in existingFractures)
        {
            if (existingFracture.IsRightArm == isRightArm)
            {
                existingFracture.ModifySeverity(severity);
                return;
            }
        }

        // If no existing fracture on that side, apply new fracture
        HC_ArmFracture newFracture = (HC_ArmFracture)ApplyHealthCondition(HealthConditionDefOf.ArmFracture, severity);
        newFracture.SetSide(isRightArm);
    }


    public Wound AddWound(HealthConditionDef woundDef)
    {
        Wound wound = (Wound)ApplyHealthCondition(woundDef);
        if (wound != null)
        {
            WoundRenderer renderer = Renderer.GetUnusedWoundRenderer(woundDef);
            renderer.SetWound(wound);
            wound.SetRenderer(renderer);
        }
        return wound;
    }

    public void TendWound(Wound wound)
    {
        wound.Tend();
        wound.Renderer.Refresh();
    }
    public void TreatWound(Wound wound)
    {
        wound.Treat();
        wound.Renderer.Refresh();
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

    public HC_Hunger Hunger => (HC_Hunger)HealthConditions.First(hc => hc.Def == HealthConditionDefOf.Hunger);
    public HC_Thirst Thirst => (HC_Thirst)HealthConditions.First(hc => hc.Def == HealthConditionDefOf.Thirst);

    // Wounds
    public List<Wound> Wounds => HealthConditions.Where(hc => hc is Wound w).Select(hc => (Wound)hc).ToList();
    public List<Wound> TendableWounds => Wounds.Where(w => !w.IsTended).ToList();
    public List<Wound> TreatableWounds => Wounds.Where(w => w.IsInfected && !w.IsTreated).ToList();

    public List<HC_BruiseWound> BruiseWounds => Wounds.Where(w => w is HC_BruiseWound).Select(w => (HC_BruiseWound)w).ToList();
    public List<HC_BruiseWound> UntendedBruiseWounds => BruiseWounds.Where(w => !w.IsTended).ToList();
    public bool HasUntendedBruiseWound => UntendedBruiseWounds.Count > 0;

    public List<HC_CutWound> CutWounds => Wounds.Where(w => w is HC_CutWound).Select(w => (HC_CutWound)w).ToList();
    public List<HC_CutWound> UntendedCutWounds => CutWounds.Where(w => !w.IsTended).ToList();
    public bool HasUntendedCutWound => UntendedCutWounds.Count > 0;

    #endregion
}
