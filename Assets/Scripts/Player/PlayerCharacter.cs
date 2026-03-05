using System.Collections;
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
    public const float BASE_NUTRITION_DROP_PER_DAY = 1f;
    public const float BASE_HYDRATION_DROP_PER_DAY = 1f;

    public const float BASE_BONE_REGEN_PER_DAY = 0.1f;
    public const float BASE_BLOOD_REGEN_PER_DAY = 0.1f;

    public const float BRUISE_WOUND_BONE_DAMAGE = 0.3f;
    public const float CUT_WOUND_BLEED_PER_DAY = 0.1f;

    public const int POISON_COUNTDOWN_START = 20; // How many days to live when poisoning starts
    public const int REPOISON_STRENGTH = 5; // How much the poison countdown gets reduced when getting poisoned while already posioned
    public const int EXTREME_POISONING_LIMIT = 3; // At how many days left the poisoning is considered extreme
    public const int MAJOR_POISONING_LIMIT = 10; // At how many days left the poisoning is considered major

    // State
    public Dictionary<HealthConditionDef, HealthCondition> PermanentConditions;
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

        // Add instance of each permanent health condition
        PermanentConditions = new Dictionary<HealthConditionDef, HealthCondition>();
        foreach(HealthConditionDef def in DefDatabase<HealthConditionDef>.AllDefs.Where(x => x.IsPermanent))
        {
            HealthCondition newHC = ApplyHealthCondition(def);
            PermanentConditions.Add(def, newHC);
        }
    }

    private HealthCondition ApplyHealthCondition(HealthConditionDef def)
    {
        HealthCondition newHC = (HealthCondition)System.Activator.CreateInstance(def.HealthConditionClass);
        newHC.Init(this, def);
        HealthConditions.Add(newHC);
        return newHC;
    }

    public void RemoveHealthCondition(HealthCondition condition)
    {
        if (condition.Def.IsPermanent) throw new System.Exception($"Cannot remove permanent health condition {condition.Def}");

        HealthConditions.Remove(condition);
    }

    /// <summary>
    /// Performs all events that happen during the night and adds them to the morning report.
    /// </summary>
    public void OnEndDay(Game game, MorningReport morningReport)
    {
        // Health conditions
        List<HealthCondition> healthConditions = new List<HealthCondition>(HealthConditions); // Copy list in case it gets modified during the loop
        foreach (HealthCondition hc in healthConditions) hc.OnEndDay(game, morningReport);
    }

    public void ModifyNutrition(float value) => Hunger.ModifyNutrition(value);
    public void ModifyHydration(float value) => Thirst.ModifyHydration(value);
    public void ModifyLegBoneHealth(float value) => LegFracture.ModifyBoneHealth(value);
    public void ModifyBloodAmount(float value) => BloodLoss.ModifyBloodAmount(value);

    public Wound AddWound(HealthConditionDef woundDef)
    {
        Wound wound = (Wound)ApplyHealthCondition(woundDef);
        WoundRenderer renderer = Renderer.GetUnusedWoundRenderer(woundDef);
        renderer.SetWound(wound);
        wound.SetRenderer(renderer);
        return wound;
    }

    public void RemoveWound(Wound wound)
    {
        wound.Renderer.SetWound(null);
        RemoveHealthCondition(wound);
    }

    public void TendWound(Wound wound)
    {
        wound.Tend(Game);
    }
    public void HealInfection(Wound wound)
    {
        wound.HealInfection(Game);
    }

    public void ApplyPoison() => Poison.ApplyPoison();
    public void HealPoison() => Poison.HealPoison();

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
    public List<HealthCondition> ActiveHealthConditions => HealthConditions.Where(hc => hc.IsActive).ToList();

    // Permanent conditions
    public int GetHealthConditionAmount(HealthConditionDef def) => ActiveHealthConditions.Count(hc => hc.Def == def);
    public HC_Hunger Hunger => (HC_Hunger)PermanentConditions[HealthConditionDefOf.Hunger];
    public HC_Thirst Thirst => (HC_Thirst)PermanentConditions[HealthConditionDefOf.Thirst];
    public HC_BloodLoss BloodLoss => (HC_BloodLoss)PermanentConditions[HealthConditionDefOf.BloodLoss];
    public HC_LegFracture LegFracture => (HC_LegFracture)PermanentConditions[HealthConditionDefOf.LegFracture];
    public HC_ArmFracture ArmFracture => (HC_ArmFracture)PermanentConditions[HealthConditionDefOf.ArmFracture];
    public HC_Poison Poison => (HC_Poison)PermanentConditions[HealthConditionDefOf.Poison];

    // Wounds
    public List<Wound> Wounds => HealthConditions.Where(hc => hc.IsActive && hc is Wound w).Select(hc => (Wound)hc).ToList();
    public List<Wound> TendableWounds => Wounds.Where(w => !w.IsTended).ToList();
    public List<Wound> InfectedWounds => Wounds.Where(w => w.IsInfected).ToList();

    public List<HC_BruiseWound> BruiseWounds => Wounds.Where(w => w is HC_BruiseWound).Select(w => (HC_BruiseWound)w).ToList();
    public List<HC_BruiseWound> UntendedBruiseWounds => BruiseWounds.Where(w => !w.IsTended).ToList();
    public bool HasUntendedBruiseWound => UntendedBruiseWounds.Count > 0;

    public List<HC_CutWound> CutWounds => Wounds.Where(w => w is HC_CutWound).Select(w => (HC_CutWound)w).ToList();
    public List<HC_CutWound> UntendedCutWounds => CutWounds.Where(w => !w.IsTended).ToList();
    public bool HasUntendedCutWound => UntendedCutWounds.Count > 0;

    #endregion
}
